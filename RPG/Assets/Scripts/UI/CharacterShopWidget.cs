using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using RPG_Character;

namespace RPG_UI
{
    public class CharacterShopWidget : MonoBehaviour
    {
        [SerializeField] GameObject Equipped;
        [SerializeField] GameObject Increase;
        [SerializeField] GameObject Decrease;
        [SerializeField] Image Character;

        private Actor actor;

        public void Init(Actor actor)
        {
            this.actor = actor;
            Character.gameObject.SafeSetActive(false);
            Character.sprite = ServiceManager.Get<AssetManager>().Load<Sprite>(actor.Portrait, (_) => Character.gameObject.SafeSetActive(true));
            ResetWidget();
        }

        public void UpdateForItem(ItemInfo item)
        {
            var equipCount = actor.EquipCount(item.Id);
            SetEquipped(equipCount > 0);
            foreach (var restriction in item.UseRestriction)
            {
                if (restriction == actor.UseRestriction)
                {
                    SetIncrease(false);
                    SetDecrease(false);
                    return;
                }
            }
            var currentTotal = actor.Stats.GetTotalStats();
            var newTotal = actor.PredictStats(item.GetEquipSlot(), item).Sum();
            SetIncrease(newTotal > currentTotal);
            SetDecrease(currentTotal > newTotal);
        }

        public void UpdateEquipOnly(ItemInfo item)
        {
            var equipCount = actor.EquipCount(item.Id);
            SetEquipped(equipCount > 0);
            SetIncrease(false);
            SetDecrease(false);
        }

        public void ResetWidget()
        {
            SetEquipped(false);
            SetIncrease(false);
            SetDecrease(false);
        }

        public void SetEquipped(bool on)
        {
            if (Equipped != null)
                Equipped.SafeSetActive(on);
        }

        public void SetIncrease(bool on)
        {
            if (Increase != null)
                Increase.SafeSetActive(on);
        }

        public void SetDecrease(bool on)
        {
            if (Decrease != null)
                Decrease.SafeSetActive(on);
        }
    }
}
