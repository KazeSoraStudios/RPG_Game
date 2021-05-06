using RPG_UI;
using System.Collections.Generic;
using UnityEngine;

namespace RPG_UI
{
    public class InGameMenu : ConfigMonoBehaviour, IGameState
    {
        public StateStack Stack;
        public StateMachine StateMachine;

        private Map Map;
        private FrontMenuState frontMenu;
        private ItemMenuState itemMenu;
        private StatusMenuState statusMenu;
        private MagicMenuState magicMenu;
        private EquipMenuState equipMenu;
        private FrontMenuState optionMenu;

        private UIController uiController;

        public FrontMenuState.Config FrontConfig;
        public StatusMenuState.Config StatusConfig;
        public ItemMenuState.Config ItemMenuConfig;
        public MagicMenuState.Config MagicConfig;
        public EquipMenuState.Config EquipConfig;

        void Start()
        {
            ServiceManager.Register(this);
            var states = new Dictionary<string, IState>();
            states.Add(Constants.FRONT_MENU_STATE, GetFrontMenu());
            states.Add(Constants.ITEM_MENU_STATE, GetItemMenu());
            states.Add(Constants.STATUS_MENU_STATE, GetStatusMenu());
            states.Add(Constants.MAGIC_MENU_STATE, GetMagicMenu());
            states.Add(Constants.EQUIP_MENU_STATE, GetEquipMenu());
            states.Add(Constants.OPTION_MENU_STATE, optionMenu);
            StateMachine = new StateMachine(states);
            var world = ServiceManager.Get<World>();
            FrontConfig = new FrontMenuState.Config
            {
                Parent = this
            };
            StatusConfig = new StatusMenuState.Config
            {
                Parent = this,
                StateMachine = StateMachine
            };
            MagicConfig = new MagicMenuState.Config
            {
                Parent = this
            };
            ItemMenuConfig = new ItemMenuState.Config
            {
                Parent = this
            };
            EquipConfig = new EquipMenuState.Config
            {
                Parent = this
            };
        }

        void OnDestroy()
        {
            ServiceManager.Unregister(this);
        }

        public void Init(Map map, StateStack stack)
        {
            Map = map;
            Stack = stack;
            gameObject.SafeSetActive(true);
            RefreshConfigs(map.MapName);
            StateMachine.Change(Constants.FRONT_MENU_STATE, FrontConfig);
        }

        public void SetUIController(UIController controller)
        {
            uiController = controller;
        }

        public bool Execute(float deltaTime)
        {
            if (Stack.Top().GetHashCode() == GetHashCode())
                StateMachine.Update(deltaTime);
            return true;
        }

        public void Enter(object o) { }

        public void Exit()
        {
            StateMachine.Stop();
            gameObject.SafeSetActive(false);
            short layers = 0x3;
            uiController.ClearChildren(layers);
        }

        public void HandleInput() { }

        public string GetName()
        {
            return "InGameMenuState";
        }

        private FrontMenuState GetFrontMenu()
        {
            if (frontMenu == null)
            {
                var asset = ServiceManager.Get<AssetManager>().Load<FrontMenuState>(Constants.FRONT_MENU_PREFAB);
                frontMenu = Instantiate(asset);
                uiController.AddMenuScreen(frontMenu.transform);
                frontMenu.gameObject.SafeSetActive(false);
            }
            return frontMenu;
        }

        private ItemMenuState GetItemMenu()
        {
            if (itemMenu == null)
            {
                var asset = ServiceManager.Get<AssetManager>().Load<ItemMenuState>(Constants.ITEM_MENU_PREFAB);
                itemMenu = Instantiate(asset);
                uiController.AddMenuScreen(itemMenu.transform);
                itemMenu.gameObject.SafeSetActive(false);
            }
            return itemMenu;
        }

        private StatusMenuState GetStatusMenu()
        {
            if (statusMenu == null)
            {
                var asset = ServiceManager.Get<AssetManager>().Load<StatusMenuState>(Constants.STATUS_MENU_PREFAB);
                statusMenu = Instantiate(asset);
                uiController.AddMenuScreen(statusMenu.transform);
                statusMenu.gameObject.SafeSetActive(false);
            }
            return statusMenu;
        }

        private MagicMenuState GetMagicMenu()
        {
            if (magicMenu == null)
            {
                var asset = ServiceManager.Get<AssetManager>().Load<MagicMenuState>(Constants.MAGIC_MENU_PREFAB);
                magicMenu = Instantiate(asset);
                uiController.AddMenuScreen(magicMenu.transform);
                magicMenu.gameObject.SafeSetActive(false);
            }
            return magicMenu;
        }

        private EquipMenuState GetEquipMenu()
        {
            if (equipMenu == null)
            {
                var asset = ServiceManager.Get<AssetManager>().Load<EquipMenuState>(Constants.EQUIP_MENU_PREFAB);
                equipMenu = Instantiate(asset);
                uiController.AddMenuScreen(equipMenu.transform);
                equipMenu.gameObject.SafeSetActive(false);
            }
            return equipMenu;
        }

        //private FrontMenuState GetOptionMenu()
        //{
        //    if (optionMenu == null)
        //    {
        //        optionMenu = ServiceManager.Get<AssetManager>().Load<FrontMenuState>(Constants.OPTION_MENU_PREFAB);
        //         uiController.AddMenuScreen(optionMenu.transform);
        //optionMenu.gameObject.SafeSetActive(false);
        //    }
        //    return optionMenu;
        //}

        private void RefreshConfigs(string mapName)
        {
            FrontConfig.MapName = mapName;
            var world = ServiceManager.Get<World>();
            ItemMenuConfig.Items = world.GetUseItemsList();
            ItemMenuConfig.KeyItems = world.GetKeyItemsList();
        }
    }
}