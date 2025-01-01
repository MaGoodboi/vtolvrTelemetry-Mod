/* Thank you to nevbriv for creating the original version.*/
using System.Net.Sockets;
using System.Reflection;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using UnityEngine.Events;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HarmonyLib;
using System.Collections;

namespace vtolvrTelemetry
{
    [ItemId("mystikos.vtolvrtelemetry")] // Harmony ID for your mod, make sure this is unique
    static class Globals
    {
        public static string projectName = "vtolvrtelemtry";
        public static string projectAuthor = "mystikos";
        public static string projectVersion = "1.0";
    }
    
    public class VtolvrTelemetry : VtolMod
    {
        public string ModFolder;

        public bool runlogger;
        public int iterator;
        public int secondCount;
        public UnityAction<string> stringChanged;
        public UnityAction<int> intChanged;
        public UnityAction<bool> csvChanged;
        public string receiverIp = "127.0.0.1";
        public int receiverPort = 4123;
        public bool csvEnabled = true;

        public bool udpEnabled = false;
        public UnityAction<bool> udpChanged;

        public UnityAction<bool> jsonChanged;
        public bool jsonEnabled = true;

        public UdpClient udpClient;
        public string DataLogFolder;

        public string csv_path;
        public string json_path;

        public bool printOutput = false;

        public DataGetters dataGetter;

        public class TelemetrySettings
        {
            public string ReceiverIp { get; set; } = "127.0.0.1"; // Default IP
            public int ReceiverPort { get; set; } = 4123;         // Default port
            public bool EnableUDP { get; set; } = false;          // Default: UDP disabled
            public bool EnableCSV { get; set; } = false;          // Default: CSV disabled
            public bool EnableJSON { get; set; } = false;         // Default: JSON disabled
        }

        public TelemetrySettings settings = new TelemetrySettings();

        private void Awake()
        {
            // Initialize Unity-based settings
            CreateSettingsUI();

            Debug.Log($"{Globals.projectName} - Telemetry Mod {Globals.projectVersion} by {Globals.projectAuthor} initialized!");
        }

        public void Start()
        {
            Harmony harmony = new Harmony("vtolvrTelemetry.logger.logger");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            udpClient = new UdpClient();
            udpClient.Connect(receiverIp, receiverPort);

            Support.WriteLog("Running Startup and Waiting for map load");

            StartCoroutine(WaitForMap());

            System.IO.Directory.CreateDirectory("TelemetryDataLogs");
            System.IO.Directory.CreateDirectory("TelemetryDataLogs\\" + DateTime.UtcNow.ToString("yyyy-MM-dd HHmm"));

            DataLogFolder = "TelemetryDataLogs\\" + DateTime.UtcNow.ToString("yyyy-MM-dd HHmm") + "\\";

            csv_path = @DataLogFolder + "datalog.csv";
            json_path = @DataLogFolder + "datalog.json";

            dataGetter = new DataGetters(this);
        }

        public void CreateSettingsUI()
        {
            // Create a settings menu (basic example)
            GameObject canvasGO = new GameObject("SettingsCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            GameObject panelGO = new GameObject("SettingsPanel");
            panelGO.transform.SetParent(canvasGO.transform);
            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(400, 600);
            panelRect.anchoredPosition = Vector2.zero;

            // Add a toggle for UDP
            GameObject toggleGO = new GameObject("UDP Toggle");
            toggleGO.transform.SetParent(panelGO.transform);
            Toggle udpToggle = toggleGO.AddComponent<Toggle>();
            udpToggle.isOn = udpEnabled;
            udpToggle.onValueChanged.AddListener(OnUDPEnabledChanged);

            // Add more UI elements for IP, Port, CSV, JSON as needed
        }

        public void OnUDPEnabledChanged(bool enabled)
        {
            udpEnabled = enabled;
            Debug.Log($"UDP Enabled: {udpEnabled}");
        }

        IEnumerator WaitForMap()
        {
            while (SceneManager.GetActiveScene().buildIndex != 7 || SceneManager.GetActiveScene().buildIndex == 12)
            {
                // Pausing this method till the loader scene is unloaded
                yield return null;
            }

            Support.WriteLog("Done waiting for map load");
            yield return new WaitForSeconds(5);
            runlogger = true;
        }

        public string CleanString(string input)
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
            Debug.Log("Unloading mod...");
        }
    }
}