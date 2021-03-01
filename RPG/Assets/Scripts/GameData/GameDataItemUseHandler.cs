using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataItemUseHandler : GameDataHandler
{
    private static readonly ItemTarget EmptyTarget = new ItemTarget
    {
        Selector = (state, hurt) => CombatSelector.RandomPlayer(state),
        SwitchSides = true,
        Type = "one"
    };

    public static Dictionary<string, ItemUse> ProcessItems(int index, int count, int numberOfColumns, string[] data)
    {
        LogManager.LogDebug("Creating GameData ItemUses.");
        LogManager.LogDebug($"Processing ItemUse for data {data}");
        var items = new Dictionary<string, ItemUse>();
        // Account for difference in columns
        var columnDifference = numberOfColumns - 7;
        for (int i = 0; i < count; i++)
        {
            var name = data[index++];
            var itemUse = new ItemUse()
            {
                Id = name,
                Amount = GetIntFromCell(data[index++]),
                Hint = data[index++],
                UseOnMap = data[index++].Equals("1"),
                Target = new ItemTarget
                {
                    Selector = GetTarget(name, data[index++]),
                    SwitchSides = data[index++].Equals("1"),
                    Type = data[index++]
                }
            };
            index += columnDifference;
            items.Add(name, itemUse);
        }
        LogManager.LogDebug("Processing Gamedata ItemUses finished.");
        return items;
    }

    private static Action<CombatState, bool> GetTarget(string name, string data)
    {
        if (data.IsEmpty())
        {
            LogManager.LogError($"Empty selecotr data found for item use: {name}. Returning random selector.");
            return (state, hurt) => CombatSelector.RandomPlayer(state);
        }
        var selector = GetEnum(Selector.Random, data);
        switch (selector)
        {
            case Selector.FirstDeadEnemy:
                // TODO implement
                return (state, hurt) => CombatSelector.FirstDeadPartyMember(state);
            case Selector.FirstDeadParty:
                return (state, hurt) => CombatSelector.FirstDeadPartyMember(state);
            case Selector.LowestEnemyHP:
                return (state, hurt) => CombatSelector.FindWeakestHurtEnemy(state);
            case Selector.LowestHP:
                return (state, hurt) => CombatSelector.FindWeakestActor(state.GetAllActors(), true);
            case Selector.LowestMPEnemy:
                // TODO implement
                return (state, hurt) => CombatSelector.FindLowestMPPartyMember(state);
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
            case Selector.Random:
            default:
                return (state, hurt) => CombatSelector.RandomPlayer(state);
        }
}
}
