using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Valve.Newtonsoft.Json;
using VTOLAPI;
using VtolPlugin;

namespace vtolvrtelemetry
{
    public class DataGetters
    {
        public vtolvrtelemetry dataLogger { get; set; }

        public DataGetters(vtolvrtelemetry dataLogger)
        {
            this.dataLogger = dataLogger;
        }

        public void GetData()
        {
            CollectData flightInfo = new CollectData();

            try
            {
                Actor playeractor = FlightSceneManager.instance.playerActor;
                GameObject currentVehicle = VTAPI.GetPlayersVehicleGameObject();

                flightInfo.unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                flightInfo.Physics.XAccel = (float)Math.Round(playeractor.flightInfo.acceleration.x, 2);
                flightInfo.Physics.YAccel = (float)Math.Round(playeractor.flightInfo.acceleration.y, 2);
                flightInfo.Physics.ZAccel = (float)Math.Round(playeractor.flightInfo.acceleration.z, 2);
                flightInfo.Physics.PlayerGs = Math.Round(playeractor.flightInfo.playerGs, 2).ToString();
                flightInfo.Vehicle.Heading = (float)Math.Round(playeractor.flightInfo.heading, 2);
                flightInfo.Vehicle.Pitch = (float)Math.Round(playeractor.flightInfo.pitch, 2);
                flightInfo.Vehicle.Roll = (float)Math.Round(playeractor.flightInfo.roll, 2);
                flightInfo.Vehicle.AoA = (float)Math.Round(playeractor.flightInfo.aoa, 2);
                flightInfo.Vehicle.Airspeed = (float)Math.Round(playeractor.flightInfo.airspeed, 2);
                flightInfo.Vehicle.VerticalSpeed = (float)Math.Round(playeractor.flightInfo.verticalSpeed, 2);
                flightInfo.Vehicle.AltitudeASL = (float)Math.Round(playeractor.flightInfo.altitudeASL, 2);
                flightInfo.Vehicle.Fuel.FuelDensity = DataGetters.getFuelDensity(currentVehicle);
                flightInfo.Vehicle.Fuel.FuelBurnRate = DataGetters.getFuelBurnRate(currentVehicle);
                flightInfo.Vehicle.Fuel.FuelLevel = DataGetters.getFuelLevel(currentVehicle);
                flightInfo.Vehicle.BatteryLevel = DataGetters.GetBattery(currentVehicle);
                flightInfo.Vehicle.Engines = DataGetters.GetEngineStats(currentVehicle);
                flightInfo.Vehicle.TailHook = DataGetters.GetHook(currentVehicle);
                flightInfo.Vehicle.Lights = DataGetters.getVehicleLights(currentVehicle);
                flightInfo.Vehicle.EjectionState = DataGetters.getEjectionState(currentVehicle);
                flightInfo.Vehicle.Avionics.RadarState = DataGetters.getRadarState(currentVehicle);
                flightInfo.Vehicle.Avionics.RWRContacts = DataGetters.getRWRContacts(currentVehicle);
                flightInfo.Vehicle.Avionics.MissileDetected = DataGetters.getMissileDetected(currentVehicle);
                flightInfo.Vehicle.Avionics.StallDetector = DataGetters.GetStall(currentVehicle);
                flightInfo.Vehicle.Avionics.masterArm = DataGetters.getMasterArm(currentVehicle);

                flightInfo.Vehicle.Avionics.masterArm = DataGetters.getMasterArm(currentVehicle);

                // Dumps all components in the vehicle. Will freeze game, but useful to see what we get.
                //GetAllVehicleComponents(currentVehicle);

            }
            catch (Exception ex)
            {
                Support.WriteErrorLog($"{Globals.projectName} - Error getting telemetry data " + ex.ToString());
            }

            if (dataLogger.jsonEnabled == true)
            {
                if (dataLogger.printOutput)
                {
                    Support.WriteLog("Saving JSON...");
                }

                using (StreamWriter sw = File.AppendText(dataLogger.json_path))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(flightInfo) + "\n");
                }
            }

            if (dataLogger.udpEnabled == true)
            {
                if (dataLogger.printOutput)
                {
                    Support.WriteLog("Sending UDP Packet...");
                }
                try
                {
                    SendUdp(JsonConvert.SerializeObject(flightInfo));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{Globals.projectName} - Error sending UDP " + ex.ToString());
                }
            }


        }

        public static List<Dictionary<string, string>> GetEngineStats(GameObject vehicle)
        {
            List<Dictionary<string, string>> engines = new List<Dictionary<string, string>>();

            int i = 1;

            foreach (ModuleEngine engine in vehicle.GetComponentsInChildren<ModuleEngine>())
            {
                Dictionary<string, string> engineDict = new Dictionary<string, string>();
                engineDict.Add("Engine Number", i.ToString());
                engineDict.Add("Enabled", engine.engineEnabled.ToString());
                engineDict.Add("Failed", engine.failed.ToString());
                engineDict.Add("Starting", engine.startingUp.ToString());
                engineDict.Add("Started", engine.startedUp.ToString());
                engineDict.Add("RPM", engine.displayedRPM.ToString());
                engineDict.Add("Afterburner", engine.afterburner.ToString());
                engineDict.Add("FinalThrust", engine.finalThrust.ToString());
                engineDict.Add("FinalThrottle", engine.finalThrottle.ToString());
                engineDict.Add("MaxThrust", engine.maxThrust.ToString());

                engines.Add(engineDict);
                i++;
            }

            return engines;
        }

        public static string getBrakes(GameObject vehicle)
        {
            try
            {
                AeroController aero = vehicle.GetComponentInChildren<AeroController>();
                return aero.brake.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting brakes: {ex.Message}"); // ✅ Logs the error
                return "Unavailable";
            }
        }

        public static string getRadarState(GameObject vehicle)
        {
            try
            {
                Radar radar = vehicle.GetComponentInChildren<Radar>();
                return radar.radarEnabled.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting Radar State: {ex.Message}");
                return "Unavailable";
            }

        }
        public static string getEjectionState(GameObject vehicle)
        {
            string ejectionState;
            try
            {
                EjectionSeat ejection = vehicle.GetComponentInChildren<EjectionSeat>();
                ejectionState = ejection.ejected.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting Ejection state {ex.Message}");
                ejectionState = "Unavailable";
            }

            return ejectionState;
        }

        public static List<Dictionary<string, string>> getRWRContacts(GameObject vehicle)
        {
            List<Dictionary<string, string>> contacts = new List<Dictionary<string, string>>();

            try
            {
                ModuleRWR rwr = vehicle.GetComponentInChildren<ModuleRWR>();

                if (rwr == null)
                {
                    Debug.LogWarning("getRWRContacts: No RWR module found on the vehicle.");
                    return contacts; // Return empty list
                }

                foreach (ModuleRWR.RWRContact contact in rwr.contacts)
                {
                    Dictionary<string, string> contactDict = new Dictionary<string, string>
            {
                { "active", contact.active.ToString() },
                { "locked", contact.locked.ToString() },
                { "radarSymbol", contact.radarSymbol.ToString() },
                { "signalStrength", contact.signalStrength.ToString() }
            };

                    if (contact.radarActor != null)
                    {
                        contactDict.Add("friendFoe", contact.radarActor.team.ToString());
                        contactDict.Add("name", contact.radarActor.name.ToString());
                    }
                    else
                    {
                        contactDict.Add("friendFoe", "Unknown");
                        contactDict.Add("name", "Unknown");
                    }

                    contacts.Add(contactDict);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting RWR Contacts: {ex.Message}");
            }

            return contacts;
        }

        public static string getMissileDetected(GameObject vehicle)
        {
            try
            {
                MissileDetector md = vehicle.GetComponentInChildren<MissileDetector>();
                return md.missileDetected.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting Missile Detected: {ex.Message}");
                return "Unavailable";
            }
        }

        public static string getMasterArm(GameObject vehicle)
        {
            bool masterArmState = false;
            try
            {


                foreach (VRLever lever in vehicle.GetComponentsInChildren<VRLever>())
                {

                    if (lever.gameObject.name == "masterArmSwitchInteractable")
                    {
                        if (lever.currentState == 1)
                        {
                            masterArmState = true;
                            break;
                        }
                        else
                        {
                            masterArmState = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Support.WriteErrorLog("Error getting master arm state: " + ex.ToString());
            }
            return masterArmState.ToString();
        }

        public static Dictionary<string, string> getVehicleLights(GameObject vehicle)
        {

            Dictionary<string, string> lights = new Dictionary<string, string>();

            //this is BAD
            //Light landingLights = vehicle.transform.Find("LandingLight").GetComponent<Light>();

            try
            {
                bool landinglight = false;
                bool navlight = false;
                bool strobelight = false;

                foreach (Light light in vehicle.GetComponentsInChildren<Light>())
                {

                    if (light.gameObject.name == "LandingLight")
                    {
                        landinglight = true;
                    }
                    if (light.gameObject.name.ToString().Contains("StrobeLight"))
                    {
                        strobelight = true;
                    }
                    Support.WriteLog(light.ToString());
                }


                foreach (SpriteRenderer spriteish in vehicle.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (spriteish.ToString().Contains("Nav"))
                    {
                        navlight = true;
                    }

                }

                lights.Add("LandingLights", landinglight.ToString());
                lights.Add("NavLights", navlight.ToString());
                lights.Add("StrobeLights", strobelight.ToString());

                return lights;

            }
            catch (Exception ex)
            {
                Support.WriteErrorLog("Error getting lights " + ex.ToString());
                return lights;
            }
        }
        public static string getFlaps(GameObject vehicle)
        {
            try
            {
                AeroController aero = vehicle.GetComponentInChildren<AeroController>();
                return aero.flaps.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting flaps: {ex.Message}");
                return "Unavailable";
            }
        }

        public static string GetStall(GameObject vehicle)
        {

            try
            {

                HUDStallWarning warning = vehicle.GetComponentInChildren<HUDStallWarning>();

                //Nullable boolean allows it to get "stalling" if it doesn't exist? and sets it as false? I think.
                Boolean? stalling = Traverse.Create(warning).Field("stalling").GetValue() as Boolean?;

                return stalling.ToString();
            }
            catch (Exception ex)
            {
                Support.WriteErrorLog("unable to get stall status: " + ex.ToString());
                return "False";
            }

        }
        public static string GetHook(GameObject vehicle)
        {

            try
            {

                Tailhook hook = vehicle.GetComponentInChildren<Tailhook>();

                //Nullable boolean allows it to get "stalling" if it doesn't exist? and sets it as false? I think.
                Boolean? deployed = Traverse.Create(hook).Field("deployed").GetValue() as Boolean?;

                return deployed.ToString();
            }
            catch (Exception ex)
            {
                Support.WriteErrorLog("unable to get stall status: " + ex.ToString());
                return "False";
            }

        }
        public static string GetBattery(GameObject vehicle)
        {


            try
            {
                Battery batteryCharge = vehicle.GetComponentInChildren<Battery>();
                string battery = batteryCharge.currentCharge.ToString();
                return battery;
            }
            catch (Exception ex)
            {
                Support.WriteErrorLog("unable to get battery status: " + ex.ToString());
                return "False";
            }

        }
        public static string getFuelLevel(GameObject vehicle)
        {
            try
            {
                FuelTank tank = vehicle.GetComponentInChildren<FuelTank>();
                return tank.totalFuel.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting fuel: {ex.Message}");
                return "False";
            }
        }

        public static string getFuelBurnRate(GameObject vehicle)
        {

            try
            {
                FuelTank tank = vehicle.GetComponentInChildren<FuelTank>();
                return tank.fuelDrain.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting burn rate: {ex.Message}");
                return "False";
            }

        }

        public static string getFuelDensity(GameObject vehicle)
        {
            try
            {
                FuelTank tank = vehicle.GetComponentInChildren<FuelTank>();
                return tank.fuelDensity.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting fuel density: {ex.Message}");
                return "False";
            }

        }

        public static bool GetGunFiring(GameObject vehicle)
        {

            try
            {
                WeaponManager weaponManager = vehicle.GetComponentInChildren<WeaponManager>();

                if (weaponManager.availableWeaponTypes.gun)
                {
                    return weaponManager.isFiring;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Support.WriteErrorLog("Unable to get weapon manager status: " + ex.ToString());
                return false;
            }

        }
        public static bool GetBombFiring(GameObject vehicle)
        {

            try
            {
                WeaponManager weaponManager = vehicle.GetComponentInChildren<WeaponManager>();

                if (weaponManager.availableWeaponTypes.bomb)
                {
                    return weaponManager.isFiring;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Support.WriteErrorLog("Unable to get weapon manager status: " + ex.ToString());
                return false;
            }
        }

        public static bool GetMissileFiring(GameObject vehicle)
        {

            try
            {
                WeaponManager weaponManager = vehicle.GetComponentInChildren<WeaponManager>();

                if (weaponManager.availableWeaponTypes.aam || weaponManager.availableWeaponTypes.agm || weaponManager.availableWeaponTypes.antirad || weaponManager.availableWeaponTypes.antiShip)
                {
                    return weaponManager.isFiring;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Support.WriteErrorLog("Unable to get weapon manager status: " + ex.ToString());
                return false;
            }
        }

        public static string getRadarCrossSection(GameObject vehicle)
        {
            try
            {
                RadarCrossSection rcs = vehicle.GetComponentInChildren<RadarCrossSection>();
                return rcs.GetAverageCrossSection().ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting radar cross section: {ex.Message}");
                return "False";
            }
        }


        public static bool GetLanded(GameObject vehicle)
        {
            return true;
        }


        public void SendUdp(string text)
        {

            Support.WriteLog($"{Globals.projectName} - Sending UDP Packet: {text} to {dataLogger.receiverIp}");

            byte[] sendBuffer = Encoding.ASCII.GetBytes(text);

            dataLogger.udpClient.Send(sendBuffer, sendBuffer.Length);

        }
    }
}