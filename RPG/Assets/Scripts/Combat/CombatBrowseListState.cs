using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG_Character;
using RPG_UI;
using TMPro;

namespace RPG_Combat
{
    public enum BrowseList { Item, Magic, Special }
    public class CombatBrowseListState : ConfigMonoBehaviour, IScrollHandler, IGameState
    {
        public class Config
        {
            public BrowseList Browse;
            public string Title;
            public Actor Actor;
            public StateStack Stack;
            public Action OnExit;
            public Func<ItemListCell.Config, string> OnRender;
            public Action<ItemListCell.Config> OnFocus;
            public Action<CombatBrowseListState, string> OnSelect;
        }

        [SerializeField] TextMeshProUGUI TitleText;
        [SerializeField] ScrollView ScrollView;
        [SerializeField] GridLayoutGroup Grid;

        private Config config;
        private List<ItemListCell.Config> configs = new List<ItemListCell.Config>();

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, GetName()))
                return;
                this.config = config;
            CreateConfigs();
            InitScrollView();
            Show();
        }

        public void Hide()
        {
            gameObject.SafeSetActive(false);
        }

        public void Show()
        {
            gameObject.SafeSetActive(true);
        }

        public void Enter(object o) { }

        public bool Execute(float deltaTime)
        {
            return true;
        }

        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                config.Stack.Pop();
                return;
            }
            ScrollView.HandleInput();
        }

        public void Exit()
        {
            config.OnExit?.Invoke();
            Hide();
        }

        public string GetName()
        {
            return "CombatBrowseListState";
        }

        public Vector2 GetCellSize()
        {
            return Grid.cellSize;
        }

        public int GetNumberOfCells()
        {
            return configs.Count;
        }

        public string GetPrefabPath()
        {
            return Constants.ITEM_LIST_CELL_PREFAB_PATH;
        }

        public void InitCell(int index, ScrollViewCell cell)
        {
            if (index < 0)
            {
                LogManager.LogError($"Index[{index}] is out of range for CombatAction configs.");
                return;
            }
            if (!ConvertConfig<ItemListCell>(cell, out var listCell) || listCell == null)
            {
                LogManager.LogError($"Cell {cell.GetType()}, passed to CombatAction but expected ItemListCell");
                return;
            }
            // If we go past the end return the empty item we added
            var config = index < configs.Count ? configs[index] : ItemListCell.EmptyConfig;
            listCell.Init(config);
        }

        public void OnAfterLoad() { }

        private void InitScrollView()
        {
            if (ScrollView == null)
            {
                LogManager.LogError("Scrollview is null in CombatBrowseListState");
                return;
            }
            ScrollView.Init(this, OnChange, OnSelect);
        }

        private void CreateConfigs()
        {
            configs.Clear();
            switch(config.Browse)
            {
                case BrowseList.Item:
                    SetTitle("ID_ITEM_TEXT");
                    CreateItemConfigs();
                    break;
                case BrowseList.Magic:
                    SetTitle("ID_MAGIC_TEXT");
                    CreateMagicConfigs();
                    break;
                case BrowseList.Special:
                    SetTitle("ID_SPECIAL_TEXT");
                    CreateSpecialConfigs();
                    break;
                default:
                    LogManager.LogError($"Unkown BrowseList: {config.Browse}.");
                    break;
            }
        }

        private void SetTitle(string text)
        {
            if (TitleText == null)
            {
                LogManager.LogError("TitleText is null in CombatBrowseListState.");
                return;
            }
            TitleText.SetText(ServiceManager.Get<LocalizationManager>().Localize(text));
        }

        private void CreateItemConfigs()
        {
            var items = ServiceManager.Get<World>().GetUseItemsByType(ItemType.Useable);
            foreach (var item in items)
            {
                var name = item.ItemInfo.GetName();
                var text = $"{name} x{item.Count}";
                configs.Add(new ItemListCell.Config
                {
                    ShowIcon = false,
                    Name = text,
                    Id = item.ItemInfo.Id
                });
            }
        }

        private void CreateMagicConfigs()
        {
            var spells = config.Actor.Spells;
            foreach (var spell in spells)
            {
                // TODO change color if can/cannot cast
                var name = spell.LocalizedName();
                var cost = spell.MpCost.ToString();
                configs.Add(new ItemListCell.Config
                {
                    ShowIcon = false,
                    Name = name,
                    Amount = cost,
                    Id =spell.Id,
                    Color = config.Actor.CanCast(spell) ? Color.white : Color.red
                });
            }
        }

        private void CreateSpecialConfigs()
        {
            var specials = config.Actor.Specials;
            foreach (var special in specials)
            {
                var name = special.LocalizedName();
                var cost = special.MpCost.ToString();
                configs.Add(new ItemListCell.Config
                {
                    ShowIcon = false,
                    Name = name,
                    Amount = cost,
                    Id = special.Id,
                    Color = config.Actor.CanCast(special) ? Color.white : Color.red
                });
            }
        }

        private void OnChange(int index)
        {
            if (index < 0 || index > configs.Count || configs.Count == 0)
                return;
            config.OnFocus?.Invoke(configs[index]);
        }

        private void OnSelect(int index)
        {
            if (configs.Count == 0)
                return;
            if (index < 0 || index > configs.Count)
            {
                LogManager.LogError("Index is invalid for BrowseList configs.");
                return;
            }
            if (configs[index].Id.IsEmptyOrWhiteSpace())
                return;
            config.OnSelect?.Invoke(this, configs[index].Id);
        }
    }
}