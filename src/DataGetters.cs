using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using VTOLAPI;

namespace vtolvrtelemetry
{
    public static class CollectData
    {
        public static void GetData()
        {
            double num = 0.0;
            try
            {
                Actor playerActor = FlightSceneManager.instance.playerActor;
                GameObject playersVehicleGameObject = VTAPI.GetPlayersVehicleGameObject();
                CollectData.Data.Clear();
                num = 1.0;
                Data.AddRange(BitConverter.GetBytes(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds));
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.pilotAccel.x));
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.pilotAccel.y));
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.pilotAccel.z));
                num = 1.1;
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.playerGs));
                num = 1.2;
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.rb.drag));
                num = 1.3;
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.rb.mass));
                num = 1.4;
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.airspeed));
                num = 1.5;
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.verticalSpeed));
                num = 1.6;
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.altitudeASL));
                num = 1.7;
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.heading));
                num = 1.8;
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.pitch));
                num = 1.9;
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.roll));
                num = 1.11;
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.aoa));
                num = 2.0;
                CollectData.Data.AddRange(BitConverter.GetBytes(CollectData.GetFlaps(playersVehicleGameObject)));
                num = 3.0;
                CollectData.Data.AddRange(BitConverter.GetBytes(CollectData.GetBrakes(playersVehicleGameObject)));
                num = 4.0;
                CollectData.Data.AddRange(BitConverter.GetBytes(CollectData.GetFuelLevel(playersVehicleGameObject)));
                num = 5.0;
                CollectData.Data.AddRange(BitConverter.GetBytes(CollectData.GetGear(playersVehicleGameObject)));
                num = 6.0;
                CollectData.Data.AddRange(BitConverter.GetBytes(CollectData.GetCanopy(playersVehicleGameObject)));
                num = 7.0;
                CollectData.Data.AddRange(BitConverter.GetBytes(CollectData.GetHook(playersVehicleGameObject)));
                num = 8.0;
                CollectData.Data.AddRange(BitConverter.GetBytes(CollectData.GetEjectionState(playersVehicleGameObject)));
                num = 9.0;
                CollectData.Data.AddRange(BitConverter.GetBytes(CollectData.GetStall(playersVehicleGameObject)));
                num = 10.0;
                CollectData.Data.AddRange(BitConverter.GetBytes(playerActor.flightInfo.isLanded));
                num = 11.0;
                CollectData.Data.AddRange(BitConverter.GetBytes(CollectData.GetAverageSuspension(playersVehicleGameObject)));
                num = 12.0;
                CollectData.Data.AddRange(BitConverter.GetBytes(CollectData.GetEnginesNumber(playersVehicleGameObject)));
                foreach (float value in CollectData.GetEnginesRPM(playersVehicleGameObject))
                {
                    CollectData.Data.AddRange(BitConverter.GetBytes(value));
                }
                foreach (bool value2 in CollectData.GetEnginesAfterburner(playersVehicleGameObject))
                {
                    CollectData.Data.AddRange(BitConverter.GetBytes(value2));
                }
                CollectData.Data.AddRange(BitConverter.GetBytes(playersVehicleGameObject.name.Trim().Length));
                CollectData.Data.AddRange(Encoding.Default.GetBytes(playersVehicleGameObject.name.Trim()));
            }
            catch (Exception ex)
            {
                LogData.AddEntry("Error getting telemetry data at #" + num.ToString() + ":" + ex.ToString());
            }
        }

        public static bool GetHook(GameObject vehicle)
        {
            bool result;
            try
            {
                result = vehicle.GetComponentInChildren<Tailhook>().isDeployed;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public static float GetFlaps(GameObject vehicle)
        {
            float result;
            try
            {
                result = vehicle.GetComponentInChildren<AeroController>().flaps;
            }
            catch
            {
                result = 0f;
            }
            return result;
        }

        public static float GetBrakes(GameObject vehicle)
        {
            float result;
            try
            {
                result = vehicle.GetComponentInChildren<AeroController>().brake;
            }
            catch
            {
                result = 0f;
            }
            return result;
        }
        public static float GetCanopy(GameObject vehicle)
        {
            float result;
            try
            {
                CanopyAnimator componentInChildren = vehicle.GetComponentInChildren<CanopyAnimator>();
                result = (componentInChildren.isBroken ? 5f : ((float)componentInChildren.targetState));
            }
            catch
            {
                result = 0f;
            }
            return result;
        }
        public static float GetGear(GameObject vehicle)
        {
            float result;
            try
            {
                result = vehicle.GetComponentInChildren<GearAnimator>().transitionTime;
            }
            catch
            {
                result = 0f;
            }
            return result;
        }
        public static float GetAverageSuspension(GameObject vehicle)
        {
            float result;
            try
            {
                GearAnimator componentInChildren = vehicle.GetComponentInChildren<GearAnimator>();
                float num = 0f;
                int num2 = componentInChildren.gearLegs.Count<GearAnimator.AnimatedGearLeg>();
                if (num2 > 0)
                {
                    foreach (GearAnimator.AnimatedGearLeg animatedGearLeg in componentInChildren.gearLegs)
                    {
                        num += animatedGearLeg.suspension.suspensionDistance;
                    }
                    num /= (float)num2;
                }
                result = num;
            }
            catch
            {
                result = 0f;
            }
            return result;
        }

        public static bool GetEjectionState(GameObject vehicle)
        {
            bool result;
            try
            {
                result = vehicle.GetComponentInChildren<EjectionSeat>().ejected;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public static float GetFuelLevel(GameObject vehicle)
        {
            float result;
            try
            {
                result = vehicle.GetComponentInChildren<FuelTank>().totalFuel;
            }
            catch
            {
                result = 0f;
            }
            return result;
        }

        public static bool GetStall(GameObject vehicle)
        {
            bool result;
            try
            {
                result = vehicle.GetComponentInChildren<HUDStallWarning>().isActiveAndEnabled;
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public static int GetEnginesNumber(GameObject vehicle)
        {
            int result;
            try
            {
                result = vehicle.GetComponentsInChildren<ModuleEngine>().Count<ModuleEngine>();
            }
            catch
            {
                result = 0;
            }
            return result;
        }

        public static List<float> GetEnginesRPM(GameObject vehicle)
        {
            List<float> list = new List<float>();
            try
            {
                int num = 1;
                foreach (ModuleEngine moduleEngine in vehicle.GetComponentsInChildren<ModuleEngine>())
                {
                    list.Add(moduleEngine.displayedRPM);
                    num++;
                }
            }
            catch
            {
            }
            return list;
        }

        public static List<bool> GetEnginesAfterburner(GameObject vehicle)
        {
            List<bool> list = new List<bool>();
            try
            {
                int num = 1;
                foreach (ModuleEngine moduleEngine in vehicle.GetComponentsInChildren<ModuleEngine>())
                {
                    list.Add(moduleEngine.afterburner);
                    num++;
                }
            }
            catch
            {
            }
            return list;
        }

        public static void SendTo(UdpClient udpClient)
        {
            try
            {
                int num = Marshal.SizeOf<List<byte>>(CollectData.Data);
                byte[] array = new byte[num];
                IntPtr intPtr = IntPtr.Zero;
                try
                {
                    intPtr = Marshal.AllocHGlobal(num);
                    Marshal.StructureToPtr<List<byte>>(CollectData.Data, intPtr, true);
                    Marshal.Copy(intPtr, array, 0, num);
                }
                finally
                {
                    Marshal.FreeHGlobal(intPtr);
                }
                udpClient.Send(array, array.Length);
            }
            catch (Exception ex)
            {
                LogData.AddEntry("Error sending telemetry data: " + ex.ToString());
            }
        }
        public static List<byte> Data = new List<byte>();
    }
}
