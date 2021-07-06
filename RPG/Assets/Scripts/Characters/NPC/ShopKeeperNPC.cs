using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_UI;

namespace RPG_Character
{
    public class ShopKeeperNPC : MonoBehaviour, Trigger
    {
        [SerializeField] string ShopId;

        public void OnEnter(TriggerParams triggerParams)
        {
            
        }

        public void OnExit(TriggerParams triggerParams) { }

        public void OnStay(TriggerParams triggerParams) { }

        public void OnUse(TriggerParams triggerParams)
        {
            var stack = ServiceManager.Get<GameLogic>().Stack;
        var config = new Textbox.Config
        {
            ImagePath = string.Empty,
            Text = ServiceManager.Get<LocalizationManager>().Localize("ID_SHOP_KEEPER_WELCOME_TEXT"),
            AdvanceTime = float.MaxValue,
            ShowImage = false,
            OnFinish = () => LoadShopState(stack)
        };
            stack.PushTextbox(config);
        }

        private void LoadShopState(StateStack stack)
        {
            var asset = ServiceManager.Get<AssetManager>().Load<WeaponShopMenu>(Constants.WEAPON_SHOP_MENU_PREFAB);
            if (asset == null)
                return;
            var shop = Instantiate(asset);
            var shopParent = ServiceManager.Get<UIController>().MenuLayer;
            shop.transform.SetParent(shopParent, false);
            var config = new WeaponShopMenu.Config
            {
                ShopId = ShopId,
                Stack = stack
            };
            shop.Init(config);
            stack.Push(shop);
        }
    }
}
