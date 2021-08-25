using UnityEngine;

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
                LogManager.LogWarn($"{name} does not have an Id, cannot get text from GameData.");
                return;
            }
            text = ServiceManager.Get<LocalizationManager>().Localize(text);
        }

        public void SetText(string text)
        {
            if (text.IsEmptyOrWhiteSpace())
            {
                LogManager.LogError($"Text passed to SetText is Empty or Null.");
                return;
            }
            var message = ServiceManager.Get<LocalizationManager>().Localize(text);
            this.text = message;
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
            stack.PushTextbox(text, false, portrait);
        }
    }
}
