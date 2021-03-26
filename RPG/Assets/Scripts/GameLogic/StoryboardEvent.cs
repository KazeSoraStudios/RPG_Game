using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RPG_Character;

public enum SBEvent { None, HandIn }

public interface IStoryboardEvent
{
    void Execute(float deltaTime);
    bool IsBlocking();
    bool IsFinished();
    bool HasEventFunction();
    IStoryboardEvent Transform(Storyboard storyboard);
}

public struct WaitEvent : IStoryboardEvent
{
    public float Seconds;

    public WaitEvent(float seconds)
    {
        Seconds = seconds;
    }

    public void Execute(float deltaTime)
    {
        Seconds -= deltaTime;
    }

    public bool IsBlocking()
    {
        return true;
    }

    public bool IsFinished()
    {
        return Seconds <= 0;
    }

    public IStoryboardEvent Transform(Storyboard storyboard) {  return this; }
    public bool HasEventFunction() { return false; }
}


public struct TweenEvent<T> : IStoryboardEvent
{
    public float Start;
    public float Distance;
    public float Duration;
    public T Target;
    public Action<T, float> Function;

    private bool finished;
    private float TimePassed;

    public void Init(T target, Action<T, float> function, float start = 0, float end = 1, float duration = 1)
    {
        Target = target;
        Function = function;
        Start = start;
        Distance = end - start;
        Duration = duration;
        TimePassed = 0.0f;
        finished = false;
    }

    public void Execute(float deltaTime)
    {
        TimePassed += deltaTime;
        var value = TweenLinear();
        Function?.Invoke(Target, value);
        if (TimePassed < Duration)
            return;
        Start += Distance;
        finished = true;
    }


    public bool IsFinished()
    {
        return finished;
    }

    public bool IsBlocking()
    {
        return true;
    }

    public IStoryboardEvent Transform(Storyboard storyboard) {  return this; }
    public bool HasEventFunction() { return false; }

    private float TweenLinear()
    {
        return Distance * TimePassed / Duration + Start;

    }
}

public struct BlockUntilEvent : IStoryboardEvent
{
    public Func<bool> UntilFunction;

    public void Init(Func<bool> untilFunction)
    {
        UntilFunction = untilFunction;
    }

    public void Execute(float deltaTime) { }

    public bool IsBlocking()
    {
        return !UntilFunction();
    }

    public bool IsFinished()
    {
        return !IsBlocking();
    }

    public bool HasEventFunction() { return false; }
    public IStoryboardEvent Transform(Storyboard storyboard) {  return this; }
}

public struct AnimateEvent : IStoryboardEvent
{
    public Character Character;
    public string Animation;

    public AnimateEvent(Character character, string animation)
    {
        Character = character;
        Animation = animation;
    }

    public void Execute(float deltaTime)
    {
        /*self.mAnimation:Update(dt)
        local frame = self.mAnimation:Frame()
        self.mEntity:SetFrame(frame)*/
    }

    public bool IsBlocking()
    {
        return true;
    }

    public bool IsFinished()
    {
        return Character.IsAnimationFinished(Animation);
    }

    public bool HasEventFunction() { return false; }
    public IStoryboardEvent Transform(Storyboard storyboard) {  return this; }
}

public struct TimedTextBoxEvent : IStoryboardEvent
{
    public float Time;
    // TODO Textbox

    public void Init(/*Textbox,*/float time)
    {
        //mTextbox = box,
        Time = time;
    }

    public void Execute(float deltaTime)
    {
        Time -= deltaTime;
        if (Time <= 0)
        { }// TODO click self.mTextbox:OnClick()
    }

    public bool IsBlocking()
    {
        return false; // TODO not self.mTextbox:IsDead()
    }

    public bool IsFinished()
    {
        return true;// TODO self.mTextbox:IsDead()
    }

    public bool HasEventFunction() { return false; }
    public IStoryboardEvent Transform(Storyboard storyboard) {  return this; }
}

public struct EmptyStoryboardEvent : IStoryboardEvent
{
    public void Execute(float deltaTime) { }

    public bool HasEventFunction() { return false; }

    public bool IsBlocking() { return false; }

    public bool IsFinished() { return true; }

    public IStoryboardEvent Transform(Storyboard storyboard) {  return this; }
}

public struct StoryboardFunctionEvent : IStoryboardEvent
{
    public Storyboard storyboard;
    public Func<Storyboard, IStoryboardEvent> Function;

    public void Execute(float deltaTime) { }

    public bool HasEventFunction() { return true; }

    public bool IsBlocking() { return true; }

    public bool IsFinished() { return IsBlocking(); }

    public IStoryboardEvent Transform(Storyboard storyboard)
    {
        return Function?.Invoke(storyboard);
    }
}

public struct NonBlockingEvent : IStoryboardEvent
{
    public IStoryboardEvent Event;

    public void Execute(float deltaTime)
    {
        Event.Execute(deltaTime);
    }

    public bool HasEventFunction()
    {
        return Event.HasEventFunction();
    }

    public bool IsBlocking()
    {
        return false;
    }

    public bool IsFinished()
    {
        return Event.IsFinished();
    }

    public IStoryboardEvent Transform(Storyboard storyboard)
    {
        return Event.Transform(storyboard);
    }
}

public class StoryboardEventFunctions
{
    public static IStoryboardEvent EmptyEvent = new EmptyStoryboardEvent();

    public static IStoryboardEvent Wait(float seconds)
    {
        return new WaitEvent(seconds);
    }


    public static IStoryboardEvent BlackScreen(string id = "blackscreen", float alpha = 1)
    {
        var color = Color.black;
        color.a = alpha;

        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                var renderer = ServiceManager.Get<GameLogic>().ScreenImage;
                var screenState = new ScreenState(renderer, Color.black);
                storyboard.PushState(id, screenState);
                return EmptyEvent;
            }
        };
    }

    public static IStoryboardEvent PlaySound(string soundName, float volume, string identifer = "")
    {
        if (identifer.Equals(string.Empty))
            identifer = soundName;
        if (volume < 0 || volume > 1)
            volume = 1;
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                // TODO sounds manager
                //var id = Sound.Play(soundName)
                //Sound.SetVolume(id, volume)
                //storyboard:Sound(name, id)
                return EmptyEvent;
            }
        };
    }

    public static IStoryboardEvent StopSound(string name)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                // TODO sounds
                storyboard.StopSound(name);
                return EmptyEvent;
            }
        };
    }


    public static IStoryboardEvent FadeOutChararcter(int mapId, string npcId, float duration = 1)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                var map = GetMapReference(storyboard, mapId);
                var npcs = ServiceManager.Get<NPCManager>();
                if (!npcs.HasNPC(npcId))
                {
                    LogManager.LogError($"Map [{map.name}] does not contain npc [{npcId}]. Returning from FadeOutCharacterEvent.");
                    return EmptyEvent;
                }
                var npc = npcs.GetNPC(npcId);
                if (npc.IsHero())
                    npc = ((ExploreState)storyboard.States[Constants.EXPLORE_STATE]).Hero;
                return new TweenEvent<Character>
                {
                    Function = (target, value) =>
                    {
                        if (target.GetComponent<SpriteRenderer>() is var renderer && renderer != null)
                        {
                            var color = renderer.color;
                            color.a = value;
                            renderer.color = color;
                        }
                    },
                    Target = npc,
                    Start = 1,
                    Distance = 1,
                    Duration = 1,
                };
            }
        };
    }

    public static IStoryboardEvent WriteTile(int mapId, Map.ChangeTile changeTile)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                var map = GetMapReference(storyboard, mapId);
                map.WriteTile(changeTile);
                return EmptyEvent;
            }
        };
    }

    public static IStoryboardEvent MoveCameraToTile(string state, int x, int y, float duration = 1.0f)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                var exploreState = (ExploreState)storyboard.States[state];
                var startX = exploreState.manualCameraX;
                var startY = exploreState.manualCameraY;
                var topLeft = exploreState.Map.GetTileFoot(x, y);
                var endX = topLeft.Item1;
                var endY = topLeft.Item2;
                var xDistance = endX - startX;
                var yDistance = endY - startY;

                return new TweenEvent<ExploreState>
                {
                    Target = exploreState,
                    Function = (target, value) =>
                    {
                        var dX = startX + xDistance * value;
                        var dY = startY + xDistance * value;
                        target.manualCameraX = (int)dX;
                        target.manualCameraY = (int)dY;
                    },
                    Start = 0,
                    Duration = duration
                };
            }
        };
    }

    public static IStoryboardEvent FadeSound(string name, float start, float end, float duration)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                var id = storyboard.PlayingSounds[name];
                // TODO assert sound id
                return new TweenEvent<float>
                {
                    Target = 0.0f,
                    Start = start,
                    Distance = end - start,
                    Duration = duration,
                    Function = (target, value) =>
                    {
                        // TODO sound manager 
                        // Sound.SetVolume(target, value);
                    }
                };
            }
        };
    }

    public static IStoryboardEvent FadeScreen(float start, float finish, float duration = 3.0f, string state = "")
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                var gameState = storyboard.SubStack.Top();
                if (state != string.Empty)
                    gameState = storyboard.States[state];
                DebugAssert.Assert(gameState.GetType() == typeof(ScreenState), $"gameState {gameState} from story is not type ScreenState. Cannot fade screen,");

                return new TweenEvent<ScreenState>
                {
                    Start = start,
                    Distance = finish - start,
                    Duration = duration,
                    Function = (target, value) =>
                    {
                        var image = target.Image;
                        var color = image.color;
                        color.a = value;
                        image.color = color;
                    },
                    Target = (ScreenState)gameState
                };
            }
        };
    }

    public static IStoryboardEvent FadeScreenIn(string state = "", float duration = 3.0f)
    {
        return FadeScreen(0, 1, duration, state);
    }

    public static IStoryboardEvent FadeScreenOut(string state = "", float duration = 3.0f)
    {
        return FadeScreen(1, 0, duration, state);
    }

    //function SOP.Caption(id, style, text)

    //    return function(storyboard)
    //        local style = ShallowClone(CaptionStyles [style])
    //        local caption = CaptionState:Create(style, text)
    //        storyboard:PushState(id, caption)

    //        return TweenEvent:Create(
    //                Tween:Create(0, 1, style.duration),
    //                style,
    //                style.ApplyFunc)

    //    end

    //end

    //function SOP.FadeOutCaption(id, duration)
    //    return function(storyboard)
    //        local target = storyboard.mSubStack:Top()
    //        if id then
    //            target = storyboard.mStates [id]
    //end
    //        print(id, target)
    //        local style = target.mStyle
    //        duration = duration or style.duration

    //        return TweenEvent:Create(
    //            Tween:Create(1, 0, duration),
    //            style,
    //            style.ApplyFunc
    //        )
    //    end
    //end

    public static IStoryboardEvent NoBlock(IStoryboardEvent storyboardEvent)
    {
        return new NonBlockingEvent
        {
            Event = storyboardEvent
        };
    }

    public static IStoryboardEvent KillState(string state)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                storyboard.RemoveState(state);
                return EmptyEvent;
            }
        };
    }

    public static IStoryboardEvent Scene(string scene, int startingX = 1, int startingY = 1, bool hideHero = false)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                //var map = MapDB[map];
                SceneManager.LoadScene(scene, LoadSceneMode.Additive);
                var startingPosition = new Vector2(startingX, startingY);
               // TODO set up mangement ExploreState
                //storyboard.PushState(GameStates.Explore, state);
                return NoBlock(Wait(0.1f));
            }
        };
    }

    //function SOP.Scene(params)
    //    return function(storyboard)
    //        local id = params.name or params.map
    //        local map = MapDB [params.map]()
    //        local focus = Vector.Create(params.focusX or 1,
    //                                    params.focusY or 1,
    //                                    params.focusZ or 1)
    //        local state = ExploreState:Create(nil, map, focus)
    //        if params.hideHero then
    //            state:HideHero()
    //        end
    //        storyboard:PushState(id, state)

    //        -- Allows the following instruction to be carried out
    //        -- on the same frame.
    //        return SOP.NoBlock(SOP.Wait(0.1))()
    //    end
    //end


    public static Map GetMapReference(Storyboard storyboard, int stateId)
    {
        var exploreState = (ExploreState)storyboard.States[Constants.EXPLORE_STATE];
        DebugAssert.Assert(exploreState != null && exploreState.Map != null, "ExploreState is null or has null map.");
        return exploreState.Map;
    }

    //function SOP.ReplaceScene(name, params)
    //    return function(storyboard)
    //        gGame.World.mParty:DebugPrintParty()
    //        local state = storyboard.mStates [name]
    //print("Replace scene tried to get", name, "got", state.mId)

    //        -- Give the state an updated name
    //        local id = params.name or params.map
    //        storyboard.mStates [name] = nil
    //        storyboard.mStates [id] = state

    //        local mapDef = MapDB [params.map](gGame.World.mGameState)
    //        state.mMap =  Map:Create(mapDef)
    //        state.mMapDef = mapDef

    //        print("after map created")
    //        gGame.World.mParty:DebugPrintParty()

    //        state.mMap:GotoTile(params.focusX, params.focusY)
    //        state.mHero = Character:Create(gCharacters.hero, state.mMap)
    //        state.mHero.mEntity:SetTilePos(
    //            params.focusX,
    //            params.focusY,
    //            params.focusZ or 1,
    //            state.mMap)
    //        state:SetFollowCam(true, state.mHero)

    //        if params.hideHero then
    //            state:HideHero()
    //        else
    //            state:ShowHero()
    //        end

    //        gGame.World.mParty:DebugPrintParty()
    //        return SOP.NoBlock(SOP.Wait(0.1))()
    //    end
    //end

    public static IStoryboardEvent ReplaceState(IGameState current, IGameState newState)
    {
        return new StoryboardFunctionEvent
         {
           Function = (storyboard) =>
           {
               var stack = storyboard.Stack;
               var states = stack.GetStates();
               for (int i = 0; i < states.Count; i++)
               {
                   if (states[i].GetType() == current.GetType())
                   {
                       states[i].Exit();
                       states[i] = newState;
                       states[i].Enter();
                       return EmptyEvent;
                   }
               }
               LogManager.LogError($"Failed to replace {current} with {newState}");
               return EmptyEvent;
           }
         };
    }

    public static IStoryboardEvent RemoveState(IGameState state)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                storyboard.Stack.RemoveState(state);
                return EmptyEvent;
            }
        };
    }

    //function SOP.Say(mapId, npcId, text, time, params)

    //    time = time or 1
    //    params = params or {textScale = 0.8}

    //    return function(storyboard)
    //        local map = GetMapRef(storyboard, mapId)
    //        local npc = map.mNPCbyId[npcId]
    //        if npcId == "hero" then
    //           npc = storyboard.mStates[mapId].mHero
    //        end
    //        local pos = npc.mEntity.mSprite:GetPosition()
    //        storyboard.mStack:PushFit(
    //            gRenderer,
    //            -map.mCamX + pos:X(), -map.mCamY + pos:Y() + 32,
    //            text, -1, params)
    //        local box = storyboard.mStack:Top()
    //        return TimedTextboxEvent:Create(box, time)
    //    end
    //end

    
    public static IStoryboardEvent RunAction(RunAction action, List<object> def)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                def.Insert(0, storyboard);
                action?.Invoke(def);
                return EmptyEvent;
            }
        };
    }

    public static IStoryboardEvent MoveNPC(string npcId, int mapId, List<Character.Direction> path)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                var map = GetMapReference(storyboard, mapId);
                var npc = ServiceManager.Get<NPCManager>().GetNPC(npcId);
                npc.FollowPath(path);
                return new BlockUntilEvent
                {
                    UntilFunction = () =>
                    {
                        return npc.PathIndex > npc.PathToMove.Count;
                    }
                };
            }
        };
    }

    public static IStoryboardEvent HandOffToExploreState()
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                LogManager.LogDebug($"Handing off to explore state.");
                var exploreState = (ExploreState)storyboard.States[Constants.HANDIN_STATE];
                storyboard.Stack.Pop();
                storyboard.Stack.Push(exploreState);
                exploreState.stack = ServiceManager.Get<GameLogic>().Stack;
                return EmptyEvent;
            }
        };
    }

    public static IStoryboardEvent Function(Action action)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                action?.Invoke();
                return EmptyEvent;
            }
        };
    }

    public static IStoryboardEvent RunCombatState(StateMachine stateMachine, string state, StateParams stateParams)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                stateMachine.Change(state, stateParams);
                return new BlockUntilEvent
                {
                    UntilFunction = () =>
                    {
                        return ((CombatState)stateMachine.CurrentState).IsFinished();
                    }
                };
            }
        };
    }
    
    public static IStoryboardEvent UpdateState(IState state, float duration)
    {
        return new TweenEvent<IState>
        {
            Start = 0,
            Distance = 1,
            Target = state,
            Duration = duration,
            Function = (target, value) => target.Execute(Time.deltaTime)
        };
    }

    public static IStoryboardEvent Animate(Character character, string animation)
    {
        return new AnimateEvent(character, animation);
    }
}