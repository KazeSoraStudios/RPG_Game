using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;

public interface Trigger
{
    void OnEnter(TriggerParams triggerParams);
    void OnUse(TriggerParams triggerParams);
    void OnStay(TriggerParams triggerParams);
    void OnExit(TriggerParams triggerParams);
}

public class TriggerParams
{
    public int X;
    public int Y;
    public Character character;

    public TriggerParams(int x, int y, Character character)
    {
        X = x;
        Y = y;
        this.character = character;
    }
}

public sealed class EmptyTrigger : Trigger
{
    void Trigger.OnEnter(TriggerParams triggerParams) { }
    void Trigger.OnUse(TriggerParams triggerParams) { }
    void Trigger.OnStay(TriggerParams triggerParams) { }
    void Trigger.OnExit(TriggerParams triggerParams) { }
}