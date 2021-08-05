using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
	private Dictionary<string, string> localization = new Dictionary<string, string>();

    void Awake()
    {
        ServiceManager.Register(this);
    }

    void OnDestroy()
    {
        ServiceManager.Unregister(this);
    }

	public string Localize(string id)
    {
        if (id != null && localization.ContainsKey(id))
            return localization[id];
        return id;
    }

    public void SetLocalization(Dictionary<string, string> loc)
    {
        localization = loc;
    }

	public void Load(string localizationPath)
    {

    }
}