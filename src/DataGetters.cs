using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace vtolvrtelemetry
{
    public class DataGetters
    {
        private VtolvrTelemetry mod;
        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;

        public DataGetters(VtolvrTelemetry mod)
        {
            this.mod = mod;
            udpClient = new UdpClient();
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(mod.receiverIp), mod.receiverPort);
        }

        public void GetData()
        {
            try
            {
                if (string.IsNullOrEmpty(mod.receiverIp) || mod.receiverIp == "127.0.0.1")
                {
                    Debug.LogWarning("Telemetry skipped: Yaw2 device not discovered yet.");
                    return;
                }

                // Example telemetry data - replace with actual telemetry values
                string telemetryData = "Yaw: 10, Pitch: 5, Roll: 2";
                SendUdp(telemetryData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error retrieving telemetry data: {ex.Message}");
            }
        }

        private void SendUdp(string data)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(data);
                udpClient.Send(bytes, bytes.Length, remoteEndPoint);
                Debug.Log($"Sent telemetry: {data}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending telemetry data: {ex.Message}");
            }
        }
    }
}
