using System.Collections.Generic;
using UnityEngine;

public class Storyboard : IGameState
{
    public StateStack Stack = new StateStack();
    public StateStack SubStack = new StateStack();
    public Dictionary<string, IGameState> States = new Dictionary<string, IGameState>();
    public List<IStoryboardEvent> events = new List<IStoryboardEvent>();
    public Dictionary<string, AudioSource> PlayingSounds = new Dictionary<string, AudioSource>();

    public Storyboard (StateStack stack, List<IStoryboardEvent> events, bool handIn = false)
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
        
        int removeIndex = -1;
        for (int i = 0; i < events.Count; i++)
        {
            // If our event is an event to a function, 
            // call the function and then keep a reference to the new event it creates
            if (events[i].HasEventFunction())
                events[i] = events[i].Transform(this);
            events[i].Execute(deltaTime);
            if (events[i].IsFinished())
            {
                removeIndex = i;
                break;
            }
            if (events[i].IsBlocking())
                break;
        }
        if (removeIndex != -1)
            events.RemoveAt(removeIndex);
        return false;
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
        if (States.ContainsKey(id))
        {
            LogManager.LogError($"Tried to push [{id}. {state}] onto storyboard but {States} does not contain it.");
            return;
        }
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