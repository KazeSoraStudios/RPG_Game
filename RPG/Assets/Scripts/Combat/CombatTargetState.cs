using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;

namespace RPG_Combat
{
    public enum CombatTargetType { One, Side, All };

    public class CombatTargetState : ConfigMonoBehaviour, IGameState
    {
        public class Config
        {
            public bool CanSwitchSides = true;
            public CombatTargetType CombatTargetType = CombatTargetType.One;
            public CombatGameState GameState;
            public Func<CombatGameState, bool, List<Actor>> DefaultSelector;
            public Action<List<Actor>> OnSelect;
            public Action OnExit;
        }

        [SerializeField] RectTransform SelectionArrow;

        private Config config;
        private List<Actor> party = new List<Actor>();
        private List<Actor> enemies = new List<Actor>();
        private List<Actor> targets = new List<Actor>();

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, GetName()))
                return;
            this.config = config;
            if (config.DefaultSelector == null)
            {
                if (config.CombatTargetType == CombatTargetType.One) // TODO pass list as reference to avoid creating new list
                    config.DefaultSelector = (state, hurt) => CombatSelector.FindWeakestEnemy(state);
                else if (config.CombatTargetType == CombatTargetType.Side)
                    config.DefaultSelector = (state, hurt) => CombatSelector.Enemies(state);
                else if (config.CombatTargetType == CombatTargetType.All)
                    config.DefaultSelector = (state, hurt) => CombatSelector.SelectAll(state);
                else
                {
                    LogManager.LogError($"$Unkown Type of CombatTargetType [{config.CombatTargetType}]. Setting to Weakest Enemy.");
                    config.DefaultSelector = (state, hurt) => CombatSelector.FindWeakestEnemy(state);
                }
            }
        }

        public void Enter(object o = null)
        {
            gameObject.SafeSetActive(true);
            if (config.GameState.EnemyActors.Count > 0)
                enemies.AddRange(config.GameState.EnemyActors);
            if (config.GameState.PartyActors.Count > 0)
                party.AddRange(config.GameState.PartyActors);
            targets = config.DefaultSelector(config.GameState, false);
        }

        public bool Execute(float deltaTime)
        {
            return true;
        }

        public void Exit()
        {
            enemies.Clear();
            party.Clear();
            targets.Clear();
            config.OnExit?.Invoke();
            gameObject.SafeSetActive(false);
        }

        public string GetName()
        {
            return "CombatTargetState";
        }

        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
                config.GameState.CombatStack.Pop();
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                Up();
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                Down();
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                Left();
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                Right();
            else if (Input.GetKeyDown(KeyCode.Space))
                config.OnSelect?.Invoke(targets);
        }

        private List<Actor> GetActorList(Actor actor)
        {
            var inParty = config.GameState.IsPartyMember(actor);
            return inParty ? party : enemies;
        }

        private void Up()
        {
            if (config.CombatTargetType != CombatTargetType.One)
                return;
            if (!AreTargetsOkay("up"))
                return;
            var target = targets[0];
            var side = GetActorList(target);
            var index = GetActorIndex(side, target);
            index --;
            if (index <= 0)
                index = side.Count - 1;
            targets.Clear();
            targets.Add(side[index]);
        }

        private void Down()
        {
            if (config.CombatTargetType != CombatTargetType.One)
                return;
            if (!AreTargetsOkay("down"))
                return;
            var target = targets[0];
            var side = GetActorList(target);
            var index = GetActorIndex(side, target);
            index++;
            if (index >= side.Count)
                index = 0;
            targets.Clear();
            targets.Add(side[index]);
        }

        private void Left()
        {
            if (!config.CanSwitchSides)
                return;
            if (!AreTargetsOkay("left"))
                return;
            if (!config.GameState.IsPartyMember(targets[0]))
                return;
            targets.Clear();
            if (config.CombatTargetType == CombatTargetType.One)
                targets.Add(enemies[0]);
            else if (config.CombatTargetType == CombatTargetType.Side)
                targets.AddRange(enemies);
        }

        private void Right()
        {
            if (!config.CanSwitchSides)
                return;
            if (!AreTargetsOkay("right"))
                return;
            if (config.GameState.IsPartyMember(targets[0]))
                return;
            targets.Clear();
            if (config.CombatTargetType == CombatTargetType.One)
                targets.Add(party[0]);
            else if (config.CombatTargetType == CombatTargetType.Side)
                targets.AddRange(party);
        }

        public int GetActorIndex(List<Actor> actors, Actor actor)
        {
            for(int i = 0; i < actors.Count; i++)
                if (actors[i].Id == actor.Id)
                    return i;
            return -1;
        }

        private bool AreTargetsOkay(string direction)
        {
            if (targets == null || targets.Count < 1)
            {
                LogManager.LogError("Targets is null or empty in CombatTargetstate Cannot move up.");
                return false;
            }
            return true;
        }

    /*
function CombatTargetState:Render(renderer)
for k, v in ipairs(self.mTargets) do
    local char = self.mCombatState.mActorCharMap[v]

    --
    -- GetTargetPosition is new!
    --
    local pos = char.mEntity:GetTargetPosition()
    pos:SetX(pos:X() + self.mMarkerWidth/2)
    self.mMarker:SetPosition(pos)
    renderer:DrawSprite(self.mMarker)
end
end     */
}
}