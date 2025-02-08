using UnityEngine;

public class Support
{
    public static void WriteLog(string line)
    {
        Debug.Log("[Telemetry] " + line);
    }

    public static void WriteErrorLog(string line)
    {
        Debug.LogError("[Telemetry] " + line);
    }
}
