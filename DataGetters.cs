using System;
using UnityEngine;
using Valve.Newtonsoft.Json;
using System.IO;
using System.Text;
using VTOLAPI;

namespace vtolvrTelemetry
{
    public class DataGetters
    {
        public VtolvrTelemetry DataLogger { get; set; }

        public DataGetters(VtolvrTelemetry DataLogger)
        {
            this.DataLogger = DataLogger;
        }

        public void GetData()
        {
            LogData f_info = new LogData();

            try
            {
                if (DataLogger.printOutput)
                {
                    Support.WriteLog("Collecting Data...");
                }

                // Get player actor and vehicle
                Actor playerActor = FlightSceneManager.instance.playerActor;
                GameObject currentVehicle = VTAPI.GetPlayersVehicleGameObject();

                if (playerActor == null || currentVehicle == null)
                {
                    Support.WriteErrorLog("Player actor or vehicle is null. Skipping data collection.");
                    return;
                }

                // General Info
                f_info.UnixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                // Physics Info
                f_info.Physics.XAccel = SafeGet(() => Math.Round(playerActor.flightInfo.acceleration.x, 2).ToString());
                f_info.Physics.YAccel = SafeGet(() => Math.Round(playerActor.flightInfo.acceleration.y, 2).ToString());
                f_info.Physics.ZAccel = SafeGet(() => Math.Round(playerActor.flightInfo.acceleration.z, 2).ToString());
                f_info.Physics.PlayerGs = SafeGet(() => Math.Round(playerActor.flightInfo.playerGs, 2).ToString());

                // Vehicle Info
                f_info.Vehicle.VehicleName = SafeGet(() => currentVehicle.name);
                f_info.Vehicle.Heading = SafeGet(() => Math.Round(playerActor.flightInfo.heading, 2).ToString());
                f_info.Vehicle.Pitch = SafeGet(() => Math.Round(playerActor.flightInfo.pitch, 2).ToString());
                f_info.Vehicle.Roll = SafeGet(() => Math.Round(playerActor.flightInfo.roll, 2).ToString());
                f_info.Vehicle.Airspeed = SafeGet(() => Math.Round(playerActor.flightInfo.airspeed, 2).ToString());
                f_info.Vehicle.VerticalSpeed = SafeGet(() => Math.Round(playerActor.flightInfo.verticalSpeed, 2).ToString());
                f_info.Vehicle.AltitudeASL = SafeGet(() => Math.Round(playerActor.flightInfo.altitudeASL, 2).ToString());

                if (DataLogger.printOutput)
                {
                    Support.WriteLog(f_info.ToCSV());
                }
            }
            catch (Exception ex)
            {
                Support.WriteErrorLog($"{Globals.projectName} - Error getting telemetry data: {ex}");
            }

            SaveData(f_info);
        }

        private void SaveData(LogData data)
        {
            // Save as CSV
            if (DataLogger.csvEnabled)
            {
                if (!File.Exists(DataLogger.csv_path))
                {
                    using (StreamWriter sw = File.AppendText(DataLogger.csv_path))
                    {
                        sw.WriteLine(data.CSVHeaders());
                    }
                }
                using (StreamWriter sw = File.AppendText(DataLogger.csv_path))
                {
                    sw.WriteLine(data.ToCSV());
                }
            }

            // Save as JSON
            if (DataLogger.jsonEnabled)
            {
                using (StreamWriter sw = File.AppendText(DataLogger.json_path))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(data) + "\n");
                }
            }

            // Send as UDP
            if (DataLogger.udpEnabled)
            {
                try
                {
                    SendUdp(JsonConvert.SerializeObject(data));
                }
                catch (Exception ex)
                {
                    Support.WriteErrorLog($"{Globals.projectName} - Error sending UDP: {ex}");
                }
            }
        }

        public void SendUdp(string text)
        {
            byte[] sendBuffer = Encoding.ASCII.GetBytes(text);
            DataLogger.udpClient.Send(sendBuffer, sendBuffer.Length);
        }

        private static T SafeGet<T>(Func<T> getter, T defaultValue = default)
        {
            try
            {
                return getter();
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
