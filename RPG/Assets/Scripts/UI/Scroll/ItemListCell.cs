using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RPG_UI
{
    public class ItemListCell : ScrollViewCell
    {
        public class Config
        {
            public bool ShowIcon;
            public string Id;
            public string Name;
            public string Amount;
            public string Description;
            public string Icon;
        }

        [SerializeField] TextMeshProUGUI NameText;
        [SerializeField] TextMeshProUGUI ItemNameText;
        [SerializeField] TextMeshProUGUI AmountText;
        [SerializeField] Image Icon;

        public static readonly ItemListCell.Config EmptyConfig = new ItemListCell.Config
        {
            Name = "--",
            Description = string.Empty,
            Amount = string.Empty,
            ShowIcon = false
        };

        public void Init(Config config, bool itemMenu = false)
        {
            if (CheckUIConfigAndLogError(config, "ItemListCell"))
                return;
            if (itemMenu)
            {
                NameText.SetText(string.Empty);
                ItemNameText.SetText(config.Name);
            }
            else
            {
                NameText.SetText(config.Name);
                ItemNameText.SetText(string.Empty);
            }
            AmountText.SetText(config.Amount);
            Icon.gameObject.SafeSetActive(false);
            if (config.ShowIcon && !config.Icon.IsEmptyOrWhiteSpace())
                Icon.sprite = ServiceManager.Get<AssetManager>().Load<Sprite>(config.Icon, () => Icon.gameObject.SafeSetActive(true));
        }
    }
}