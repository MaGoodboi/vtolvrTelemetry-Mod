using System;
using UnityEngine;
using Valve.Newtonsoft.Json;
using VTOLAPI;

namespace vtolvrtelemetry
{
    public class TelemetryManager
    {
        private Vector3 yawVRRotation = Vector3.zero; // Stores IMU motion cancellation data
        private float initialYawOffset = 0f; // Stores starting yaw offset
        private bool yawInitialized = false;

        public TelemetryManager() { }

        public string GetData()
        {
            try
            {
                // Get the player's aircraft (supports multiplayer)
                Actor playerActor = FlightSceneManager.instance.playerActor;
                if (playerActor == null)
                {
                    Support.WriteErrorLog("[Telemetry] No player aircraft detected.");
                    return null;
                }

                GameObject playersVehicleGameObject = VTAPI.GetPlayersVehicleGameObject();
                if (playersVehicleGameObject == null)
                {
                    Support.WriteErrorLog("[Telemetry] GetPlayersVehicleGameObject() returned null.");
                    return null;
                }

                VehicleMaster vehicleMaster = playersVehicleGameObject.GetComponent<VehicleMaster>();
                if (vehicleMaster == null)
                {
                    Support.WriteErrorLog("[Telemetry] VehicleMaster component missing.");
                    return null;
                }

                FlightInfo flightInfo = vehicleMaster.flightInfo;
                if (flightInfo == null)
                {
                    Support.WriteErrorLog("[Telemetry] FlightInfo component missing.");
                    return null;
                }

                // Get telemetry data
                Transform planeTransform = playersVehicleGameObject.transform;
                float yaw = 0f; // Disabled Yaw as the movement in VTOL VR is unpredictable
                float pitch = (planeTransform.rotation.eulerAngles.x + 180) % 360 - 180;
                float roll = -((planeTransform.rotation.eulerAngles.z + 180) % 360 - 180);

                float airspeed = flightInfo.indicatedAirspeed;
                float verticalSpeed = flightInfo.verticalSpeed;
                float aoa = flightInfo.aoa;

                // Format data as JSON (matching VtolPlugin expectations)
                string telemetryJson = JsonConvert.SerializeObject(new
                {
                    Vehicle = new
                    {
                        Heading = yaw,
                        Pitch = pitch,
                        Roll = roll,
                        Airspeed = airspeed,
                        VerticalSpeed = verticalSpeed,
                        AoA = aoa
                    }
                });

                // Log the output for debugging
                Support.WriteLog("[Telemetry] JSON Output: " + telemetryJson);

                return telemetryJson;  // ✅ Now correctly formatted
            }
            catch (Exception ex)
            {
                Support.WriteErrorLog("[Telemetry] Error getting telemetry data: " + ex.ToString());
                return null;
            }
        }

        public void UpdateYawVRIMU(Vector3 imuRotation)
        {
            yawVRRotation = imuRotation;
        }
    }
}
