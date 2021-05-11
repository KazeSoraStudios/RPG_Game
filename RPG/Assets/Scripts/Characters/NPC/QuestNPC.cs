using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_GameData;
namespace RPG_Character
{
    public class QuestNPC : MonoBehaviour, Trigger
    {
        [SerializeField] string QuestId;
        [SerializeField] string BeforeCompletionText;
        [SerializeField] string AfterCompletionText;

        private Quest quest;

        private void Start()
        {
            var quests = ServiceManager.Get<GameData>().Quests;
            if (!quests.ContainsKey(QuestId))
            {
                LogManager.LogError($"Quest [{QuestId}] not found in GameData for NPC [{name}].");
                return;
            }
            quest = quests[QuestId];
            var message = quest.IsComplete ? AfterCompletionText : BeforeCompletionText;
        }

        public void OnEnter(TriggerParams triggerParams)
        {
            
        }

        public void OnExit(TriggerParams triggerParams)
        {
            throw new System.NotImplementedException();
        }

        public void OnStay(TriggerParams triggerParams)
        {
            throw new System.NotImplementedException();
        }

        public void OnUse(TriggerParams triggerParams)
        {
            if (quest == null)
                return;
            quest.TryToComplete();
            var stack = ServiceManager.Get<GameLogic>().Stack;
            var text = BeforeCompletionText;
            if (quest.IsComplete)
            {
                text = AfterCompletionText;
                var reward = quest.CreateReward();
                var world = ServiceManager.Get<World>();
                world.GiveReward(reward);
            }
            
            stack.PushTextbox(text, false);
        }
    }

}