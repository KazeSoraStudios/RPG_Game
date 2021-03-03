using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetManager : MonoBehaviour
{
    private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

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
        var asset = Resources.Load<GameObject>(prefabPath);
        if (asset == null)
            LogManager.LogError($"Cannot load prefab for {prefabPath}");
        return asset;
    }

    public T Load<T>(string prefabPath, Action callback = null) where T : UnityEngine.Object
    {
        var asset = Resources.Load<T>(prefabPath);
        if (asset == null)
            LogManager.LogError($"Cannot load prefab for {prefabPath}");
        callback?.Invoke();
        return asset;
    }
}
