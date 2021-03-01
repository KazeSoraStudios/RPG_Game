using System.Collections.Generic;
using UnityEngine;

public class Storyboard : IGameState
{
    public StateStack Stack = new StateStack();
    public StateStack SubStack = new StateStack();
    public Dictionary<string, IGameState> States = new Dictionary<string, IGameState>();
    public Dictionary<SBEvent, IStoryboardEvent> events = new Dictionary<SBEvent, IStoryboardEvent>();
    public Dictionary<string, AudioSource> PlayingSounds = new Dictionary<string, AudioSource>();

    public void Init(StateStack stack, Dictionary<SBEvent, IStoryboardEvent> events, bool handIn)
    {
        this.Stack = stack;
        this.events = events;
        States.Clear();
        SubStack.Clear();
        PlayingSounds.Clear();

        if (!handIn)
            return;
        var state = stack.Pop();
        PushState(Constants.HANDIN_STATE, state);
    }

    public void Enter(object o) { }

    public void Exit()
    {
        foreach (var sound in PlayingSounds)
            sound.Value.Stop();
    }

    public void HandleInput() { }

    public bool Execute(float deltaTime)
    {
        SubStack.Update(deltaTime);

        if (events.Count == 0)
            Stack.Pop();

        SBEvent removeEvent = SBEvent.None;
        foreach (var _event in events)
        {
            var e = _event.Value;
            if (!e.HasRan())
                e.RunFirst();
            e.Execute(deltaTime);
            if (e.IsFinished())
            {
                removeEvent = _event.Key;
                break;
            }
            if (e.IsBlocking())
                break;
        }
        if (removeEvent != SBEvent.None)
            events.Remove(removeEvent);
        return true;
    }

    public string GetName()
    {
        return "StoryboardState";
    }

    public void AddSound(string name, AudioSource sound)
    {
        if (PlayingSounds.ContainsKey(name))
            return;
        PlayingSounds.Add(name, sound);
    }

    public void StopSound(string name)
    {
        // TODO use audio manager
        if (!PlayingSounds.ContainsKey(name))
            return;
        PlayingSounds[name].Stop();
    }

    public void PushState(string id, IGameState state)
    {
        // TODO assert
        if (States.ContainsKey(id))
            return;
        States.Add(id, state);
        SubStack.Push(state);
    }

    public void RemoveState(string id)
    {
        if (!States.ContainsKey(id))
            return;
        var state = States[id];
        var subStates = SubStack.GetStates();
        for (int i = subStates.Count - 1; i > -1; i--)
        {
            if (state == subStates[i])
                subStates.RemoveAt(i);
        }
    }
}