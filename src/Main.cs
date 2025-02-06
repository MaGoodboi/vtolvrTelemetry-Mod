using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace vtolvrtelemetry
{
    [ItemId("mytikos.vtolvrtelemetry")]
    public class Main : VtolMod
    {
        public string ModFolder;
        public UdpClient SendUdpClient = new UdpClient();
        public string SendToIP = "127.0.0.1"; // Default, updated by Yaw2 discovery
        public int SendToPort = 4123;
        private bool canSendUDP = false;

        private void Awake()
        {
            ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            StartCoroutine(WaitForLoad()); // ✅ Ensures game is fully loaded before telemetry starts
            LogData.AddEntry("Start sending telemetry data to YAW Device");
        }

        private IEnumerator WaitForLoad()
        {
            while (SceneManager.GetActiveScene().buildIndex != 7 || SceneManager.GetActiveScene().buildIndex == 12)
            {
                yield return null;
            }
            LogData.AddEntry("Done waiting for map loading, starting Yaw2 discovery...");
            yield return StartCoroutine(DiscoverDevice()); // ✅ Finds Yaw2 before enabling telemetry
            canSendUDP = true;
        }

        private void Update()
        {
            if (canSendUDP)
            {
                CollectData.GetData();
                try
                {
                    SendUdpClient.Send(CollectData.Data.ToArray(), CollectData.Data.Count);
                    LogData.AddEntry("✅ Sent telemetry data to " + SendToIP);
                }
                catch (Exception ex)
                {
                    LogData.AddEntry("Error sending telemetry data: " + ex.ToString());
                }
            }
        }

        public override void UnLoad()
        {
            SendUdpClient.Close();
            SendUdpClient.Dispose();
        }

        private IEnumerator DiscoverDevice()
        {
            LogData.AddEntry("[Discovery] Attempting to find Yaw2 device...");
            UdpClient discoveryClient = null;
            try
            {
                discoveryClient = new UdpClient(50010);
                discoveryClient.EnableBroadcast = true;
                discoveryClient.Client.ReceiveTimeout = 1000; // 1s timeout
            }
            catch (Exception ex)
            {
                LogData.AddEntry("[Discovery] Could not open socket on port 50010: " + ex.Message);
                yield break;
            }

            byte[] discoveryMsg = Encoding.ASCII.GetBytes("YAW_CALLING");
            IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, 50010);
            bool deviceFound = false;
            int maxAttempts = 10;

            for (int i = 0; i < maxAttempts && !deviceFound; i++)
            {
                LogData.AddEntry("[Discovery] Attempt " + (i + 1) + "/" + maxAttempts);
                try
                {
                    discoveryClient.Send(discoveryMsg, discoveryMsg.Length, broadcastEndpoint);
                    IPEndPoint responseEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] response = discoveryClient.Receive(ref responseEndpoint);

                    if (response != null && response.Length > 0)
                    {
                        string responseText = Encoding.ASCII.GetString(response);
                        if (responseText.Contains("YAWDEVICE2"))
                        {
                            SendToIP = responseEndpoint.Address.ToString();
                            LogData.AddEntry("✅ [Discovery] Found Yaw2 at " + SendToIP);
                            deviceFound = true;
                        }
                    }
                }
                catch (SocketException)
                {
                    LogData.AddEntry("[Discovery] No response (timeout)");
                }
                catch (Exception ex)
                {
                    LogData.AddEntry("[Discovery] Error: " + ex.Message);
                }

                yield return new WaitForSeconds(1f);
            }

            discoveryClient?.Close();

            if (!deviceFound)
            {
                LogData.AddEntry("[Discovery] Failed to find Yaw2 device, using fallback IP 127.0.0.1");
                SendToIP = "127.0.0.1";
            }

            LogData.AddEntry("Testing UDP connection to " + SendToIP + ":" + SendToPort + "...");
            try
            {
                SendUdpClient.Connect(SendToIP, SendToPort);
                byte[] testMessage = Encoding.ASCII.GetBytes("TEST_MESSAGE");
                SendUdpClient.Send(testMessage, testMessage.Length);
                LogData.AddEntry("✅ Test UDP message sent successfully.");
            }
            catch (Exception ex)
            {
                LogData.AddEntry("❌ Failed to send test UDP message: " + ex.Message);
            }
        }
    }
}
