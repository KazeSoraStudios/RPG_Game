using UnityEngine;
using TMPro;

namespace RPG_UI
{
    public class InventoryInfoWidget : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI GoldText;
        [SerializeField] TextMeshProUGUI InventoryText;
        [SerializeField] TextMeshProUGUI EquippedText;

        public void SetGoldText(int gold)
        {
            if (GoldText == null)
                return;
            GoldText.SetText($"{gold}");
        }

        public void SetInventoryText(int inventory)
        {
            if (InventoryText == null)
                return;
            InventoryText.SetText($"{inventory}");
        }

        public void SetValues(int inventory, int equipped)
        {
            if (InventoryText != null)
            {
                InventoryText.SetText($"{inventory}");
            }
            if (EquippedText != null)
            {
                EquippedText.SetText($"{equipped}");
            }
        }
    }
}