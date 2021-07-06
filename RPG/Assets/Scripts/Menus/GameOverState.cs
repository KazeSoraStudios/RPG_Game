using System.Collections.Generic;
using UnityEngine;
using RPG_GameState;

namespace RPG_UI
{
    public class GameOverState : ConfigMonoBehaviour, IGameState
    {
        public class Config
        {
            public StateStack Stack;
            public World World;
            public bool won = false;
        }

        [SerializeField] MenuOptionsList MenuList;

        private bool showContinue;
        private Config config;

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, GetName()))
                return;
            this.config = config;
            if (MenuList == null)
            {
                LogManager.LogError("MenuOptionsList is null in GameOverState");
                return;
            }
            var menuConfig = new MenuOptionsList.Config
            {
                OnClick = OnSelect
            };
            MenuList.Init(menuConfig);
            var dataManager = ServiceManager.Get<GameStateManager>();
            showContinue = dataManager.GetNumberOfSaves() > 0;
            if (!showContinue)
            {
                MenuList.SetTextColor(0, Color.gray);
            }
            // TODO create continue state menu and pass in
        }

        public void Enter(object o = null) { }

        public bool Execute(float deltaTime) { return false;}

        public void Exit() { }

        public string GetName()
        {
            return "GameOverState";
        }

        public void HandleInput()
        {
            if (config.won)
                return;
            MenuList.HandleInput();
        }

        private void OnSelect(int index)
        {
            if (!showContinue && index == 0)
                return;
            if (index == 1)
                //Load
                //index = index;
                return;
            else if(index == 2)
            {
                var events = new List<IStoryboardEvent>(); // TODO SetupNewGame()
                var storyboard = new Storyboard(config.Stack, events);
                config.Stack.Push(storyboard);
                storyboard.Execute(0);
            }
        }
    }
}