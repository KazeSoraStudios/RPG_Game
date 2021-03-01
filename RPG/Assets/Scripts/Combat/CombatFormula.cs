using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FormulaResult
{
    public int Damage;
    public CombatFormula.HitResult Result;
}

public class CombatFormula : MonoBehaviour
{
    private static FormulaResult result;

    public enum HitResult 
    {
        Miss        = 0,
        Dodge       = 1,
        Hit         = 2,
        Critical    = 3
    }

    public static FormulaResult MeleeAttack(CombatState state, Actor attacker, Actor target)
    {
        var stats = attacker.Stats;
        var enemyStats = target.Stats;
        result.Damage = 0;
        var hitResult = IsHit(state, attacker, target);
        if (hitResult == HitResult.Miss)
        {
            result.Result = HitResult.Miss;
            return result;
        }
        if (IsDodge(state, attacker, target))
        {
            result.Result = HitResult.Miss;
            return result;
        }
        result.Damage = CalculateDamage(state, attacker, target);
        if (hitResult == HitResult.Hit)
        {
            result.Result = HitResult.Hit;
            return result;
        }

        // TODO assert
        result.Damage += BaseAttack(state, attacker, target);
        result.Result = HitResult.Critical;
        return result;
    }

    public static HitResult IsHit(CombatState state, Actor attacker, Actor target)
    {
        var stats = attacker.Stats;
        var speed = stats.Get(Stat.Speed);
        var intelligence = stats.Get(Stat.Intelligence);
        // max value is 255 if we add then divide by 255 we get 0-1
        var bonus = ((speed + intelligence) * 0.5f) * 0.555f;
        var chanceToHit = Constants.CHANCE_TO_HIT + bonus * 0.5f;
        var value = Random.Range(0, 1);
        var isHit = value <= chanceToHit;
        var isCrit = value <= Constants.CHANCE_TO_CRIT;
        return isCrit ? HitResult.Critical : isHit ?
            HitResult.Hit : HitResult.Miss;
    }

    public static bool IsDodge(CombatState state, Actor attacker, Actor target)
    {
        var stats = attacker.Stats;
        var enemyStats = target.Stats;
        var speed = stats.Get(Stat.Speed);
        var enemySpeed = enemyStats.Get(Stat.Speed);
        float speedDifference = speed - enemySpeed;
        // Clamp to 0-1
        speedDifference = Mathf.Clamp(speedDifference, -10, 10) * 0.01f;
        var chanceToDodge = Mathf.Max(0, Constants.CHANCE_TO_DODGE + speedDifference);
        return Random.Range(0, 1) <= chanceToDodge;
    }

    public static bool IsCounter(CombatState state, Actor attacker, Actor target)
    {
        var counter = target.Stats.Get(Stat.Counter);
        return Random.Range(0, 1) * Constants.COUNTER_MULTIPLIER < counter;
    }

    public static int BaseAttack(CombatState state, Actor attacker, Actor target)
    {
        var stats = attacker.Stats;
        var strength = stats.Get(Stat.Strength);
        var attack = stats.Get(Stat.Attack);
        var attackStrength = (strength * 0.5f) + attack;
        return (int)Random.Range(attackStrength, attackStrength * 2);

    }

    public static int CalculateDamage(CombatState state, Actor attacker, Actor target)
    {
        var enemyStats = target.Stats;
        var defense = enemyStats.Get(Stat.Defense);
        var attack = BaseAttack(state, attacker, target);
        return (int)Mathf.Floor(Mathf.Max(0, attack - defense));
    }

    public static bool CanEscape(CombatState state, Actor actor)
    {
        var stats = actor.Stats;
        var speed = stats.Get(Stat.Speed);
        var enemies = state.GetEnemiesActors();
        int enemyCount = enemies.Count;
        int totalSpeed = 0;
        foreach(var a in enemies)
        {
            var enemySpeed = a.Stats.Get(Stat.Speed);
            totalSpeed += enemySpeed;
        }
        var averageSpeed = totalSpeed / enemyCount;
        var escapeChance = Constants.ESCAPE_CHANCE;
        escapeChance += speed > averageSpeed ? Constants.ESCAPE_BONUS : -Constants.ESCAPE_BONUS;
        return Random.Range(0, 1) <= escapeChance;
    }

    public static bool CanSteal(CombatState state, Actor attacker, Actor target)
    {
        var chanceToSteal = Constants.CHANCE_TO_STEAL;
        if (attacker.Level > target.Level)
        {
            chanceToSteal = (Constants.STEAL_BONUS + attacker.Level - target.Level) / 128;
            chanceToSteal = Mathf.Clamp(chanceToSteal, Constants.STEAL_MIN_CHANCE, Constants.STEAL_MAX_CHANCE);
        }
        return Random.Range(0, 1) <= chanceToSteal;
    }


        //function Formula.IsHitMagic(state, attacker, target, spell)
        //    local hitchance = spell.base_hit_chance
        //    if math.random() <= hitchance then
        //        return HitResult.Hit
        //    else
        //        return HitResult.Miss
        //    end
        //end

        //function Formula.CalcSpellDamage(state, attacker, target, spell)

        //    local base = spell.base_damage
        //    if type(base) == 'table' then
        //        base = math.random(base[1], base[2])
        //    end

        //    local damage = base * 4

        //    local level = attacker.mLevel
        //    local stats = attacker.mStats

        //    local bonus = level * stats:Get("intelligence") * (base / 32)

        //    damage = damage + bonus

        //    if spell.element then
        //        local modifier = target.mStats:Get(spell.element)
        //        damage = damage + (damage* modifier)
        //    end

        //    local resist = math.min(255, target.mStats:Get("resist"))
        //    local resist01 = 1 - (resist * 0.555)
        //    damage = damage* resist01

        //    return math.floor(damage)
        //end

        //function Formula.MagicAttack(state, attacker, target, spell)

        //    local damage = 0
        //    local hitResult = Formula.IsHitMagic(state, attacker, target, spell)

        //    if hitResult == HitResult.Miss then
        //        return math.floor(damage), HitResult.Miss
        //    end

        //    local damage = Formula.CalcSpellDamage(state, attacker, target, spell)

        //    return math.floor(damage), HitResult.Hit
        //end
    }
