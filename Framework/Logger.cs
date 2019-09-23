using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Logger 
{
    public enum Level
    {
        Verbose,
        Detailed,
        Critical
    }

    public static void Log(object logger,Level lev,string message)
    {
        StackTraceUtility.ExtractStackTrace();

        string messagePrefix = logger.ToString() +" "+logger.GetType().ToString()+ "\n";
        string messageSuffix = "\n"+StackTraceUtility.ExtractStackTrace();
        message = messagePrefix + message + messageSuffix;

        switch (lev)
        {
            case Level.Verbose:
                Debug.Log(message);
                break;
            case Level.Detailed:
                Debug.LogWarning(message);
                break;
            case Level.Critical:
                Debug.LogError(message);
                break;

        }
            
    }
}
