using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemListCell : ScrollViewCell
{
    public class Config
    {
        public bool ShowIcon;
        public Spell Spell;
        public string Icon;
    }

    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] TextMeshProUGUI AmountText;
    [SerializeField] Image Icon;

    public void Init(Config config)
    {
        if (CheckUIConfigAndLogError(config, "ItemListCell"))
            return;
        // TODO generic
        var name = config.Spell == null ? "--" : config.Spell.LocalizedName();
        NameText.SetText(name);
        var amount = config.Spell == null ? string.Empty : config.Spell.MpCost.ToString();
        AmountText.SetText(amount);

        Icon.gameObject.SafeSetActive(false);
        if (config.ShowIcon)
            Icon.sprite = ServiceManager.Get<AssetManager>().Load<Sprite>(config.Icon, () => Icon.gameObject.SafeSetActive(true));
    }
}
