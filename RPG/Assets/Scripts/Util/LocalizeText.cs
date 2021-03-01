using UnityEngine;
using TMPro;

public class LocalizeText : MonoBehaviour
{
    [SerializeField] string Text;

    void Start()
    {
        var localization = ServiceManager.Get<LocalizationManager>();
        var text = GetComponent<TextMeshProUGUI>();
        if (localization == null || text == null)
            return;
        text.SetText(localization.Localize(Text));
    }
}
