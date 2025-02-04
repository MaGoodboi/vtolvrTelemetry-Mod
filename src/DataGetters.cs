using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace vtolvrtelemetry
{
    public class DataGetters
    {
        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;

        public DataGetters(VtolvrTelemetry mod)
        {
            udpClient = new UdpClient();
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(mod.receiverIp), mod.receiverPort);
        }

        public void GetData()
        {
            try
            {
                // Get the player's aircraft
                var playerActor = FlightSceneManager.instance.playerActor;
                if (playerActor == null)
                {
                    Debug.LogWarning("Player aircraft not found. Skipping telemetry update.");
                    return;
                }

                // Get flight data
                var flightInfo = playerActor.flightInfo;

                // Extract movement data
                float yaw = playerActor.transform.eulerAngles.y;
                float pitch = playerActor.transform.eulerAngles.x;
                float roll = playerActor.transform.eulerAngles.z;

                // Acceleration values
                float xAccel = flightInfo.acceleration.x;
                float yAccel = flightInfo.acceleration.y;
                float zAccel = flightInfo.acceleration.z;
                float playerGs = flightInfo.playerGs;

                // 🔥 Detect weapon firing for extra force effects
                bool gunFired = GetGunFiring(playerActor.gameObject);
                bool missileFired = GetMissileFiring(playerActor.gameObject);

                if (gunFired || missileFired)
                {
                    Debug.Log("Weapon fired! Adding recoil force to telemetry.");
                    xAccel += (gunFired ? -0.5f : 0) + (missileFired ? -0.3f : 0); // Simulated recoil push
                }

                // 🛬 Landing detection (adds bump effect)
                bool isLanded = IsAircraftLanded(playerActor.gameObject);
                if (isLanded)
                {
                    Debug.Log("Aircraft Landed! Adding touchdown effect.");
                    yAccel -= 2.0f; // Simulate sudden downward force upon landing
                }

                // 🛑 Tail Hook Deployment (Abrupt Stop Effect)
                bool tailHookDeployed = GetTailHookDeployed(playerActor.gameObject);
                if (tailHookDeployed)
                {
                    Debug.Log("Tail Hook Deployed! Simulating abrupt landing force.");
                    yAccel -= 3.0f; // Sudden stop effect
                }

                // 🚀 Launch Bar Activation (Simulates Catapult Acceleration)
                bool launchBarEngaged = GetLaunchBarEngaged(playerActor.gameObject);
                if (launchBarEngaged)
                {
                    Debug.Log("Launch Bar Engaged! Simulating catapult acceleration.");
                    xAccel += 5.0f; // Strong forward acceleration effect
                }

                // ✈️ Flaps (Affect Pitch Sensitivity)
                float flapAngle = GetFlapPosition(playerActor.gameObject);
                pitch += flapAngle * 5.0f; // Adjust pitch slightly based on flaps

                // 🛑 Brakes (Sudden Deceleration Effect)
                float brakeForce = GetBrakes(playerActor.gameObject);
                xAccel -= brakeForce * 3.0f; // Apply braking force

                // ⚠️ Stalling Effect (Small Random Vibrations)
                bool isStalling = IsAircraftStalling(playerActor.gameObject);
                if (isStalling)
                {
                    Debug.Log("Aircraft is stalling! Adding subtle shake.");
                    yAccel += UnityEngine.Random.Range(-0.2f, 0.2f); // Small shake effect
                }

                // 🎯 Missile Lock (Stress Vibration)
                bool missileLocked = IsMissileLocked(playerActor.gameObject);
                if (missileLocked)
                {
                    Debug.Log("Missile Lock Detected! Adding stress shake.");
                    xAccel += UnityEngine.Random.Range(-0.3f, 0.3f); // Subtle shaking
                }

                // Ensure Yaw is continuous (prevents sudden jumps)
                yaw = NormalizeYaw(yaw);

                // Format telemetry data
                string telemetryData = $"Yaw: {yaw:F2}, Pitch: {pitch:F2}, Roll: {roll:F2}, " +
                                       $"XAccel: {xAccel:F2}, YAccel: {yAccel:F2}, ZAccel: {zAccel:F2}, " +
                                       $"G-Force: {playerGs:F2}";

                // Send telemetry data
                byte[] data = Encoding.ASCII.GetBytes(telemetryData);
                udpClient.Send(data, data.Length, remoteEndPoint);
                Debug.Log($"Live telemetry sent: {telemetryData}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error retrieving telemetry data: {ex.Message}");
            }
        }

        // Helper Function to Normalize Yaw for Smooth 360° Rotation
        private float NormalizeYaw(float yaw)
        {
            if (yaw > 180f)
            {
                yaw -= 360f;
            }
            return yaw;
        }

        // 🔥 Detect Gun Firing
        public static bool GetGunFiring(GameObject vehicle)
        {
            try
            {
                WeaponManager weaponManager = vehicle.GetComponentInChildren<WeaponManager>();
                if (weaponManager == null)
                {
                    return false; // Safe return instead of throwing an error
                }
                return weaponManager.availableWeaponTypes.gun && weaponManager.isFiring;
            }
            catch { return false; }
        }

        // 🚀 Detect Missile Firing
        public static bool GetMissileFiring(GameObject vehicle)
        {
            try
            {
                WeaponManager weaponManager = vehicle.GetComponentInChildren<WeaponManager>();
                return (weaponManager.availableWeaponTypes.aam ||
                        weaponManager.availableWeaponTypes.agm ||
                        weaponManager.availableWeaponTypes.antirad ||
                        weaponManager.availableWeaponTypes.antiShip) && weaponManager.isFiring;
            }
            catch { return false; }
        }

        // 🛑 Detect Tail Hook Deployment
        public static bool GetTailHookDeployed(GameObject vehicle)
        {
            try
            {
                Tailhook hook = vehicle.GetComponentInChildren<Tailhook>();
                if (hook != null)
                {
                    bool deployed = false;
                    try
                    {
                        deployed = Traverse.Create(hook).Field("deployed").GetValue<bool>();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error accessing tail hook deployment state: {ex.Message}");
                    }
                    return deployed;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error detecting tail hook deployment: {ex.Message}");
                return false;
            }
        }

        // 🚀 Detect Launch Bar Activation
        public static bool GetLaunchBarEngaged(GameObject vehicle)
        {
            try
            {
                // Find the component that manages the launch bar (if it exists)
                Component launchBar = vehicle.GetComponentInChildren<Component>();
                if (launchBar != null)
                {
                    // Use HarmonyLib reflection to check if the launch bar is engaged
                    bool engaged = Traverse.Create(launchBar).Field("engaged").GetValue<bool>();
                    return engaged;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error detecting launch bar engagement: {ex.Message}");
                return false;
            }
        }

        // ✈️ Detect Flap Position
        public static float GetFlapPosition(GameObject vehicle)
        {
            try { return vehicle.GetComponentInChildren<AeroController>().flaps; }
            catch { return 0; }
        }

        // 🛑 Detect Brakes
        public static float GetBrakes(GameObject vehicle)
        {
            try { return vehicle.GetComponentInChildren<AeroController>().brake; }
            catch { return 0; }
        }

        // ⚠️ Detect Stall
        public static bool IsAircraftStalling(GameObject vehicle)
        {
            try
            {
                HUDStallWarning stallWarning = vehicle.GetComponentInChildren<HUDStallWarning>();
                if (stallWarning != null)
                {
                    // Use HarmonyLib to access the private "stalling" field
                    bool stalling = Traverse.Create(stallWarning).Field("stalling").GetValue<bool>();
                    return stalling;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error detecting stall status: {ex.Message}");
                return false;
            }
        }

        // 🎯 Detect Missile Lock
        public static bool IsMissileLocked(GameObject vehicle)
        {
            MissileDetector detector = vehicle.GetComponentInChildren<MissileDetector>();
            return detector != null && detector.missileDetected;
        }
        public static bool IsAircraftLanded(GameObject vehicle)
        {
            try
            {
                Actor actor = vehicle.GetComponent<Actor>();
                return actor.flightInfo.isLanded;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error Detecting landing: {ex.Message}");
                return false;
            }
        }
        public void Dispose()
        {
            udpClient?.Close();
            udpClient?.Dispose();
        }
    }
}
