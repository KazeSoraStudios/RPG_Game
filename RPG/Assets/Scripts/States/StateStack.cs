using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG_UI;

public class StateStack
{
    private static ErrorGameState emptyState = new ErrorGameState();
    private List<IGameState> states = new List<IGameState>();

    public StateStack()
    {
        states.Clear();
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

    public void PushTextbox(string text, string portrait)
    {
        var config = new Textbox.Config
        {
            ImagePath = portrait,
            Text = ServiceManager.Get<LocalizationManager>().Localize(text)
        };
        var textbox = ServiceManager.Get<UIController>().GetTextbox();
        textbox.Init(config);
        Push(textbox);
    }

    //function StateStack: PushFix(renderer, x, y, width, height, text, params)

    //    params = params or {}
    //local avatar = params.avatar
    //local title = params.title
    //local choices = params.choices

    //local padding = 10
    //    local titlePadY = params.titlePadY or 10
    //    local textScale = params.textScale or 1.5
    //    local panelTileSize = 3

    //    local wrap = width - padding
    //    local boundsTop = padding
    //    local boundsLeft = padding
    //    local boundsBottom = padding

    //    local children = { }

    //    if avatar then
    //        boundsLeft = avatar:GetWidth() + padding* 2
    //        wrap = width - (boundsLeft) - padding
    //        local sprite = Sprite.Create()
    //        sprite:SetTexture(avatar)
    //        table.insert(children,
    //        {
    //            type = "sprite",
    //            sprite = sprite,
    //            x = avatar:GetWidth() * 0.5 + padding,
    //            y = -avatar:GetHeight() * 0.5
    //        })
    //    end

    //    local selectionMenu = nil
    //    if choices then
    //        selectionMenu = Selection:Create
    //        {
    //            data = choices.options,
    //            OnSelection = choices.OnSelection,
    //            displayRows = #choices.options,
    //            columns = 1,
    //        }
    //        boundsBottom = boundsBottom - padding*0.5
    //    end

    //    if title then
    //        local size = renderer:MeasureText(title, wrap)
    //        boundsTop = size:Y() + padding* 2 + titlePadY

    //       table.insert(children,
    //        {
    //    type = "text",
    //            text = title,
    //            x = 0,
    //            y = size:Y() + padding + titlePadY
    //        })
    //    end

    //    local faceHeight = math.ceil(renderer:MeasureText(text) :Y())
    //    local start, finish = gRenderer:NextLine(text, 1, wrap)

    //    local boundsHeight = height - (boundsTop + boundsBottom)
    //    local currentHeight = faceHeight

    //    local chunks = { { string.sub(text, start, finish) } }
    //    while finish< #text do
    //        start, finish = gRenderer:NextLine(text, finish, wrap)

    //        if (currentHeight + faceHeight) > boundsHeight then
    //            currentHeight = 0
    //            table.insert(chunks, {string.sub(text, start, finish)})
    //        else
    //            table.insert(chunks[#chunks], string.sub(text, start, finish))
    //        end
    //        currentHeight = currentHeight + faceHeight
    //    end

    //    for k, v in ipairs(chunks) do
    //        chunks[k] = table.concat(v)
    //    end

    //    local textbox = Textbox:Create
    //    {
    //        text = chunks,
    //        textScale = textScale,
    //        size =
    //        {
    //            left    = x - width* 0.5,
    //            right   = x + width* 0.5,
    //            top     = y + height* 0.5,
    //            bottom  = y - height* 0.5
    //        },
    //        textbounds =
    //        {
    //            left = boundsLeft,
    //            right = -padding,
    //            top = -boundsTop,
    //            bottom = boundsBottom
    //        },
    //        panelArgs =
    //        {
    //            texture = Texture.Find("gradient_panel.png"),
    //            size = panelTileSize,
    //        },
    //        children = children,
    //        wrap = wrap,
    //        selectionMenu = selectionMenu,
    //        OnFinish = params.OnFinish,
    //        stack = self,
    //    }
    //    table.insert(self.mStates, textbox)
    //end

    //function StateStack:PushFit(renderer, x, y, text, wrap, params)

    //    local params = params or {}
    //    local choices = params.choices
    //    local title = params.title
    //    local avatar = params.avatar

    //    local padding = 10
    //    local titlePadY = params.titlePadY or 10
    //    local panelTileSize = 3
    //    local textScale = params.textScale or 1.5

    //    local size = renderer:MeasureText(text, wrap)
    //    local width = size:X() + padding* 2
    //    local height = size:Y() + padding* 2

    //    if choices then
    //        local selectionMenu = Selection:Create
    //        {
    //            data = choices.options,
    //            displayRows = #choices.options,
    //            columns = 1,
    //        }
    //        height = height + selectionMenu:GetHeight() + padding
    //        width = math.max(width, selectionMenu: GetWidth() + padding * 2)
    //    end

    //    if title then
    //        local size = renderer:MeasureText(title, wrap)
    //        height = height + size:Y() + titlePadY
    //        width = math.max(width, size: X() + padding * 2)
    //    end

    //    if avatar then
    //        local avatarWidth = avatar:GetWidth()
    //        local avatarHeight = avatar:GetHeight()
    //        width = width + avatarWidth + padding
    //        height = math.max(height, avatarHeight + padding)
    //    end

    //    return self:PushFix(renderer, x, y, width, height, text, params)
    //end
}
