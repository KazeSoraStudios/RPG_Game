using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_AI;
using RPG_Character;
using RPG_Combat;
using RPG_GameData;

namespace RPG_CombatSim
{
    public abstract class CombatSimEvent : IEvent
    {
        protected bool finished;
        protected int priority;
        protected ICombatState state;
        protected Actor actor;

        public CombatSimEvent(Actor actor, ICombatState state)
        {
            this.actor = actor;
            this.state = state;
        }

        public virtual void Execute(EventQueue queue) { }

        public virtual void Update() { }

        public Actor GetActor()
        {
            return actor;
        }

        public int GetPriority()
        {
            return priority;
        }

        public abstract string GetName();

        public virtual bool IsFinished()
        {
            return finished;
        }

        public void SetPriority(int value)
        {
            priority = value;
        }

        public virtual int CalculatePriority(EventQueue queue)
        {
            if (queue == null)
                return 0;
            var speed = actor.Stats.Get(Stat.Speed);
            return queue.SpeedToPoints(speed);
        }
    }

    public class CESimTurn : CombatSimEvent
    {
        public CESimTurn (Actor actor,  ICombatState state)
            : base(actor, state) { }

        public override void Execute(EventQueue queue)
        {
            LogManager.LogDebug($"Executing CETurn for {actor.name}");
            // Player first
            if (!state.IsPartyMember(actor))
            {
                LogManager.LogError($"AI Character {actor.name} has player turn event.");
                finished = true;
                return;
            }
            HandlePlayerTurn();
        }

        private void HandlePlayerTurn()
        {
            var config = new CSEAttack.Config
            {
                Actor = actor,
                IsPlayer = true,
                IsCounter = false,
                State = state,
                Targets = CombatSelector.FindWeakestEnemy(state)
            };
            var attackEvent = new CSEAttack(config);
            state.CombatTurnHandler().AddEvent(attackEvent, -1);
            finished = true;
        }

        public override string GetName()
        {
            return $"CombatSimEventTurn for {actor.name}";
        }
    }

    public class CSEAITurn : CombatSimEvent
    {
        public CSEAITurn (Actor actor,  ICombatState state)
            : base(actor, state) { }

        public override void Execute(EventQueue queue)
        {
            LogManager.LogDebug($"Executing CETurn for {actor.name}");
            HandleEnemyTurn();
        }

        private void HandleEnemyTurn()
        {
            var ai = ServiceManager.Get<GameData>().EnemyAI;
            if (!ai.ContainsKey(actor.GameDataId))
            {
                LogManager.LogError($"SimActor {actor.name} GameDataId {actor.GameDataId} not found in EnemyAI.");
                finished = true;
                return;
            }
            var aiData = ai[actor.GameDataId];
            var tree = state.GetBehaviorTreeForAIType(aiData.Type);
            if (tree == null)
            {
                LogManager.LogError($"Behavior Tree type {aiData.Type} not found in CombatGameState for SimActor{actor.name}.");
                finished = true;
                return;
            }
            if (tree.Evaluate(actor) == NodeState.Failure)
            {
                LogManager.LogError($"Actor [{actor.name}] decision tree failed. Attacking random enemy.");
                var targets = new List<Actor>();
                targets.AddRange(CombatSelector.RandomPlayer(state));
                var config = new CSEAttack.Config
                {
                    IsCounter = false,
                    IsPlayer = false,
                    Actor = actor,
                    State = state,
                    Targets = targets
                };
                var attackEvent = new CSEAttack(config);
                var turnHandler = state.CombatTurnHandler();
                turnHandler.AddEvent(attackEvent, -1);
            }
            finished = true;
        }

        public override string GetName()
        {
            return $"CombatSimEventAITurn for {actor.name}";
        }
    }

    public class CSEAttack : CombatSimEvent
    {
        public class Config
        {
            public bool IsCounter;
            public bool IsPlayer;
            public Actor Actor;
            public ICombatState State;
            public List<Actor> Targets = new List<Actor>();
        }

        private bool counter;
        private bool isPlayer;
        private FormulaResult result;
        private Character character;
        private Func<ICombatState, List<Actor>> targeter;
        private List<Actor> targets;

        public CSEAttack(Config config) : base(config.Actor, config.State)
        {
            isPlayer = config.IsPlayer;
            targets = config.Targets;
            counter = config.IsCounter;
            character = actor.GetComponent<Character>();
            if (character == null)
            {
                LogManager.LogError($"Character is null for actor {config.Actor}");
                return;
            }
        }

        public CSEAttack(CEAttack.Config config) : base(config.Actor, config.CombatState)
        {
            isPlayer = config.IsPlayer;
            targets = config.Targets;
            counter = config.IsCounter;
            character = actor.GetComponent<Character>();
            if (character == null)
            {
                LogManager.LogError($"Character is null for actor {config.Actor}");
                return;
            }
        }

        public override void Execute(EventQueue queue)
        {
            LogManager.LogDebug($"Executing CEAttack for {actor.name}");
            for (int i = targets.Count - 1; i > -1; i--)
            {
                var hp = targets[i].Stats.Get(Stat.HP);
                if (hp <= 0)
                    targets.RemoveAt(i);
            }
            if (targets.Count < 1)
                targets.AddRange(targeter(state));
            targeter = (state) => isPlayer ? CombatSelector.FindWeakestEnemy(state) : CombatSelector.RandomPlayer(state);
            DoAttack();
            ShowResult();
            OnFinish();
        }

        private void OnFinish()
        {
            finished = true;        
        }

        private void DoAttack()
        {
            foreach (var target in targets)
            {
                AttackTarget(target);
                if (!counter)
                    CounterTarget(target);
            }
        }

        private void CounterTarget(Actor target)
        {
            var countered = CombatFormula.IsCounter(actor, target);
            if (countered)
            {
                LogManager.LogDebug($"Countering [{actor}] with Target [{target}]");
                CombatActions.ApplyCounter(target, actor, state);
            }
        }

        private void AttackTarget(Actor target)
        {
            LogManager.LogDebug($"Attacking {target.name}");
            result = CombatFormula.MeleeAttack(actor, target);
            var entity = target.GetComponent<Entity>();
            if (result.Result == CombatFormula.HitResult.Miss)
            {
                CombatActions.ApplyMiss(target);
                return;
            }
            if (result.Result == CombatFormula.HitResult.Dodge)
                CombatActions.ApplyDodge(target);
            else
            {
                var isCrit = result.Result == CombatFormula.HitResult.Critical;
                CombatActions.ApplyDamage(target, result.Damage, isCrit);
            }
        }

        private void ShowResult()
        {
            if (result == null)
                return;
            state.CombatReporter().ReportResult(result, actor.name);
        }

        private string CreateResultMessage()
        {
            var localization = ServiceManager.Get<LocalizationManager>();
            if (result.Result == CombatFormula.HitResult.Miss)
            {
                var message = localization.Localize("ID_MISS_TEXT");
                return string.Format(message, actor.Name);
            }
            else if (result.Result == CombatFormula.HitResult.Dodge)
                return localization.Localize("ID_DODGE_TEXT");
            else if (result.Result == CombatFormula.HitResult.Hit)
            { 
                var message = localization.Localize("ID_ATTACK_HIT_TEXT");
                return string.Format(message, result.Damage);
            }
            else
            {
                var message = localization.Localize("ID_CRITICAL_HIT_TEXT");
                return string.Format(message, result.Damage);
            }
        }

        public override string GetName()
        {
            return "CombatSimEventAttack";
        }
    }

    public class CSEUseItemEvent : CombatSimEvent
    {
        public class Config
        {
            public bool IsPlayer;
            public Actor Actor;
            public ICombatState CombatState;
            public ItemInfo Item;
            public ItemUse ItemUse;
            public List<Actor> Targets = new List<Actor>();
        }

        private ItemInfo item;
        private ItemUse itemUse;
        private List<Actor> targets;
        public CSEUseItemEvent(Config config)
            : base(config.Actor, config.CombatState)
        {
            item = config.Item;
            itemUse = config.ItemUse;
            targets = config.Targets;
            finished = false;
        }

        public override void Execute(EventQueue queue)
        {
            LogManager.LogDebug($"Executing CEUseItemEvent for {actor.name}");
            ServiceManager.Get<World>().RemoveItem(item.Id);
            UseItem();
            Finish();
        }

        private void UseItem()
        {
            var config = new CombatActionConfig
            {
                Owner = actor,
                StateId = "item",
                Targets = targets,
                Def = itemUse
            };
            CombatActions.RunAction(itemUse.Action, config);
        }

        private void Finish()
        {
            finished = true;
        }

        public override string GetName()
        {
            return "CEUseItemEvent";
        }
    }
    public class CSECastSpellEvent : CombatSimEvent
    {
        public class Config
        {
            public bool IsPlayer;
            public Actor Actor;
            public ICombatState CombatState;
            public Spell spell;
            public List<Actor> Targets = new List<Actor>();
        }

        private Spell spell;
        private List<Actor> targets;

        public CSECastSpellEvent(Config config)
            : base(config.Actor, config.CombatState)
        {
            spell = config.spell;
            targets = config.Targets;
            finished = false;
        }

        public CSECastSpellEvent(CECastSpellEvent.Config config)
            : base(config.Actor, config.CombatState)
        {
            spell = config.spell;
            targets = config.Targets;
            finished = false;
        }

        public override void Execute(EventQueue queue)
        {
            LogManager.LogDebug($"Executing CECastSpellEvent for {actor.name}");
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                var hp = targets[i].Stats.Get(Stat.HP);
                var isEnemy = !state.IsPartyMember(targets[i]);
                if (isEnemy && hp <= 0) // TODO keep enemies, possibly for enemy revive spells?
                    targets.RemoveAt(i);
            }
            if (targets.Count < 1)
                targets = spell.ItemTarget.Selector(state, true);
            Cast();
            Finish();
        }
        private void Cast()
        {
            actor.ReduceManaForSpell(spell);
            var config = new CombatActionConfig
            {
                Owner = actor,
                StateId = "magic",
                Targets = targets,
                Def = spell
            };
            CombatActions.RunAction(spell.Action, config);
        }

        private void Finish()
        {
            finished = true;
        }

        public override string GetName()
        {
            return "CECastSpellEvent";
        }
    }
}