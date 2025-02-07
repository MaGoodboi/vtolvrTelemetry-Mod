using System;
using System.Collections;
using System.Net.Sockets;
using System.Reflection;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VTOLAPI;
using HarmonyLib;
using System.IO;

namespace vtolvrtelemetry
{
    static class Globals
    {
        public static string projectName = "vtolvrtelemetry";
        public static string originalprojectAuthor = "nebriv";
        public static string updateprojectauthor = "mystikos";
        public static string projectVersion = "v2.0";
    }
    public class vtolvrtelemetry : VtolMod
    {
        public bool runlogger;
        public int iterator;
        public int secondCount;
        public UnityAction<string> stringChanged;
        public UnityAction<int> intChanged;
        public UnityAction<bool> csvChanged;
        public string receiverIp = "127.0.0.1";
        public int receiverPort = 4123;
        public bool udpEnabled = true;
        public bool jsonEnabled = true;

        public UdpClient udpClient;
        public VTAPI vtolmod_api;

        public string DataLogFolder;

        public string json_path;

        public bool printOutput = false;
        public DataGetters dataGetter;

        private void Awake()
        {
            Support.WriteLog("[Telemetry] Initializing vtolvrtelemetry mod...");

            // Setup UDP Client
            udpClient = new UdpClient();
            udpClient.Connect(receiverIp, receiverPort);

            // Create Data Logging Folder
            DataLogFolder = Path.Combine(Application.persistentDataPath, "TelemetryDataLogs", DateTime.UtcNow.ToString("yyyy-MM-dd_HHmm"));
            Directory.CreateDirectory(DataLogFolder);

            // Define Log File Paths
              json_path = Path.Combine(DataLogFolder, "datalog.json");

            Support.WriteLog($"[Telemetry] Data logs stored at: {DataLogFolder}");

        }
        public void Start()
        {
            Harmony harmony = new Harmony("vtolvrTelemetry.logger.logger");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Support.WriteLog("Running Startup and Waiting for map load");
            vtolmod_api = VTAPI.instance;

            StartCoroutine(WaitForMap());

            dataGetter = new DataGetters(this);
        }
        private IEnumerator WaitForMap()
        {
            // Wait until the player is in an active scene (avoids loading screens)
            while (SceneManager.GetActiveScene().buildIndex <= 0)
            {
                yield return null;
            }

            Support.WriteLog("Scene detected. Initializing telemetry...");

            // Wait a short delay to ensure scene is fully initialized
            yield return new WaitForSeconds(3);

            Support.WriteLog("Telemetry activated for all scenes.");
            runlogger = true;
        }
        public string cleanString(string input)
        {
            string clean = input.Replace("\\", "").Replace("/", "").Replace("<", "").Replace(">", "").Replace("*", "").Replace("\"", "").Replace("?", "").Replace(":", "").Replace("|", "");
            return clean;
        }
        public void FixedUpdate()
        {

            if (iterator < 46)
            {
                iterator++;
            }
            else
            {
                iterator = 0;
                secondCount++;

                if (runlogger)
                {
                    if (SceneManager.GetActiveScene().buildIndex != 7 && SceneManager.GetActiveScene().buildIndex != 12)
                    {

                        ResetLogger();
                    }
                }
            }

            if (runlogger)
            {
                try
                {
                    dataGetter.GetData();
                }
                catch (Exception ex)
                {
                    Support.WriteErrorLog("Error getting data." + ex.ToString());
                }

            }

        }
        public void ResetLogger()
        {
            runlogger = false;
            udpClient.Close();

            Support.WriteLog("Scene end detected. Stopping telemetry");

            Start();
        }
        public void GetAllChildrenComponents(GameObject g_object)
        {
            Component[] components = g_object.GetComponentsInChildren<Component>(true);
            foreach (Component comp in components)
            {
                Support.WriteLog(comp.ToString());
            }
            Debug.Log("");

        }
        public override void UnLoad()
        {
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient.Dispose();
            }
        }
    }
}