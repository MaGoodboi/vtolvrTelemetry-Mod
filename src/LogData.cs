using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace vtolvrtelemetry
{
    public class LogData
    {
        private string logFilePath;
        private List<string> logEntries;

        public LogData(string filePath)
        {
            logFilePath = filePath;
            logEntries = new List<string>();
        }

        public void AddEntry(string entry)
        {
            string timestampedEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {entry}";
            logEntries.Add(timestampedEntry);
            Debug.Log(timestampedEntry);
        }

        public void SaveLog()
        {
            try
            {
                File.AppendAllLines(logFilePath, logEntries);
                logEntries.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving log data: {ex.Message}");
            }
        }
    }
}
