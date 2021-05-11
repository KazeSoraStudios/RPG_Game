using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG_Character;
using RPG_GameData;

namespace RPG_UI
{
    public class EquipMenuState : ConfigMonoBehaviour, IGameState, IScrollHandler
    {
        public class Config
        {
            public InGameMenu Parent;
            public Actor Actor;
        }

        [SerializeField] ActorSummaryPanel ActorSummary;
        [SerializeField] ItemInfoWidget ItemInfo;
        [SerializeField] StatsWidget StatsWidget;
        [SerializeField] TextMeshProUGUI DescriptionText;
        [SerializeField] ScrollView ScrollView;
        [SerializeField] RectTransform[] Slots;
        [SerializeField] GridLayoutGroup Grid;
        [SerializeField] StatsWidget Stats;

        private bool inScrollview;
        private Actor actor;
        private InGameMenu parent;
        private StateStack stack;
        private StateMachine stateMachine;
        private Dictionary<int, List<ItemListCell.Config>> configs = new Dictionary<int, List<ItemListCell.Config>>();


        public void Enter(object o = null)
        {
            if (CheckUIConfigAndLogError(0, GetName()) || ConvertConfig<Config>(o, out var config) && config == null)
                return;
            gameObject.SafeSetActive(true);
            inScrollview = false;
            parent = config.Parent;
            stack = parent.Stack;
            stateMachine = parent.StateMachine;
            ScrollView.HideCursor();
            if (actor == null || actor.Id != config.Actor.Id)
            {
                actor = config.Actor;
                configs.Clear();
                BuildConfigs(actor);
            }
            InitItemInfo(actor);
            InitActorSummary(actor);
            InitStatsWidget(actor.Stats);
            SetEquippedItemNames();
        }

        public bool Execute(float deltaTime)
        {
            if (inScrollview)
                HandleScrollInput();
            else
                HandleInput();
            return true;
        }

        public void Exit()
        {
            gameObject.SafeSetActive(false);
        }

        public string GetName()
        {
            return "EquipMenuState";
        }

        public void HandleInput()
        {
            ItemInfo.HandleInput();
            if (Input.GetKeyDown(KeyCode.Backspace))
                stateMachine.Change(Constants.FRONT_MENU_STATE, parent.FrontConfig);
        }

        public string GetPrefabPath()
        {
            return Constants.ITEM_LIST_CELL_PREFAB_PATH;
        }

        public void InitCell(int index, ScrollViewCell cell)
        {
            var slotIndex = ItemInfo.GetCurrentSelection();
            if (IsIndexError(index, slotIndex))
                return;
            if (!ConvertConfig<ItemListCell>(cell, out var listCell) || listCell == null)
            {
                LogManager.LogError($"Cell {cell.GetType()}, passed to item menu but expected ItemListCell");
                return;
            }
            var list = configs[slotIndex];
            var config = index < list.Count ? list[index] : ItemListCell.EmptyConfig;
            listCell.Init(config, true);
        }

        public Vector2 GetCellSize()
        {
            return Grid.cellSize;
        }

        public void OnAfterLoad()
        {

        }

        public int GetNumberOfCells()
        {
            var slotIndex = ItemInfo.GetCurrentSelection();
            if (!configs.ContainsKey(slotIndex))
                return 0;
            return configs[slotIndex].Count;
        }

        private void InitItemInfo(Actor actor)
        {
            if (ItemInfo == null)
            {
                LogManager.LogError("ItemInfoWidget is null in EquipMenuState.");
                return;
            }
            ItemInfo.Init(new ItemInfoWidget.Config
            {
                Actor = actor,
                OnChange = OnSelectedMenuChange,
                OnSelect = EnterScrollMenu
            });
            ItemInfo.ShowCursor();
        }

        private void InitActorSummary(Actor actor)
        {
            if (ActorSummary == null)
            {
                LogManager.LogError("ActorSummaryPanel is null in EquipMenuState.");
                return;
            }
            ActorSummary.Init(new ActorSummaryPanel.Config { Actor = actor });
        }

        private void InitStatsWidget(Stats stats)
        {
            if (Stats == null)
            {
                LogManager.LogError("StatsWidget is null in EquipMenuState.");
                return;
            }
            Stats.Init(new StatsWidget.Config { Stats = stats });
        }

        private void SetDesciptionText(int index)
        {
            var slotIndex = ItemInfo.GetCurrentSelection();
            if (IsIndexError(index, slotIndex))
                return;
            var list = configs[slotIndex];
            var config = index < list.Count - 1 ? list[index] : ItemListCell.EmptyConfig;
            SetDesciptionText(config.Description);
        }

        private void SetDesciptionText(string text)
        {
            var localizedText = ServiceManager.Get<LocalizationManager>().Localize(text);
            DescriptionText.SetText(localizedText);
        }

        private void HandleScrollInput()
        {
            ScrollView.HandleInput();
            if (Input.GetKeyDown(KeyCode.Backspace))
                ReturnFromScrollMenu();
        }

        private void EnterScrollMenu()
        {
            ScrollView.ShowCursor();
            inScrollview = true;
            OnScrollPositionChange(ScrollView.GetCurrentSelection());
        }

        private void ReturnFromScrollMenu()
        {
            inScrollview = false;
            ScrollView.HideCursor();
            Stats.DisplayStats();
            Stats.TurnOffPredicitionStats();
        }

        private void OnSelectedMenuChange()
        {
            ScrollView.Refresh();
            var slotIndex = ItemInfo.GetCurrentSelection();
            var slot = GetSlotFromIndex(slotIndex);
            var text = actor.GetEquipmentName(slot);
            SetDesciptionText(text);
        }

        private void OnEquip(int index)
        {
            var slotIndex = ItemInfo.GetCurrentSelection();
            if (IsIndexError(index, slotIndex))
                return;
            var list = configs[slotIndex];
            var config = index < list.Count ? list[index] : ItemListCell.EmptyConfig;
            var slot = GetSlotFromIndex(slotIndex);
            var currentItem = actor.GetEquipmentAtSlot(slot);
            if (config.Id.IsEmptyOrWhiteSpace() || currentItem != null && currentItem.Id.Equals(config.Id))
            {
                actor.Equip(slot);
            }
            else
            {
                var item = ServiceManager.Get<GameData>().Items[config.Id] as ItemInfo;
                actor.Equip(slot, item);
            }
            SetItemName(slot, actor.GetEquipmentName(slot));
            ReturnFromScrollMenu();
        }

        private void BuildConfigs(Actor actor)
        {
            for (int i = 0; i < actor.Equipment.Length; i++)
                configs.Add(i, new List<ItemListCell.Config>());
            var items = ServiceManager.Get<World>().GetUseItemsList();
            foreach (var item in items)
            {
                if (!actor.CanUse(item.ItemInfo))
                    continue;
                var config = new ItemListCell.Config { Name = item.ItemInfo.GetName(), Id = item.ItemInfo.Id };
                switch (item.ItemInfo.Type)
                {
                    case ItemType.Weapon:
                        configs[0].Add(config);
                        break;
                    case ItemType.Armor:
                        configs[1].Add(config);
                        break;
                    case ItemType.Accessory:
                        configs[2].Add(config);
                        break;
                    default:
                        break;
                }
            }
            ScrollView.Init(this, OnScrollPositionChange, OnEquip);
        }

        private bool IsIndexError(int configIndex, int slotIndex)
        {
            if (configIndex < 0)
            {
                LogManager.LogError($"Index[{configIndex}] is out of range for equip menu configs.");
                return true;
            }
            if (!configs.ContainsKey(slotIndex))
            {
                LogManager.LogError($"EquipMenu does not contain configs for slot index: {slotIndex}");
                return true;
            }
            return false;
        }

        private EquipSlot GetSlotFromIndex(int index)
        {
            return index == 0 ? EquipSlot.Weapon :
                index == 1 ? EquipSlot.Armor :
                EquipSlot.Accessory1;
        }

        private void SetEquippedItemNames()
        {
            SetItemName(EquipSlot.Weapon, actor.GetEquipmentName(EquipSlot.Weapon));
            SetItemName(EquipSlot.Armor, actor.GetEquipmentName(EquipSlot.Armor));
            SetItemName(EquipSlot.Accessory1, actor.GetEquipmentName(EquipSlot.Accessory1));
        }

        private void SetItemName(EquipSlot slot, string name)
        {
            if (slot == EquipSlot.Weapon)
                ItemInfo.ChangeWeaponText(name);
            else if (slot == EquipSlot.Armor)
                ItemInfo.ChangeArmorText(name);
            else if (slot == EquipSlot.Accessory1)
                ItemInfo.ChangeAccessoryText(name);
        }

        private void OnScrollPositionChange(int index)
        {
            if (!inScrollview)
                return;
            SetDesciptionText(index);
            var slotIndex = ItemInfo.GetCurrentSelection();
            if (IsIndexError(index, slotIndex))
                return;
            var list = configs[slotIndex];
            var config = index < list.Count ? list[index] : ItemListCell.EmptyConfig;
            var slot = GetSlotFromIndex(slotIndex);
            var currentItem = actor.GetEquipmentAtSlot(slot);
            if (config.Id.IsEmptyOrWhiteSpace() || currentItem != null && currentItem.Id.Equals(config.Id))
            {
                Stats.TurnOffPredicitionStats();
                return;
            }
            var item = ServiceManager.Get<GameData>().Items[config.Id] as ItemInfo;
            var predictedStats = actor.PredictStats(slot, item);
            for (int i = 0; i < predictedStats.Count; i++)
                predictedStats[i] = Random.Range(-5, 7);
            Stats.ShowPredictionStats(predictedStats);
        }
    }
}