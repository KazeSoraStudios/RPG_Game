using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_Combat;

namespace RPG_UI
{
    public class XPSummaryState : ConfigMonoBehaviour, IGameState
    {
        public class Config
        {
            public StateStack Stack;
            public List<Actor> Party;
            public LootData Loot;
        }

        private Config config;

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, GetName()))
                return;
            this.config = config;
        }

        public void Enter(object o = null)
        {
            throw new System.NotImplementedException();
        }

        public bool Execute(float deltaTime)
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        public string GetName()
        {
            return "XPSummaryState";
        }

        public void HandleInput()
        {
            throw new System.NotImplementedException();
        }
    }
}