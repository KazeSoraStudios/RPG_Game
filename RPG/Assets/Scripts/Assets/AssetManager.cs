using System;
using System.Collections.Generic;
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

    public T Load<T>(string prefabPath, Action<T> callback = null) where T : UnityEngine.Object
    {
        if (prefabs.ContainsKey(prefabPath))
        {
            var prefab = prefabs[prefabPath] as T;
            callback?.Invoke(prefab);
            return prefab; 
        }
        var asset = Resources.Load<T>(prefabPath);
        if (asset == null)
            LogManager.LogError($"Cannot load prefab for {prefabPath}");
        callback?.Invoke(asset);
        prefabs[prefabPath] = asset as UnityEngine.Object;
        return asset;
    }
}
