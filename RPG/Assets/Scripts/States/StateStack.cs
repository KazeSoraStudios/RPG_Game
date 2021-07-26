using System;
using System.Collections.Generic;
using RPG_UI;

public class StateStack
{
    public static ErrorGameState emptyState = new ErrorGameState();
    private List<IGameState> states = new List<IGameState>();

    public StateStack()
    {
        states.Clear();
    }

    public bool IsEmpty()
    {
        return states.Count < 1;
    }

    public void Push(IGameState state)
    {
        states.Add(state);
        state.Enter();
    }
    
    public IGameState Pop()
    {
        if (states.Count < 1)
            return emptyState;
        var state = states[states.Count - 1];
        states.RemoveAt(states.Count - 1);
        state.Exit();
        return state;
    }

    public IGameState Top()
    {
        if (states.Count < 1)
            return emptyState;
        return states[states.Count - 1];
    }
    
    public void Update(float deltaTime)
    {
        for (int i = states.Count - 1; i > -1; i--)
            if (!states[i].Execute(deltaTime))
                break;
        if (states.Count < 1)
            return;
        states[states.Count - 1].HandleInput();
    }

    public void Clear()
    {
        if (states.Count > 0)
            states[states.Count - 1].Exit();
        states.Clear();
    }

    public List<IGameState> GetStates()
    {
        return states;
    }

    public void RemoveState(IGameState state)
    {
        LogManager.LogDebug($"Trying to remove {state}.");
        for (int i = states.Count - 1; i > -1; i--)
        {
            var current = states[i];
            if(current.GetHashCode() == state.GetHashCode())
            {
                current.Exit();
                states.RemoveAt(i);
                LogManager.LogDebug($"Successfully removed {state}.");
                return;
            }
        }
        LogManager.LogDebug($"Failed to remove {state}");
    }

    public void PushTextbox(string text, bool showImage = false, string portrait = "", float advanceTime = 99999, TextBoxAnchor anchor = TextBoxAnchor.Bottom)
    {
        var config = new Textbox.Config
        {
            ImagePath = portrait,
            Text = ServiceManager.Get<LocalizationManager>().Localize(text),
            AdvanceTime = advanceTime,
            ShowImage = showImage
        };
        var textbox = ServiceManager.Get<UIController>().GetTextbox(anchor);
        textbox.Init(config);
        Push(textbox);
    }

    public void PushTextbox(Textbox.Config config, TextBoxAnchor anchor = TextBoxAnchor.Bottom)
    {
        var textbox = ServiceManager.Get<UIController>().GetTextbox(anchor);
        textbox.Init(config);
        Push(textbox);
    }
}
