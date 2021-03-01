using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipMenuState : UIMonoBehaviour, IGameState
{
    public class Config
    {
        public InGameMenu Parent;
    }

    private InGameMenu parent;
    private StateStack stack;
    private StateMachine stateMachine;

    public void Init(Config config)
    {
        if (CheckUIConfigAndLogError(config, GetName()))
            return;
        parent = config.Parent;
        stack = parent.Stack;
        stateMachine = parent.StateMachine;
    }

    public void Enter(object o = null)
    {
        throw new System.NotImplementedException();
    }

    public bool Execute(float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public string GetName()
    {
        throw new System.NotImplementedException();
    }

    public void HandleInput()
    {
        throw new System.NotImplementedException();
    }
}
