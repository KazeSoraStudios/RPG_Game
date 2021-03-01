using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyState : IState
{
    public bool Execute(float deltaTime) { return true; }
    public void Enter(object stateParams) { }
    public void Exit() { }
    public string GetName() { return "EmptyState"; }  // TODO change to int

    public EmptyState() { }
}