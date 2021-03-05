using System;
using UnityEngine;


public class GameDataHandler : MonoBehaviour
{
    protected static int GetIntFromCell(string intCell)
    {
        int price = 0;
        if (intCell.IsNotEmpty())
            price = int.Parse(intCell);
        return price;
    }

    protected static float GetFloatFromCell(string floatCell)
    {
        float price = 0.0f;
        if (floatCell.IsNotEmpty())
            price = float.Parse(floatCell);
        return price;
    }

    protected static Vector2 GetVector2FromCell(string vec2Cell)
    {
        var vars = vec2Cell.Split(':');
        if (vars.Length != 2)
            return Vector2.zero;
        return new Vector2(GetFloatFromCell(vars[0]), GetFloatFromCell(vars[1]));
    }

    protected static  T GetEnum<T>(T defaultT, string type) where T : struct, Enum
    {
        var t = defaultT;
        if (!Enum.TryParse(type, true, out t))
            LogManager.LogError($"Cannot parse {type} into {t}. Returning default {defaultT}.");
        return t;
    }
    
    protected static T[] GetEnumArray<T>(T defaultT, string data, bool returnEmpty = false) where T: struct, Enum
    {
        if (data.IsEmpty())
            return returnEmpty ?  new T[0] : new T[] { defaultT };
        data = data.ToLower();
        var enums = data.Split(':');
        int count = enums.Length;
        var ts = new T[count];
        for (int i = 0; i < count; i++)
        {
            var t = GetEnum(defaultT, data);
            ts[i] = t;
        }
        return ts;
    }

    protected static Action<CombatState, bool> GetTarget(string name, string data)
    {
        if (data.IsEmpty())
        {
            LogManager.LogError($"Empty selecotr data found for item use: {name}. Returning random selector.");
            return (state, hurt) => CombatSelector.RandomPlayer(state);
        }
        var selector = GetEnum(Selector.RandomParty, data);
        switch (selector)
        {
            case Selector.FirstDeadEnemy:
                return (state, hurt) => CombatSelector.FirstDeadEnemy(state);
            case Selector.FirstDeadParty:
                return (state, hurt) => CombatSelector.FirstDeadPartyMember(state);
            case Selector.LowestEnemyHP:
                return (state, hurt) => CombatSelector.FindWeakestHurtEnemy(state);
            case Selector.LowestHP:
                return (state, hurt) => CombatSelector.FindWeakestActor(state.GetAllActors(), true);
            case Selector.LowestMPEnemy:
                return (state, hurt) => CombatSelector.FindLowestMPEnemy(state);
            case Selector.LowestMPParty:
                return (state, hurt) => CombatSelector.FindLowestMPPartyMember(state);
            case Selector.LowestPartyHP:
                return (state, hurt) => CombatSelector.FindWeakestHurtPartyMember(state);
            case Selector.WeakestActor:
                return (state, hurt) => CombatSelector.FindWeakestActor(state.GetAllActors(), false);
            case Selector.WeakestEnemy:
                return (state, hurt) => CombatSelector.FindWeakestEnemy(state);
            case Selector.WeakestPartyMember:
                return (state, hurt) => CombatSelector.FindWeakestPartyMember(state);
            case Selector.RandomEnemy:
                return (state, hurt) => CombatSelector.RandomEnemy(state);
            case Selector.RandomParty:
            default:
                return (state, hurt) => CombatSelector.RandomPlayer(state);
        }
    }
}

