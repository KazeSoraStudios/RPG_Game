using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatScene : MonoBehaviour
{
    public Transform CameraPosition;
    [SerializeField] SpriteRenderer Background;

    private void Awake()
    {
        ServiceManager.Register(this);
        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        ServiceManager.Unregister(this);
    }

    public void SetBackground(string path)
    {
        ServiceManager.Get<AssetManager>().Load<Sprite>(path, (_) => Background.gameObject.SafeSetActive(true));
    }
}
