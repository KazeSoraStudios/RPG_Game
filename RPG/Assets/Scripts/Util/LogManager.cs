using System;
using UnityEngine;

public enum LogLevel
{
    Debug,
    Warn,
    Info,
    Error,
    Fatal,
    Ignore
};

public class LogManager
{

    private static LogLevel logLevel = LogLevel.Info;


    internal LogManager()
    {
        Debug.LogError("LogManager has been initialized!");
    }

    public static LogLevel GetLogLevel()
    {
        return logLevel;
    }

    public static void SetLogLevel(LogLevel level)
    {
        logLevel = level;
    }
    
    public static void Log(LogLevel level, object message)
    {
        if (level > logLevel)
            return;
        _LogMessage(message);
    }


    public static void LogDebug(object message)
    {
        if (logLevel > LogLevel.Debug)
            return;
        _LogMessage(message);
    }

    public static void LogWarn(object message)
    {
        if (logLevel > LogLevel.Warn)
            return;
        _LogMessage(message);
    }

    public static void LogInfo(object message)
    {
        if (logLevel > LogLevel.Info)
            return;
        _LogMessage(message);
    }

    public static void LogError(object message)
    {
        if (logLevel > LogLevel.Error)
            return;
        _LogError(message);
    }

    public static void LogFatal(object message)
    {
        if (logLevel > LogLevel.Fatal)
            return;
        _LogError(message);
    }

    public static void LogException(object message, Exception exception)
    {
        if (logLevel > LogLevel.Error)
            return;
        _LogMessage(message);
    }

    private static void _LogMessage(object message)
    {
        Debug.Log(message);
    }

    private static void _LogError(object message)
    {
        Debug.LogError(message);
    }

    private static void _LogException(object message, Exception exception)
    {
        Debug.LogError(message);
        Debug.LogException(exception);
    }
}