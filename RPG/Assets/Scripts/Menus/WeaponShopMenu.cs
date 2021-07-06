using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG_Character;
using RPG_GameData;

namespace RPG_UI
{
    public class WeaponShopMenu : ConfigMonoBehaviour, IGameState, IScrollHandler
    {
        public class Config
        {
            public string ShopId;
            public StateStack Stack;
        }
        
        [SerializeField] TextMeshProUGUI DescriptionText;
        [SerializeField] ScrollView ScrollView;
        [SerializeField] GridLayoutGroup Grid;
        [SerializeField] MenuOptionsList OptionsList;
        [SerializeField] InventoryInfoWidget InventoryWidget;
        [SerializeField] CharacterShopWidgetList CharacterWidgets;

        private bool inScrollView;
        private bool inSellItems;
        private int categoryIndex = 0;
        private StateStack stack;
        private World world;
        private Party party;
        private List<ItemListCell.Config> buyConfigs = new List<ItemListCell.Config>();
        private List<ItemListCell.Config> sellConfigs = new List<ItemListCell.Config>();
        private List<ItemListCell.Config> activeConfigs = new List<ItemListCell.Config>();
        private DictionaryList<string, ItemInfo> items;

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, GetName()))
                return;
            gameObject.SafeSetActive(true);
            stack = config.Stack;
            inScrollView = false;
            inSellItems = false;
            world = ServiceManager.Get<World>();
            world.LockInput();
            party = ServiceManager.Get<Party>();
            items = ServiceManager.Get<GameData>().Items;
            ScrollView.HideCursor();
            buyConfigs.Clear();
            sellConfigs.Clear();
            activeConfigs.Clear();
            SetUpOptionsList();
            SetUpInventoryWidget();
            CharacterWidgets.Init();
            SetUpMenu(config.ShopId);
        }

        public void Enter(object o = null) { }

        public bool Execute(float deltaTime)
        {
            ScrollView.Execute();
            return false;
        }

        public void Exit()
        {
            ScrollView.ClearCells();
            buyConfigs.Clear();
            sellConfigs.Clear();
            gameObject.SafeSetActive(false);
            world.UnlockInput();
            Destroy(this.gameObject);
        }

        public string GetName()
        {
            return "WeaponShopState";
        }

        public void HandleInput()
        {
            if (inScrollView)
            {
                ScrollView.HandleInput();
                HandleInScrollInput();
            }
            else
                HandleSelectItemCategoryInput();
        }

        public string GetPrefabPath()
        {
            return Constants.ITEM_LIST_CELL_PREFAB_PATH;
        }

        public void InitCell(int index, ScrollViewCell cell)
        {
            if (index < 0)
            {
                LogManager.LogError($"Index[{index}] is out of range for shop menu configs.");
                return;
            }
            if (!ConvertConfig<ItemListCell>(cell, out var listCell) || listCell == null)
            {
                LogManager.LogError($"Cell {cell.GetType()}, passed to shop menu but expected ItemListCell");
                return;
            }
            // If we go past the end return the empty item we added
            var config = index < activeConfigs.Count ? activeConfigs[index] : ItemListCell.EmptyConfig;
            listCell.Init(config);
        }

        public void OnAfterLoad()
        {

        }

        public Vector2 GetCellSize()
        {
            return Grid.cellSize;
        }

        public int GetNumberOfCells()
        {
            return activeConfigs.Count;
        }

        private void SetUpMenu(string shopId)
        {
            LoadBuyConfigs(shopId);
            LoadSellConfigs();
            activeConfigs = buyConfigs;
            ScrollView.Init(this, OnChange, OnClick);
        }

        private void LoadBuyConfigs(string shopId)
        {
            var items = ServiceManager.Get<GameData>().Items;
            var inventory = GetInventory(shopId);
            foreach (var id in inventory)
            {
                var item = items.Contains(id) ? items[id] : null;
                if (item == null)
                    continue;
                buyConfigs.Add(new ItemListCell.Config
                {
                    Id = item.Id,
                    ShowIcon = false,
                    Name = item.GetName(),
                    Amount = $"{item.Price}",
                    Description = item.Description
                });
            }
        }

        private List<string> GetInventory(string shopId)
        {
            var shops = ServiceManager.Get<GameData>().Shops;
            if (!shops.TryGetValue(shopId, out var shop))
            {
                LogManager.LogError($"ShopId [{shopId}] is not present in GameData Shops.");
                return new List<string>();
            }
            return shop.GetAllAvilableItems();
        }

        private void LoadSellConfigs()
        {
            var playerInventory = ServiceManager.Get<World>().GetUseItemsList();
            foreach (var item in playerInventory)
                sellConfigs.Add(new ItemListCell.Config
                {
                    Id = item.ItemInfo.Id,
                    ShowIcon = false,
                    Name = item.ItemInfo.GetName(),
                    Amount = item.Count.ToString(),
                    Description = item.ItemInfo.Description
                });
        }

        private void OnChange(int index)
        {
            var config = index <= activeConfigs.Count - 1 ? activeConfigs[index] : ItemListCell.EmptyConfig;
            var description = config.Description;
            DescriptionText.SetText(ServiceManager.Get<LocalizationManager>().Localize(description));
            UpdateInventoryInfo(config.Id);
            if (!items.Contains(config.Id))
            {
                CharacterWidgets.ResetWidgets();
                return;
            }
            if (inSellItems)
            {
                CharacterWidgets.UpdateEquipOnly(items[config.Id]);
            }
            else
            {
                UpdateCharacterWidgets(items[config.Id]);
            }
            
        }

        private void HandleSelectItemCategoryInput()
        {
            OptionsList.HandleInput();
            
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                stack.Pop();
            }
        }

        private void HandleInScrollInput()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                inScrollView = false;
                ScrollView.HideCursor();
            }
        }

        private void SetUpOptionsList()
        {
            if (OptionsList == null)
            {
                LogManager.LogError("MenuOptionsList is null in ShopMenuState.");
                return;
            }
            var localization = ServiceManager.Get<LocalizationManager>();
            if (localization == null)
            {
                LogManager.LogError("Localization is in ShopMenuState.");
                return;
            }
            var names = new List<string>
            {
                localization.Localize("ID_BUY_TEXT"),
                localization.Localize("ID_SELL_TEXT"),
                localization.Localize("ID_EXIT_TEXT")
            };
            var config = new MenuOptionsList.Config
            {
                Names = names,
                OnChange = OnOptionChange,
                OnClick = OnOptionClick,
                VerticalSelection = false
            };
            OptionsList.Init(config);
        }

        private void OnOptionChange(int index)
        {
            if (index > 1)
            {
                return;
            }
            if (index == 0)
            {
                inSellItems = false;
                activeConfigs = buyConfigs;
            }
            else if (index == 1)
            {
                inSellItems = true;
                activeConfigs = sellConfigs;
            }
            ScrollView.Refresh();
        }

        private void OnOptionClick(int index)
        {
            if (index > 1)
            {
                stack.Pop();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                inScrollView = true;
                ScrollView.ShowCursor();
            }
        }

        public void OnClick(int index)
        {
            if (index < 0 || index > activeConfigs.Count)
                return;
            var config = activeConfigs[index];
            var item = new Item
            {
                Count = 1,
                ItemInfo = items[config.Id]
            };
            OpenSelectionBox(item);
        }

        private void OpenSelectionBox(Item item)
        {
            Action onYes;
            string text;
            if (inSellItems)
            {
                onYes = () => 
                {
                    OnSellItem(item);  
                    UpdateInventoryInfo(item.ItemInfo.Id);
                    InventoryWidget.SetGoldText(world.Gold);
                };
                text = ServiceManager.Get<LocalizationManager>().Localize("ID_SELL_CONFIRM_TEXT");
            }
            else
            {
                onYes = () => 
                {
                    OnBuyItem(item);  
                    UpdateInventoryInfo(item.ItemInfo.Id);
                    InventoryWidget.SetGoldText(world.Gold);
                };
                text = ServiceManager.Get<LocalizationManager>().Localize("ID_BUY_CONFIRM_TEXT");
            }
            text = string.Format(text, item.ItemInfo.GetName());
            var config = new Textbox.Config
            {
                ImagePath = string.Empty,
                Text = text,
                AdvanceTime = float.MaxValue,
                ShowImage = false,
                UseSelectionBox = true,
                OnSelect = (index) => 
                    {
                        if (index == 0)
                            onYes();
                    },
            };
            stack.PushTextbox(config);
        }

        private void OnBuyItem(Item item)
        {
            if (world.Gold < item.ItemInfo.Price)
            {
                var localization = ServiceManager.Get<LocalizationManager>().Localize("ID_CANNOT_AFFORD_TEXT");
                var text = string.Format(localization, item.ItemInfo.GetName());
                var config = new Textbox.Config
                {
                    Text = text,
                    ShowImage = false
                };
                stack.PushTextbox(config, TextBoxAnchor.Center);
                return;
            }
            world.Gold -= item.ItemInfo.Price;
            world.AddItem(item);
            // TODO play sound effect
        }

        private void OnSellItem(Item item)
        {
            world.RemoveItem(item.ItemInfo.Id, 1);
            world.Gold += item.ItemInfo.SellPrice;
            var localization = ServiceManager.Get<LocalizationManager>().Localize("ID_SOLD_ITEM_TEXT");
            var text = string.Format(localization, item.ItemInfo.GetName());
            var config = new Textbox.Config
            {
                Text = text,
                ShowImage = false
            };
            stack.PushTextbox(config, TextBoxAnchor.Center);
            // TODO play sound effect
        }

        private void SetUpInventoryWidget()
        {
            if (InventoryWidget == null)
            {
                LogManager.LogError("InventoryInfoWidget is null in WeaponShopMenu.");
                return;
            }
            InventoryWidget.SetGoldText(world.Gold);
            InventoryWidget.SetValues(0,0);
        }

        private void UpdateInventoryInfo(string id)
        {
            var inventoryAmount = world.GetItemCount(id);
            var equippedAmount = party.GetEquippedCount(id);
            InventoryWidget.SetValues(inventoryAmount, equippedAmount);
        }

        private void UpdateCharacterWidgets(ItemInfo item)
        {
            CharacterWidgets.UpdateForItem(item);
        }
    }
}