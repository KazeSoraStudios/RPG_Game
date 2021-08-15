using System.Collections.Generic;
using UnityEngine;
using RPG_GameData;
using RPG_UI;

namespace RPG_Character
{
    public class BattleNPC : MonoBehaviour, Trigger
    {
        [SerializeField] string BattleId;
        [SerializeField] List<YokaiHuman> Demons;

        private Battle battle;

        private void Start() 
        {
            var battles = ServiceManager.Get<GameData>().Battles;
            if (!battles.ContainsKey(BattleId))
            {
                LogManager.LogError($"BattleId {BattleId} not found in GameData Battles for NPC {name}.");
                return;
            }
            battle = battles[BattleId];
            var gameState = ServiceManager.Get<GameLogic>().GameState;
            if (gameState.IsEventComplete(battle.Area, battle.Id))
                enabled = false;
        }

        public void OnEnter(TriggerParams triggerParams)
        {

        }

        public void OnExit(TriggerParams triggerParams)
        {

        }

        public void OnStay(TriggerParams triggerParams)
        {

        }

        public void OnUse(TriggerParams triggerParams)
        {
            if (battle == null)
            {
                LogManager.LogError($"{name} has a null battle.");
                return;
            }
            var gameState = ServiceManager.Get<GameLogic>().GameState;
            if (gameState.IsEventComplete(battle.Area, battle.Id))
            {
                this.enabled = false;
                return;
            }
            var localization = ServiceManager.Get<LocalizationManager>();
            var localizedText = new List<string>();
            foreach (var id in battle.BeforeText)
                localizedText.Add(localization.Localize(id));
            var config = new Textbox.Config
            {
                AdvanceTime = 999999.0f,
                ShowImage = false,
                Text = localizedText,
                OnFinish = StartCombat
            };
            var stack = ServiceManager.Get<GameLogic>().Stack;
            stack.PushTextbox(config);
        }

        private void StartCombat()
        {
            var config = new Actions.StartCombatConfig
            {
                CanFlee = battle.CanFlee,
                Enemies = battle.Enemies,
                Party = ServiceManager.Get<Party>().Members,
                OnWin = OnWin,
                Stack = ServiceManager.Get<GameLogic>().Stack
            };
            Actions.Combat(config);
        }

        private void OnWin() 
        {
            var gameState = ServiceManager.Get<GameLogic>().GameState;
            gameState.CompleteEventInArea(battle.Area, battle.Id);
            var localization = ServiceManager.Get<LocalizationManager>();
            var localizedText = new List<string>();
            foreach (var id in battle.AfterText)
                localizedText.Add(localization.Localize(id));
            var config = new Textbox.Config
            {
                AdvanceTime = 999999.0f,
                ShowImage = false,
                Text = localizedText,
                OnFinish = GiveItem
            };
            var stack = ServiceManager.Get<GameLogic>().Stack;
            stack.PushTextbox(config);
        }

        private void GiveItem()
        {
            if (battle.Reward.IsEmptyOrWhiteSpace())
            {
                ChangeDemonsToHumans();
                return;
            }
            var localization = ServiceManager.Get<LocalizationManager>();
            var message = localization.Localize("ID_REWARD_ITEM_TEXT");
            var items = ServiceManager.Get<GameData>().Items;
            if (!items.Contains(battle.Reward))
            {
                LogManager.LogError($"Reward {battle.Reward} not found in GameData Items.");
                return;
            }
            var itemName = items[battle.Reward].GetName();
            var localizedText = new List<string>
            {
                string.Format(message, itemName)
            };
            var config = new Textbox.Config
            {
                AdvanceTime = 999999.0f,
                ShowImage = false,
                Text = localizedText,
                OnFinish = ChangeDemonsToHumans
            };
            var stack = ServiceManager.Get<GameLogic>().Stack;
            stack.PushTextbox(config, TextBoxAnchor.Center);
        }

        private void ChangeDemonsToHumans()
        {
            foreach (var yokai in Demons)
                yokai.ChangeToHuman();
            this.enabled = false;
        }
    }
}
