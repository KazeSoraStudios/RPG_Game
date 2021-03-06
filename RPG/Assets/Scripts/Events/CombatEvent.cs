using System.Collections.Generic;
using UnityEngine;

public abstract class CombatEvent : IEvent
{
    protected bool finished;
    protected int priority;
    protected CombatState state;
    protected Actor actor;

    public CombatEvent(Actor actor, CombatState state)
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

    public int CalculatePriority(EventQueue queue)
    {
        if (queue == null)
            return 0;
        var speed = actor.Stats.Get(Stat.Speed);
        return queue.SpeedToPoints(speed);
    }
}

public class CETurn : CombatEvent
{
    public CETurn(Actor actor, CombatState state)
        : base(actor, state) { }

    public override void Execute(EventQueue queue)
    {
        // Player first
        if (state.IsPartyMember(actor))
            HandlePlayerTurn();
        else
            HandleEnemyTurn();
    }

    private void HandlePlayerTurn()
    {
        var config = new CombatChoiceState.Config
        {
            Actor = actor,
            State = state
        };
        // TODO make one instance
        var nextstate = new CombatChoiceState();
        nextstate.Init(config);
        state.Stack.Push(nextstate);
        finished = true;
    }

    private void HandleEnemyTurn()
    {
        var targets = new List<Actor>();
        targets.Add(CombatSelector.RandomPlayer(state));
        var config = new CEAttack.Config
        {
            IsCounter = false,
            IsPlayer = false,
            Actor = actor,
            CombatState = state,
            Targets = targets
        };
        var attackEvent = new CEAttack(config);
        var queue = state.EventQueue;
        var priority = attackEvent.CalculatePriority(queue);
        queue.Add(attackEvent, priority);
        finished = true;
    }

    public override string GetName()
    {
        return $"CombatEventTurn for {actor.name}";
    }
}

public class CEAttack : CombatEvent
{
    public class Config
    {
        public bool IsCounter;
        public bool IsPlayer;
        public Actor Actor;
        public CombatState CombatState;
        public List<Actor> Targets = new List<Actor>();
    }
    private bool counter;
    private int speed = 0;
    private Character character;
    private Storyboard storyboard;// = new Storyboard();
    private List<Actor> targets;

    public CEAttack(Config config)
        : base(config.Actor, config.CombatState)
    {
        targets = config.Targets;
        counter = config.IsCounter;
        character = state.ActorToCharacterMap[config.Actor.Id];
        if (character == null)
            LogManager.LogError($"Character is null for actor {config.Actor}");
        // set prone state

        //    if this.mDef.player then
        //        this.mAttackAnim = gEntities.slash
        //        this.mDefaultTargeter = CombatSelector.WeakestEnemy

        //        storyboard =
        //        {
        //            SOP.RunState(this.mController, CSMove.mName, {dir = 1}),
        //            SOP.RunState(this.mController, CSRunAnim.mName, {'attack', false}),
        //            SOP.Function(function() this:DoAttack() end),
        //            SOP.RunState(this.mController, CSMove.mName, {dir = -1}),
        //            SOP.Function(function() this:OnFinish() end)
        //        }

        //    else
        //        this.mAttackAnim = gEntities.claw
        //        this.mDefaultTargeter = CombatSelector.RandomAlivePlayer

        //        storyboard =
        //        {
        //            SOP.RunState(this.mController,
        //                        CSMove.mName,
        //                        {dir = 1, distance = 8, time = 0.1}),
        //            SOP.Function(function() this:DoAttack() end),
        //            SOP.RunState(this.mController,
        //                        CSMove.mName,
        //                        {dir = -1, distance = 8, time = 0.4}),
        //            SOP.Function(function() this:OnFinish() end)
        //        }

        //    this.mStoryboard = Storyboard:Create(this.mState.mStack,
        //                                         storyboard)
    }

    public override void Execute(EventQueue queue)
    {
        state.Stack.Push(storyboard);
        for(int i = targets.Count - 1; i > -1; i++)
        {
            var hp = targets[i].Stats.Get(Stat.HP);
            if (hp <= 0)
                targets.RemoveAt(i);
        }
        if (targets.Count < 1)
            targets.Add(CombatSelector.RandomPlayer(state));
    }

    public void OnFinish()
    {
        finished = true;
    }

    public void DoAttack()
    {
        foreach(var target in targets)
        {
            AttackTarget(target);
            if (!counter)
                CounterTarget(target);
        }
    }
    
    public void CounterTarget(Actor target)
    {
        var countered = CombatFormula.IsCounter(state, actor, target);
        if (countered)
            state.ApplyCounter(target, actor);
    }

    public void AttackTarget(Actor target)
    {
        var result = CombatFormula.MeleeAttack(state, actor, target);
        // TODO check null
        var entity = state.ActorToCharacterMap[target.Id].GetComponent<Entity>();
        if (result.Result == CombatFormula.HitResult.Miss)
        {
            state.ApplyMiss(target);
            return;
        }
        if (result.Result == CombatFormula.HitResult.Dodge)
            state.ApplyDodge(target);
        else 
        {
            var isCrit = result.Result == CombatFormula.HitResult.Critical;
            state.ApplyDamage(target, result.Damage, isCrit);
        }
        var entityPosition = entity.transform.position;
        // TODO
        //    local effect = AnimEntityFx:Create(x, y,
        //                            self.mAttackAnim,
        //                            self.mAttackAnim.frames)

        //    self.mState:AddEffect(effect)
    }

    public override string GetName()
    {
        return "CombatEventAttack";
    }
}