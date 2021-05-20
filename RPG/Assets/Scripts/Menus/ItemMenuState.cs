using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG_GameData;

namespace RPG_UI
{
    public class ItemMenuState : ConfigMonoBehaviour, IGameState, IScrollHandler
    {
        public class Config
        {
            public InGameMenu Parent;
            public List<Item> Items = new List<Item>();
            public List<Item> KeyItems = new List<Item>();
        }

        [SerializeField] RectTransform[] Categories;
        [SerializeField] TextMeshProUGUI DescriptionText;
        [SerializeField] ScrollView ScrollView;
        [SerializeField] GridLayoutGroup Grid;
        [SerializeField] Image SelectionArrow;

        private bool inScrollView;
        private bool inKeyItems;
        private int categoryIndex = 0;
        private InGameMenu parent;
        private StateStack stack;
        private StateMachine stateMachine;
        private List<ItemListCell.Config> useConfigs = new List<ItemListCell.Config>();
        private List<ItemListCell.Config> keyConfigs = new List<ItemListCell.Config>();
        private List<ItemListCell.Config> activeConfigs = new List<ItemListCell.Config>();

        public void Enter(object o = null)
        {
            if (CheckUIConfigAndLogError(o, GetName()) || ConvertConfig<Config>(o, out var config) && config == null)
                return;
            gameObject.SafeSetActive(true);
            parent = config.Parent;
            stack = parent.Stack;
            stateMachine = parent.StateMachine;
            inScrollView = false;
            inKeyItems = false;
            ScrollView.HideCursor();
            useConfigs.Clear();
            keyConfigs.Clear();
            activeConfigs.Clear();
            SetUpMenu(config);
        }

        public bool Execute(float deltaTime)
        {
            HandleInput();
            ScrollView.Execute();
            return true;
        }

        public void Exit()
        {
            ScrollView.ClearCells();
            gameObject.SafeSetActive(false);
        }

        public string GetName()
        {
            return "ItemMenuState";
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
                LogManager.LogError($"Index[{index}] is out of range for item menu configs.");
                return;
            }
            if (!ConvertConfig<ItemListCell>(cell, out var listCell) || listCell == null)
            {
                LogManager.LogError($"Cell {cell.GetType()}, passed to item menu but expected ItemListCell");
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

        private void SetUpMenu(Config config)
        {
            foreach (var use in config.Items)
                useConfigs.Add(new ItemListCell.Config
                {
                    ShowIcon = true,
                    Name = use.ItemInfo.GetName(),
                    Amount = use.Count.ToString(),
                    Description = use.ItemInfo.Description
                });

            foreach (var key in config.KeyItems)
                useConfigs.Add(new ItemListCell.Config
                {
                    ShowIcon = true,
                    Name = key.ItemInfo.GetName(),
                    Amount = key.Count.ToString(),
                    Description = key.ItemInfo.Description
                });
            activeConfigs = useConfigs;
            ScrollView.Init(this, SetDescription);
        }

        private void SetDescription(int index)
        {
            var config = index < activeConfigs.Count - 1 ? activeConfigs[index] : ItemListCell.EmptyConfig;
            var description = config.Description;
            DescriptionText.SetText(ServiceManager.Get<LocalizationManager>().Localize(description));
        }

        private bool CanUseItem(Item item)
        {
            var use = item.ItemInfo.Use;
            return ServiceManager.Get<GameData>().ItemUses[use].UseOnMap;
        }
        private void HandleSelectItemCategoryInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                categoryIndex = categoryIndex == 0 ? 1 : 0;
                SetSelectionPosition();
                activeConfigs = categoryIndex == 0 ? useConfigs : keyConfigs;
                ScrollView.Refresh();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                categoryIndex = categoryIndex == 0 ? 1 : 0;
                SetSelectionPosition();
                activeConfigs = categoryIndex == 0 ? useConfigs : keyConfigs;
                ScrollView.Refresh();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                inScrollView = true;
                ScrollView.ShowCursor();
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                stateMachine.Change(Constants.FRONT_MENU_STATE, parent.FrontConfig);
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

        private void SetSelectionPosition()
        {
            var transform = (RectTransform)Categories[categoryIndex].transform;
            var categoryPosition = transform.position;
            var categoryWidthOffset = transform.rect.width * 0.5f;
            var arrowWidthOffset = SelectionArrow.GetComponent<RectTransform>().rect.width * 0.5f;
            var xPosition = categoryPosition.x - categoryWidthOffset - arrowWidthOffset - 10.0f;
            var position = new Vector2(xPosition, categoryPosition.y);
            SelectionArrow.transform.position = position;
            activeConfigs = categoryIndex == 0 ? useConfigs : keyConfigs;
        }

        private void OnUseItem(Item item)
        {
            var useId = item.ItemInfo.Use;
            var itemUses = ServiceManager.Get<GameData>().ItemUses;
            if (!itemUses.ContainsKey(useId) || !itemUses[useId].UseOnMap)
                return;
            // TODO item use function
        }

        /*


    function ItemMenuState:OnUseItem(index, item)
        local itemDef = ItemDB[item.id]
        if not self:CanUseItem(itemDef) then
            return
        end

        local selectId = itemDef.use.target.selector

        local targetState = MenuTargetState:Create
        {
            originState = self,
            stack = gGame.Stack,
            stateMachine = self.mStateMachine,
            targetType = itemDef.use.target.type,
            selector = MenuActorSelector[selectId],
            OnCancel = function(target) print("Cancelled") end,
            OnSelect = function(targets) self:OnItemTargetsSelected(itemDef, targets) end
        }
        gGame.Stack:Push(targetState)
    end

    function ItemMenuState:OnItemTargetsSelected(itemDef, targets)

        local action = itemDef.use.action
        CombatActions[action](self,
                           nil, -- the person using the item
                           targets,
                           itemDef,
                           "item")

        gGame.World:RemoveItem(itemDef.id)
    end


    function ItemMenuState:OnCategorySelect(index, value)
        self.mCategoryMenu:HideCursor()
        self.mInCategoryMenu = false
        local menu = self.mItemMenus[index]
        menu:ShowCursor()
    end

    function ItemMenuState:Update(dt)

        local menu = self.mItemMenus[self.mCategoryMenu:GetIndex()]

        if self.mInCategoryMenu then
            if  Keyboard.JustReleased(KEY_BACKSPACE) or
                Keyboard.JustReleased(KEY_ESCAPE) then
                self.mStateMachine:Change("frontmenu")
            end
            self.mCategoryMenu:HandleInput()
        else

            if  Keyboard.JustReleased(KEY_BACKSPACE) or
                Keyboard.JustReleased(KEY_ESCAPE) then
                self:FocusOnCategoryMenu()
            end

            menu:HandleInput()
        end

        local scrolled = menu:PercentageScrolled()
        self.mScrollbar:SetScrollCaretScale(menu:PercentageShown())
        self.mScrollbar:SetNormalValue(scrolled)
    end

    function ItemMenuState:FocusOnCategoryMenu()
        self.mInCategoryMenu = true
        local menu = self.mItemMenus[self.mCategoryMenu:GetIndex()]
        menu:HideCursor()
        self.mCategoryMenu:ShowCursor()
    end


         */
    }
}