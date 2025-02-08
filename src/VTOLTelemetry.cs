using System;
using UnityEngine;
using System.Net.Sockets;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using System.IO;
using System.Reflection;
using System.Net;

namespace vtolvrtelemetry
{
    [ItemId("magoodboi.vtolvrtelemetry")]
    public class vtolvrtelemetry : VtolMod
    {
        private UdpClient SendUdpClient = new UdpClient();
        private IPEndPoint remoteEndPoint;
        private bool canSendUDP;
        private TelemetryManager telemetryManager;
        public string ModFolder;
        public string SendToIP = "127.0.0.1";
        public int SendToPort = 4123;

        public void Awake()
        {
            Support.WriteLog("[Telemetry] Initializing...");

            telemetryManager = new TelemetryManager();
            this.ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            this.remoteEndPoint = new IPEndPoint(IPAddress.Parse(SendToIP), SendToPort);
            this.canSendUDP = true;

            Support.WriteLog("[Telemetry] Telemetry system started.");
        }

        private bool hasWarnedNoTelemetry = false; // Prevent log spam

        private void FixedUpdate()
        {
            if (!this.canSendUDP || this.telemetryManager == null)
                return;

            try
            {
                string telemetryData = telemetryManager.GetData();
                if (!string.IsNullOrEmpty(telemetryData))
                {
                    Support.WriteLog("[Telemetry] Sending: " + telemetryData);
                    byte[] sendBuffer = System.Text.Encoding.ASCII.GetBytes(telemetryData);
                    this.SendUdpClient.Send(sendBuffer, sendBuffer.Length, remoteEndPoint);
                    Support.WriteLog("[Telemetry] Sent telemetry data.");
                    hasWarnedNoTelemetry = false; // Reset warning flag
                }
                else if (!hasWarnedNoTelemetry) // Only log once
                {
                    Support.WriteErrorLog("[Telemetry] Warning: No telemetry data. Waiting for player aircraft...");
                    hasWarnedNoTelemetry = true;
                }
            }
            catch (Exception ex)
            {
                Support.WriteErrorLog("[Telemetry] Error sending telemetry data: " + ex.ToString());
            }
        }

        public override void UnLoad()
        {
            Support.WriteLog("[Telemetry] Unloading telemetry module.");
            SendUdpClient?.Close();
            SendUdpClient?.Dispose();
        }
    }
}
