using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestCondition 
{
    private bool isComplete;
    private List<QuestRequirement> requirements = new List<QuestRequirement>();

    public QuestCondition(List<QuestRequirement> requirements)
    {
        this.requirements = requirements;
    }

    public bool IsComplete()
    {
        return isComplete;
    }

    public List<QuestRequirement> GetRequirements()
    {
        return requirements;
    }

    public void TryToComplete()
    {
        foreach (var requirement in requirements)
            requirement.TryToComplete();
        CheckIfComplete();
    }

    public void CompleteRequirement(string id)
    {
        foreach (var item in requirements)
            if (item.GameDataId.Equals(id))
                item.Complete();
        CheckIfComplete();
    }

    private void CheckIfComplete()
    {
        bool complete = true;
        foreach (var item in requirements)
        {
            if (!item.IsComplete)
            {
                complete = false;
                break;
            }
        }
        isComplete = complete;
    }
}

public abstract class QuestRequirement
{
    public bool IsSpecial { get; protected set; }
    public int Count { get; protected set; }
    public string GameDataId { get; protected set; }

    public bool IsComplete { get; protected set; }

    public void Complete()
    {
        IsComplete = true;
    }

    public abstract bool CanComplete();

    public bool TryToComplete()
    {
        var canComplete = CanComplete();
        IsComplete = canComplete;
        return canComplete;
    }

    protected void SetData(string id, int count = 1, bool special = false)
    {
        GameDataId = id;
        Count = count;
        IsSpecial = special;
    }
}

public class ItemQuestRequirement : QuestRequirement
{
    public ItemQuestRequirement(string id, int count = 1)
    {
        SetData(id, count, false);
    }

    public override bool CanComplete()
    {
        if (IsComplete)
            return false;
        var world = ServiceManager.Get<World>();
        return world.HasItemAmount(GameDataId, Count);
    }
}

public class SpecialItemQuestRequirement : QuestRequirement
{
    public SpecialItemQuestRequirement(string id, int count = 1)
    {
        SetData(id, count, true);
    }

    public override bool CanComplete()
    {
        if (IsComplete)
            return false;
        var world = ServiceManager.Get<World>();
        return world.HasKeyItem(GameDataId);
    }
}

public class EnemyItemRequirement : QuestRequirement
{
    public EnemyItemRequirement(string id, int count = 1, bool special = false)
    {
        SetData(id, count, special);
    }

    public override bool CanComplete()
    {
        if (IsComplete)
            return false;
        // TODO check game state
        return true;
    }
}
