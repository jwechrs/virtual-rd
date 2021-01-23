using System;
using UnityEngine;
class TNDebug {
    private static int logLevel = 0;
    private static bool shouldShowLog = false;
    public static void Log(String logText){
        if (TNDebug.shouldShowLog){
              Debug.Log(logText);
        }
    }
    public static void Log(int logTextInt){
        Log(logTextInt.ToString());
    }
    public static void Log(System.Exception e){
        Log(e.ToString());
    }
    public static void V(int verboseTextInt){
        V(verboseTextInt.ToString());
    }
    public static void V(String verboseText){
        if (logLevel>1){
            Log("Verbose:" + verboseText);
        }
    }
}
