using System;
using System.IO;
using UnityEngine;

namespace vtolvrtelemetry
{
    public static class LogData
    {
        private static string logFilePath = Path.Combine(Application.persistentDataPath, "telemetry_log.txt");

        public static void AddEntry(string entry)
        {
            string timestampedEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {entry}";
            Debug.Log(timestampedEntry);

            try
            {
                File.AppendAllText(logFilePath, timestampedEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving log data: {ex.Message}");
            }
        }
    }
}
