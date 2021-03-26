using System;
using UnityEngine;
using TMPro;
using RPG_Character;

namespace RPG_UI
{
    public class ItemInfoWidget : UIMonoBehaviour
    {
        public struct Config
        {
            public Actor Actor;
            public Action OnChange;
            public Action OnSelect;
        }

        [SerializeField] TextMeshProUGUI WeaponText;
        [SerializeField] TextMeshProUGUI ArmorText;
        [SerializeField] TextMeshProUGUI AccesoryText;
        [SerializeField] RectTransform SelectionArrow;

        private int selectionIndex = 0;
        private const int selections = 3;
        private Action onChange;
        private Action onSelect;

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, "ItemInfoWidget"))
            {
                return;
            }
            HideCursor();
            onChange = config.OnChange;
            onSelect = config.OnSelect;
            var actor = config.Actor;
            OnSelectionChange();
            ChangeWeaponText(actor.GetEquipmentName(EquipSlot.Weapon));
            ChangeArmorText(actor.GetEquipmentName(EquipSlot.Armor));
            ChangeAccessoryText(actor.GetEquipmentName(EquipSlot.Accessory1));
        }

        public void ChangeWeaponText(string text)
        {
            if (WeaponText == null)
            {
                LogManager.LogError("WeaponText is null.");
                return;
            }
            WeaponText.SetText(text);
        }

        public void ChangeArmorText(string text)
        {
            if (ArmorText == null)
            {
                LogManager.LogError("ArmorText is null.");
                return;
            }
            ArmorText.SetText(text);
        }

        public void ChangeAccessoryText(string text)
        {
            if (AccesoryText == null)
            {
                LogManager.LogError("AccessoryText is null.");
                return;
            }
            AccesoryText.SetText(text);
        }

        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectionIndex--;
                if (selectionIndex < 0)
                    selectionIndex = selections - 1;
                OnSelectionChange();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectionIndex++;
                if (selectionIndex >= selections)
                    selectionIndex = 0;
                OnSelectionChange();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
                onSelect?.Invoke();
        }

        public void ShowCursor()
        {
            SelectionArrow.gameObject.SafeSetActive(true);
        }

        public void HideCursor()
        {
            SelectionArrow.gameObject.SafeSetActive(false);
        }

        public int GetCurrentSelection()
        {
            return selectionIndex;
        }

        private void OnSelectionChange()
        {
            var y = GetYPosition();
            var position = new Vector2(SelectionArrow.transform.position.x, y);
            SelectionArrow.transform.position = position;
            onChange?.Invoke();
        }

        private float GetYPosition()
        {
            switch (selectionIndex)
            {
                case 0:
                    return WeaponText.transform.position.y;
                case 1:
                    return ArmorText.transform.position.y;
                case 2:
                    return AccesoryText.transform.position.y;
                default:
                    LogManager.LogError($"Selection index has invalid value of {selectionIndex}");
                    return 0;
            }
        }
    }
}