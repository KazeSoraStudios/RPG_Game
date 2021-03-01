using System.Collections.Generic;

public class ServiceManager
{
    private static Dictionary<object, object> _services = new Dictionary<object, object>();

    internal ServiceManager()
    {

    }

    public static void Register<T>(T t)
    {
        if (_services.ContainsKey(typeof(T)))
        {
            LogManager.LogError($"{typeof(T)} is already registered!");
            return;
        }
        _services.Add(typeof(T), t);
    }
    public static void Unregister<T>(T t)
    {
        if (!_services.ContainsKey(typeof(T)))
        {
            LogManager.LogError($"{typeof(T)} is not registered!");
            return;
        }
        _services.Remove(typeof(T));
    }

    public static T Get<T>()
    {
        if (!_services.ContainsKey(typeof(T)))
        {
            LogManager.LogError($"Key {typeof(T)} not found! Returning null.");
            return default;
        }
        else
        {
            return (T)_services[typeof(T)];
        }
    }
}