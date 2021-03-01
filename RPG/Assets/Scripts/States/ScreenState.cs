using System.Collections.Generic;
using UnityEngine;

public class ScreenState : IGameState
{
    public SpriteRenderer GetRenderer() { return null; }

    public void Enter(object o)
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