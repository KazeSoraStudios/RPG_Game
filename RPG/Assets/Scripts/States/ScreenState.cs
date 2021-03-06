using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenState : IGameState
{
    public Image Image;

    public ScreenState(Image image, Color color)
    {
        Image = image;
        image.color = color;
    }
    
    public void Enter(object o) { }

    public bool Execute(float deltaTime) { return true; }

    public void Exit() { }

    public string GetName()
    {
        return "ScreenState";
    }

    public void HandleInput() { }
}