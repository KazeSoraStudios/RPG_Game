using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerManager
{

    [SerializeField] Dictionary<Vector2Int, Trigger> Triggers = new Dictionary<Vector2Int, Trigger>();

    private EmptyTrigger emptyTrigger = new EmptyTrigger();

    public TriggerManager()
    {
        ServiceManager.Register(this);
    }

    ~TriggerManager()
    {
        ServiceManager.Unregister(this);
    }

    public Trigger GetTrigger(int x, int y)
    {
        var position = new Vector2Int(x, y);
        var trigger = emptyTrigger;
        if (!Triggers.ContainsKey(position))
            return trigger;
        return Triggers[position];
    }

    public void AddTrigger(int x, int y, Trigger trigger)
    {
        var position = new Vector2Int(x, y);
        Triggers.Add(position, trigger);
    }

    public void AddTrigger(Vector2Int position, Trigger trigger)
    {
        Triggers.Add(position, trigger);
    }
}
