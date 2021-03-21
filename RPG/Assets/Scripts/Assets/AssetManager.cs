using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    private Dictionary<string, UnityEngine.Object> prefabs = new Dictionary<string, UnityEngine.Object>();

    private void Awake()
    {
        ServiceManager.Register(this);
    }

    private void OnDestroy()
    {
        ServiceManager.Unregister(this);
    }

    public GameObject Load(string prefabPath)
    {
        if (prefabs.ContainsKey(prefabPath))
        {
            return prefabs[prefabPath] as GameObject;
        }
        var asset = Resources.Load<GameObject>(prefabPath);
        if (asset == null)
            LogManager.LogError($"Cannot load prefab for {prefabPath}");
        prefabs[prefabPath] = asset;
        return asset;
    }

    public T Load<T>(string prefabPath, Action callback = null) where T : UnityEngine.Object
    {
        if (prefabs.ContainsKey(prefabPath))
        {
            callback?.Invoke();
            return prefabs[prefabPath] as T; 
        }
        var asset = Resources.Load<T>(prefabPath);
        if (asset == null)
            LogManager.LogError($"Cannot load prefab for {prefabPath}");
        callback?.Invoke();
        prefabs[prefabPath] = asset as UnityEngine.Object;
        return asset;
    }
}
