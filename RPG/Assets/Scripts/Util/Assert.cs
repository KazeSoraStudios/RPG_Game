using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAssert
{
    public static void Assert(bool condition,  string message)
    {
        if (LogManager.GetLogLevel() > LogLevel.Debug)
            return;
        if (!condition)
            LogManager.LogError($"Assert failed: {message}");
    }
}
