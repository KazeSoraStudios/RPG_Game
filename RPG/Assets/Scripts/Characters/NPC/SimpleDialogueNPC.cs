using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_GameData;

namespace RPG_Character
{
    public class SimpleDialogueNPC : MonoBehaviour, Trigger
    {
        public string Id;

        private string text = string.Empty;
        private string portrait = string.Empty;

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

        void Start()
        {
            int x = (int)transform.position.x;
            int y = (int)transform.position.y;
            GetComponent<Character>().Map.AddTrigger(x, y, this);
        }

        public void SetText(string text)
        {
            if (Id.IsEmptyOrWhiteSpace())
            {
                LogManager.LogError($"Text passed to SetText is Empty or Null.");
                return;
            }
            var message = ServiceManager.Get<LocalizationManager>().Localize(text);
            text = message;
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
            stack.PushTextbox(text, true, portrait);
        }
    }
}
