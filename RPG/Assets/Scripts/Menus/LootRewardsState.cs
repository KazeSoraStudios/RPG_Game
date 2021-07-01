using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG_Combat;
using RPG_Character;

namespace RPG_UI
{
    public class LootRewardsState : ConfigMonoBehaviour, IGameState, IScrollHandler
    {
        public class Config
        {
            public LootData Data;
            public StateStack Stack;
            public World World;
        }

        [SerializeField] float goldPerSecond = 5;
        [SerializeField] TextMeshProUGUI GoldReceivedText;
        [SerializeField] TextMeshProUGUI PartyGoldText;
        [SerializeField] ScrollView ScrollView;
        [SerializeField] GridLayoutGroup Grid;

        private bool isCountingGold = false;
        private int gold = 0;
        private float goldCounter = 0;
        private Config config;
        private List<ItemListCell.Config> lootConfigs = new List<ItemListCell.Config>();
        private ItemListCell.Config emptyConfig = ItemListCell.EmptyConfig;


        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(0, GetName()))
                return;
            this.config = config;
            gold = config.Data.Gold;
            SetGoldText(gold);
            SetPartyGoldText(config.World.Gold);
            var digitNumber = Mathf.Log10(config.Data.Gold + 1);
            goldPerSecond *= digitNumber * digitNumber;
            ScrollView.HideCursor();
            emptyConfig.Name = string.Empty;
            CreateLootConfigs();
        }

        public void Enter(object o = null)
        {
            isCountingGold = true;
            goldCounter = 0;
            foreach (var item in config.Data.Loot)
                config.World.AddItem(item);
        }

        public bool Execute(float deltaTime)
        {
            if (isCountingGold)
            {
                goldCounter += goldPerSecond * deltaTime;
                var goldToGive = Mathf.FloorToInt(goldCounter);
                goldCounter -= goldToGive;
                gold -= goldToGive;
                config.World.Gold += goldToGive;
                SetGoldText(gold);
                SetPartyGoldText(config.World.Gold);
                if (gold == 0)
                    isCountingGold = false;
            }
            return false;
        }

        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (gold > 0)    
                {
                    SkipCountingGold();
                    return;
                }
                var events = new List<IStoryboardEvent>
                {
                    StoryboardEventFunctions.BlackScreen(),
                    StoryboardEventFunctions.Wait(1.0f),
                    StoryboardEventFunctions.Function(() => 
                    {
                        ServiceManager.Get<Party>().ReturnFromCombat();
                        ServiceManager.Get<NPCManager>().ReturnFromCombat();
                        Actions.SetCameraToFollowHero();
                    }),
                    StoryboardEventFunctions.FadeScreenOut("blackscreen", 0.3f)
                };
                var storyboard = new Storyboard(config.Stack, events);
                // Draw the black screen this frame
                gameObject.SafeSetActive(false);
                storyboard.Execute(0);
                config.Stack.Pop();
                config.Stack.Push(storyboard);
                
            }
        }

        public void Exit()
        {
            Destroy(gameObject);
        }

        public string GetName()
        {
            return "LootRewardsState";
        }

        public string GetPrefabPath()
        {
            return Constants.ITEM_LIST_CELL_PREFAB_PATH;
        }

        public void InitCell(int index, ScrollViewCell cell)
        {
            if (index < 0)
            {
                LogManager.LogError($"Index[{index}] is out of range for loot configs.");
                return;
            }
            if (!ConvertConfig<ItemListCell>(cell, out var listCell) || listCell == null)
            {
                LogManager.LogError($"Cell {cell.GetType()}, passed to LootRewardState but expected ItemListCell");
                return;
            }
            var config = index < lootConfigs.Count ? lootConfigs[index] : emptyConfig;
            listCell.Init(config);
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
            return lootConfigs.Count;
        }

        private void SetGoldText(int gold)
        {
            if (GoldReceivedText == null)
            {
                LogManager.LogError("GoldReceivedText is null in LootRewardsState");
                return;
            }
            var goldText = string.Format(Constants.GOLD_FORMAT_TEXT, gold);
            GoldReceivedText.SetText(goldText);
        }

        private void SetPartyGoldText(int gold)
        {
            if (PartyGoldText == null)
            {
                LogManager.LogError("PartyGoldText is null in LootRewardsState");
                return;
            }
            var goldText = string.Format(Constants.GOLD_FORMAT_TEXT, gold);
            PartyGoldText.SetText(goldText);
        }

        private void SkipCountingGold()
        {
            isCountingGold = false;
            goldCounter = 0;
            var goldToGive = gold;
            gold = 0;
            config.World.Gold += goldToGive;
            SetPartyGoldText(config.World.Gold);
            SetGoldText(0);
        }

        private void CreateLootConfigs()
        {
            foreach (var loot in config.Data.Loot)
            lootConfigs.Add(new ItemListCell.Config
            {
                ShowIcon = false,
                Name = loot.ItemInfo.GetName(),
                Amount = loot.Count.ToString()
            });
            ScrollView.Init(this, null);
        }
    }
}