using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public static readonly IState EmptyState = new EmptyState();
    public int State = 0;
    public IState CurrentState;
    public string owner;
    public Dictionary<string, IState> States = new Dictionary<string, IState>();

    public StateMachine(Dictionary<string, IState> states, string owner, IState defaultState = null)
    {
        this.owner = owner;
        States = states;
        CurrentState = defaultState;
        if (CurrentState == null)
            CurrentState = EmptyState;
    }

    public void Stop()
    {
        CurrentState.Exit();
        CurrentState = EmptyState;
    }

    public void Change(string state, object stateParams = null)
    {
        CurrentState.Exit();
        CurrentState = States.ContainsKey(state) ? States[state] : EmptyState;
        CurrentState.Enter(stateParams);
    }

   public void Update(float deltaTime)
    {
        CurrentState.Execute(deltaTime);
    }

    public IState GetCurrentState()
    {
        return CurrentState;
    }

    public void SetCurrentState(string state)
    {
        if (!States.ContainsKey(state))
        {
            LogManager.LogError($"Tried to set {state}, but not found in owner[{owner}] states.");
        }
        CurrentState = States[state];        
    }
}

public interface IState
{
    void Enter(object o = null);
    bool Execute(float deltaTime);
    void Exit();
    string GetName();  // TODO change to int
}

public class StateParams
{
}

public class MoveParams : StateParams
{
    public Vector2 MovePosition;

    public MoveParams(Vector2 move)
    {
        MovePosition = move;
    }
}

public interface IGameState : IState
{
    void HandleInput();
}