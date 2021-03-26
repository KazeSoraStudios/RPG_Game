using System.Collections.Generic;
using UnityEngine;
using RPG_Character;

public abstract class CombatState : IState
{
    public bool canEscape = true;
    public bool escaped = false;
    protected Character character;
    protected string previousState;
    public EventQueue EventQueue;
    public StateStack Stack;
    public StateStack GameStack;
    public Actor SelectedActor;
    public List<Actor> PartyActors = new List<Actor>();
    public List<Actor> EnemyActors = new List<Actor>();
    public List<Character> DeadCharacters = new List<Character>();
    public List<Character> PartyCharacters = new List<Character>();
    public List<Character> EnemyCharacters = new List<Character>();
    public Dictionary<int, Character> ActorToCharacterMap = new Dictionary<int, Character>();
    public List<object> Effects = new List<object>();
    public List<Item> Loot = new List<Item>();

    /*
         mDef = def,
        mBackground = Sprite.Create(),
        mPanels = {},
        mTipPanel = nil,
        mNoticePanel = nil,
        mPanelTitles = {},
        mTitleColor = Vector.Create(220/255, 220/255, 220/255, 1),
        mPartyList = nil,
        mStatsYCol = 208,
        mBars = {},
        mStatList = nil,
        OnDieCallback = def.OnDie, -- can be nil
        OnWinCallback = def.OnWin, -- can be nil


     */

    public CombatState(Character character)
    {
        this.character = character;
        EventQueue = new EventQueue();
        Stack = new StateStack();
        //mCombatScene = context;
    }

    public virtual bool Execute(float deltaTime) { return true; }
    public virtual void Enter(object stateParams) { }
    public virtual void Exit() { }
    public bool IsPartyMember(Actor actor) { return false; }
    public List<Actor> GetPartyActors() { return PartyActors; }
    public List<Actor> GetEnemiesActors() { return EnemyActors; }
    public virtual bool IsFinished() { return true; }
    public abstract string GetName(); // TODO change to int

    public List<Actor> GetAllActors()
    {
        var actors = new List<Actor>(PartyActors);
        actors.AddRange(EnemyActors);
        return actors;
    }

    public void ApplyCounter(Actor target, Actor attacker)
    {
        var targetAlive = target.Stats.Get(Stat.HP) > 0;
        if (!targetAlive)
            return;
        var isPlayer = IsPartyMember(target);
        var config = new CEAttack.Config
        {
            IsCounter = true,
            IsPlayer = isPlayer,
            CombatState = this,
            Targets = new List<Actor>() { attacker },
            Actor = target
        };
        var attack = new CEAttack(config);
        int speed = -1;
        EventQueue.Add(attack, speed);

    //    // TODO add effect

    //self:AddSpriteEffect(target,
    //    gGame.Font.damageSprite['counter'])
    }

    public void ApplyMiss(Actor target)
    {
        // TODO
        //AddSpriteEffect(target,
        //   gGame.Font.damageSprite['miss'])
    }

    public void ApplyDodge(Actor target)
    {
        if (target == null)
        {
            LogManager.LogError("Null target passed to Applydodge.");
            return;
        }
        var character = ActorToCharacterMap[target.Id];
        if (character == null)
        {
            LogManager.LogError($"character is null for Actor[{target.name}]");
            return;
        }
        // TODO pass current state
        character.Controller.Change(Constants.HURT_STATE, null);
        //controller:Change("cs_hurt", state)
        //AddSpriteEffect(target,
        //gGame.Font.damageSprite['dodge'])
    }

    public void ApplyDamage(Actor target, int damage, bool isCrit)
    {
        var stats = target.Stats;
        var hp = stats.Get(Stat.HP) - damage;
        stats.SetStat(Stat.HP, Mathf.Max(0, hp));
        LogManager.LogDebug($"Actor [{target.name}] took {damage} damage.");
        var character = ActorToCharacterMap[target.Id];
        if (damage > 0)
            // TODO pass state
            character.Controller.Change(Constants.HURT_STATE, null);
        var position = character.transform.position;
        var damageColor = isCrit ? Color.red : Color.white;

        //local dmgEffect = JumpingNumbers:Create(x, y, damage, dmgColor)
        //self: AddEffect(dmgEffect)
        HandleDeath();
    }

    public void HandleDeath()
    {
        HandlePartyDeath();
        HandleEnemyDeath();
    }

    private void HandlePartyDeath()
    {
        foreach(var actor in PartyActors)
        {
            var character = ActorToCharacterMap[actor.Id];
            var stats = actor.Stats;
            if (!character.IsAnimationPlaying(Constants.DEATH_ANIMATION))
            {
                var hp = stats.Get(Stat.HP);
                if (hp <= 0)
                {
                    // TODO params { 'death', false}
                    character.Controller.Change(Constants.DIE_STATE, null);
                    EventQueue.RemoveEventsForActor(actor.Id);
                }
            }
        }
    }

    private void HandleEnemyDeath()
    {
        for(int i = EnemyActors.Count - 1; i > -1; i++)
        {
            var actor = EnemyActors[i];
            var character = ActorToCharacterMap[actor.Id];
            var stats = actor.Stats;
            var hp = stats.Get(Stat.HP);
            if (hp <= 0)
            {
                EnemyActors.RemoveAt(i);
                EnemyCharacters.RemoveAt(i);
                ActorToCharacterMap.Remove(actor.Id);
                character.Controller.Change(Constants.DIE_STATE, null);
                EventQueue.RemoveEventsForActor(actor.Id);
                Loot.AddRange(actor.Loot);
                DeadCharacters.Add(character);
            }
        }
//    local enemyList = self.mActors['enemy']
//    for i = #enemyList, 1, -1 do
//        local actor = enemyList[i]
//        local character = self.mActorCharMap[actor]
//        local controller = character.mController
//        local stats = actor.mStats

//        local hp = stats:Get("hp_now")
//        if hp <= 0 then
//            table.remove(enemyList, i)
//            table.remove(self.mCharacters['enemy'], i)
//            self.mActorCharMap[actor] = nil

//            controller: Change("cs_die")
//            self.mEventQueue:RemoveEventsOwnedBy(actor)
//            table.insert(self.mLoot, actor.mDrop)
//            table.insert(self.mDeathList, character)
//        end
//    end
//end
    }
}

public class CombatStateParams : StateParams
{
    public string State;
    public float MoveTime;
    public Vector2 MovePosition;
    public Color flashColor;
}

public class CSStandBy : CombatState
{
    public CSStandBy(Character character)
        : base(character) { }

    public override void Enter(object stateParams)
    {
        character.SetCombatStandBy();
    }

    public override string GetName()
    {
        return "CombatStandBy";
    }
}

public class CSHurt : CombatState
{
    public CSHurt(Character character)
        : base(character) { }

    public override void Enter(object stateParams)
    {
        if (stateParams is CombatStateParams cs && cs != null)
        {
            previousState = cs.State;
            character.PlayAnimation(Constants.HURT_ANIMATION);
        }
    }

    public override bool Execute(float deltaTime)
    {
        if (!character.IsAnimationFinished(Constants.HURT_ANIMATION))
            character.Controller.Change(previousState);
        return true;
    }

    public override string GetName()
    {
        return "CombatStateHurt";
    }
}

public class CSMove : CombatState
{
    private float moveTime = 0.3f;
    private float t = 0.0f;
    private Vector2 targetPosition = Vector2.zero;

    public CSMove(Character character)
        : base(character) { }

    public override void Enter(object stateParams)
    {
        if (!(stateParams is CombatStateParams combatParams))
        {
            character.Controller.Change(character.defaultState);
            return;
        }
        targetPosition = combatParams.MovePosition;
        moveTime = combatParams.MoveTime;
        character.UpdateMovement(combatParams.MovePosition);
        t = 0.0f;

    }

    public override bool Execute(float deltaTime)
    {
        t += deltaTime;
        var distance = Vector2.Distance(character.transform.position, targetPosition);
        if (distance <= 0.1f)
        {
            character.transform.position = targetPosition;
        }
        else
        {
            var characterPosition = character.transform.position;
            var position = Vector2.Lerp(characterPosition, targetPosition, t / moveTime);
            character.UpdateMovement(position);
        }
        return true;
    }

    public override bool IsFinished()
    {
        return t >= moveTime;
    }

    public override string GetName()
    {
        return "CombatStateMove";
    }
}

public class CSRunAnimation : CombatState
{
    private string animationName;

    public CSRunAnimation(Character character)
        : base(character) { }

    public override void Enter(object stateParams)
    {
        //animation name = params;
    }

    public override string GetName()
    {
        return "CombatStateRunAnimation";
    }

    public override bool IsFinished()
    {
        return character.IsAnimationFinished(animationName);
    }
}

public class CSEnemyHurt : CombatState
{
    private int knockback = 3;
    private float moveTime = 0.2f;
    private float t = 0.0f;
    private Vector2 originalPosition = Vector2.zero;
    private Color flashColor = Color.yellow;
    private SpriteRenderer spriteRenderer;

    public CSEnemyHurt(Character character)
        : base(character) { }

    public override void Enter(object stateParams)
    {
        if (!(stateParams is CombatStateParams cs) || cs == null)
        {
            return;
        }

        previousState = cs.State;
        character.PlayAnimation(Constants.HURT_ANIMATION);
        moveTime = cs.MoveTime;
        originalPosition = character.transform.position;
        character.transform.position = character.transform.position + (Vector3)cs.MovePosition;
        flashColor = cs.flashColor;
        spriteRenderer = character.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = flashColor;
    }

    public override bool Execute(float deltaTime)
    {
        t += deltaTime;
        var distance = Vector2.Distance(character.transform.position, originalPosition);
        if (distance <= 0.1f)
        {
            character.transform.position = originalPosition;
            character.Controller.SetCurrentState(previousState);
        }
        else
        {
            var characterPosition = character.transform.position;
            var percent = t / moveTime;
            var position = Vector2.Lerp(characterPosition, originalPosition, percent);
            var color = Color.Lerp(flashColor, Color.white, percent);
            character.UpdateMovement(position);
        }
        return true;
    }

    public override void Exit()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    public override string GetName()
    {
        return "CombatStateEnemyHurt";
    }
}

public class CSEnemyDie : CombatState
{
    private float enemyDeathTime = 1.0f;
    private float time = 0.0f;
    private SpriteRenderer renderer;
    public CSEnemyDie(Character character)
    : base(character) { }

    public override void Enter(object stateParams)
    {
        character.PlayAnimation(Constants.DEATH_ANIMATION);
        renderer = character.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            LogManager.LogError($"Cannot find sprite renderer on character: {character.name}. Changing to default state");
            character.Controller.Change(character.defaultState, null);
        }
    }

    public override bool Execute(float deltaTime)
    {
        time += deltaTime;
        var color = renderer.color;
        color.a = time / enemyDeathTime;
        renderer.color = color;
        return true;
    }

    public override bool IsFinished()
    {
        return time >= enemyDeathTime;
    }

    public override string GetName()
    {
        return "CombatStateEnemyDie";
    }
}