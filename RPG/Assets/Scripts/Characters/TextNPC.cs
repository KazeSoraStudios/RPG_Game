using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG_Character
{
    public class TextNPC : MonoBehaviour, Trigger
    {
        public string Id;

        private string text = string.Empty;

        void Awake()
        {
            if (Id.IsEmptyOrWhiteSpace())
            {
                LogManager.LogError($"{name} does not have an Id, cannot get text from GameData.");
                return;
            }
            // Get from gamedata
            //ServiceManager.Get<GameData>().
            text = "This forest is 1,000 years old and filled with goblins. Be careful!";
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
            var stack = ServiceManager.Get<GameLogic>().Stack;
            stack.PushTextbox(text, string.Empty);
        }
    }
}
