using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemListCell : ScrollViewCell
{
    public class Config
    {
        public bool ShowIcon;
        public string Name;
        public string Amount;
        public string Description;
        public string Icon;
    }

    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] TextMeshProUGUI AmountText;
    [SerializeField] Image Icon;

    public static readonly ItemListCell.Config EmptyConfig = new ItemListCell.Config
    {
        Name = "--",
        Description = string.Empty,
        Amount = string.Empty,
        ShowIcon = false
    };

    public void Init(Config config)
    {
        if (CheckUIConfigAndLogError(config, "ItemListCell"))
            return;
        NameText.SetText(config.Name);
        AmountText.SetText(config.Amount);
        Icon.gameObject.SafeSetActive(false);
        if (config.ShowIcon)
            Icon.sprite = ServiceManager.Get<AssetManager>().Load<Sprite>(config.Icon, () => Icon.gameObject.SafeSetActive(true));
    }
}
