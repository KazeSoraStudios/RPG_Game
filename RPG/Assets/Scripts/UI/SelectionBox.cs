using System;
using UnityEngine;
using TMPro;

namespace RPG_UI
{
    public class SelectionBox : ConfigMonoBehaviour, IGameState
    {
        public class Config
        {
             public string Text;
            public Action OnYes;
            public Action OnNo;
        }

        [SerializeField] TextMeshProUGUI Text;
        [SerializeField] MenuOptionsList OptionsList;

        private StateStack stack;
        private Action onYes;
        private Action onNo;

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, GetName()))
                return;
            onYes = config.OnYes;
            onNo = config.OnNo;
            var optionConfig = new MenuOptionsList.Config
            {
                OnClick = OnSelect
            };
            OptionsList.Init(optionConfig);
        }

        public void SetUp(StateStack stack)
        {
            this.stack = stack;
        }

        public void Enter(object o)
        {
            gameObject.SafeSetActive(true);
        }

        public bool Execute(float deltaTime)
        {
            return false;
        }

        public void Exit()
        {
            gameObject.SafeSetActive(false);
            onYes = null;
            onNo = null;
        }

        public void HandleInput()
        {
            OptionsList.HandleInput();
        }

        public string GetName()
        {
            return "SelectionBox";
        }

        private void OnSelect(int index)
        {
            if (index == 0)
                onYes?.Invoke();
            else if (index == 1)
                onNo?.Invoke();
            else
                LogManager.LogError($"Index [{index}] is greater than Selection Box options.");
        }
    }
}
