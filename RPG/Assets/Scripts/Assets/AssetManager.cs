using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetManager : MonoBehaviour
{

    public static GameObject Load(string prefabPath)
    {
        var asset = Resources.Load<GameObject>(prefabPath);
        if (asset == null)
            LogManager.LogError($"Cannot load prefab for {prefabPath}");
        return asset;
    }

    public static T Load<T>(string prefabPath, Action callback = null) where T : UnityEngine.Object
    {
        var asset = Resources.Load<T>(prefabPath);
        if (asset == null)
            LogManager.LogError($"Cannot load prefab for {prefabPath}");
        callback?.Invoke();
        return asset;
    }
}
