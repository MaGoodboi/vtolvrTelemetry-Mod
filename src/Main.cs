/* Thank you to nevbriv for creating the original version.*/
using System.Net.Sockets;
using System.Reflection;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using UnityEngine.Events;
using HarmonyLib;
using System;
using System.Text;
using System.Collections;
using UnityEngine;
using System.Net;

namespace vtolvrtelemetry
{
    static class Globals
    {
        public static string projectName = "vtolvrtelemetry";
        public static string projectAuthor = "mystikos";
        public static string projectVersion = "1.0";
    }

    [ItemId("mystikos.vtolvrtelemetry")] // Harmony ID for your mod, make sure this is unique
    public class VtolvrTelemetry : VtolMod
    {
        public string receiverIp = "127.0.0.1"; // Placeholder until discovery
        public int receiverPort = 28067;       // Default telemetry port
        public UdpClient udpClient;

        public string ModFolder;
        public bool runlogger;
        public int iterator;
        public int secondCount;
        public UnityAction<string> stringChanged;
        public UnityAction<int> intChanged;
        public UnityAction<bool> csvChanged;
        public UnityAction<bool> udpChanged;
        public UnityAction<bool> jsonChanged;

        public string DataLogFolder;
        public string csv_path;
        public string json_path;

        public bool csvEnabled = true;
        public bool udpEnabled = true;
        public bool jsonEnabled = true;
        public bool printOutput = false;

        public DataGetters dataGetter;

        private void Awake()
        {
            Debug.Log($"{Globals.projectName} - Telemetry Mod {Globals.projectVersion} by {Globals.projectAuthor} initialized!");
        }

        public void Start()
        {
            Harmony harmony = new Harmony("vtolvrtelemetry.logger.logger");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Debug.Log("Starting telemetry mod...");

            // Start device discovery
            StartCoroutine(DiscoverDevice());

            // Enable telemetry immediately
            runlogger = true;
            Debug.Log("Telemetry logging enabled for all scenes.");

            // Setup logging directories
            DataLogFolder = $"TelemetryDataLogs\\{DateTime.UtcNow:yyyy-MM-dd HHmm}\\";
            System.IO.Directory.CreateDirectory(DataLogFolder);

            csv_path = $"{DataLogFolder}datalog.csv";
            json_path = $"{DataLogFolder}datalog.json";

            dataGetter = new DataGetters(this);
        }

        public IEnumerator DiscoverDevice()
        {
            Debug.Log("Discovering Yaw2 chair...");

            UdpClient discoveryClient = new UdpClient(50010); // Broadcast port
            discoveryClient.EnableBroadcast = true;

            byte[] discoveryMessage = Encoding.ASCII.GetBytes("YAW_CALLING");
            IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, 50010);

            // Send discovery message
            discoveryClient.Send(discoveryMessage, discoveryMessage.Length, broadcastEndpoint);
            Debug.Log("Discovery message sent.");

            IPEndPoint responseEndpoint = new IPEndPoint(IPAddress.Any, 50050); // Response port
            bool deviceFound = false;
            int maxRetries = 10; // Retry up to 10 times
            int retries = 0;

            while (!deviceFound && retries < maxRetries)
            {
                Debug.Log($"Discovery attempt {retries + 1}/{maxRetries}");

                try
                {
                    byte[] response = discoveryClient.Receive(ref responseEndpoint);
                    string responseMessage = Encoding.ASCII.GetString(response);

                    if (responseMessage.Contains("YAWDEVICE2")) // Validate response
                    {
                        receiverIp = responseEndpoint.Address.ToString();
                        Debug.Log($"Discovered Yaw2 chair at IP: {receiverIp}");
                        deviceFound = true;
                    }
                }
                catch (SocketException ex)
                {
                    retries++;
                    Debug.LogWarning($"Timeout waiting for device response. Retrying... ({retries}/{maxRetries})");
                    Debug.LogError($"SocketException: {ex.Message}");
                }

                // Delay outside catch block
                yield return new WaitForSeconds(1f);
            }

            if (!deviceFound)
            {
                receiverIp = "127.0.0.1"; // Fallback IP
                Debug.LogError("Failed to discover Yaw2 chair. Using fallback IP.");
            }

            discoveryClient.Close();
        }

        public override void UnLoad()
        {
            Debug.Log("Unloading telemetry mod...");
            udpClient?.Dispose(); // Properly release the UDP client
        }
    }
}
