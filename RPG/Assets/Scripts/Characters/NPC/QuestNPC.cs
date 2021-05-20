using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_GameData;
using RPG_GameState;

namespace RPG_Character
{
    public class QuestNPC : MonoBehaviour, Trigger
    {
        [SerializeField] string QuestId;
        [SerializeField] string BeforeStartedText;
        [SerializeField] string StartedText;
        [SerializeField] string OnCompleteText;
        [SerializeField] string CompletedText;

        private Quest quest;

        private void Start()
        {
            var position = Vector2Int.RoundToInt((Vector2)transform.position);
            ServiceManager.Get<TriggerManager>().AddTrigger(position, this);

            quest = GetQuest();
            if (quest == null)
            {
                gameObject.SafeSetActive(false);
                LogManager.LogError($"NPC [{name}] could not find quest [{QuestId}]. Turning off QuestNPC.");
                return;
            }
            var message = quest.IsComplete ? CompletedText : StartedText;
        }

        public void OnEnter(TriggerParams triggerParams)
        {
            
        }

        public void OnExit(TriggerParams triggerParams) { }

        public void OnStay(TriggerParams triggerParams) { }

        public void OnUse(TriggerParams triggerParams)
        {
            if (quest == null)
                return;
            var completed = quest.TryToComplete();
            var stack = ServiceManager.Get<GameLogic>().Stack;
            var text = GetMessage(completed);
            var world = ServiceManager.Get<World>();
            if (quest.IsComplete)
            {
                if (!world.HasQuest(quest.Id))
                    world.AddQuest(quest);
                world.CompleteQuest(quest);
            }
            else
            {
                if (!world.HasQuest(quest.Id))
                    world.AddQuest(quest);
            }
            stack.PushTextbox(text, false);
        }

        private Quest GetQuest()
        {
            var quests = ServiceManager.Get<GameData>().Quests;
            if (!quests.ContainsKey(QuestId))
            {
                LogManager.LogError($"Quest [{QuestId}] not found in GameData for NPC [{name}].");
                return null;
            }
            var quest = quests[QuestId];
            var gameState = ServiceManager.Get<GameStateManager>().GetCurrent();
            if (gameState != null && gameState.GetQuestData(QuestId) is var data && data != null)
            { 
                quest.IsComplete = data.IsComplete;
                quest.IsStarted = data.IsStarted;
            }
            return quest;
        }

        private string GetMessage(bool justCompleted)
        {
            if (justCompleted)
                return OnCompleteText.IsEmptyOrWhiteSpace() ? CompletedText : OnCompleteText;
            if (quest.IsComplete)
                return CompletedText.IsEmptyOrWhiteSpace() ? BeforeStartedText : CompletedText;
            if (quest.IsStarted)
                return StartedText.IsEmptyOrWhiteSpace() ? BeforeStartedText : StartedText;
            return BeforeStartedText;
        }
    }

}