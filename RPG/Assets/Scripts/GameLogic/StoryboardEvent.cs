using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RPG_Audio;
using RPG_Character;
using RPG_Combat;
using RPG_UI;

public enum SBEvent { None, HandIn }

public interface IStoryboardEvent
{
    void Execute(float deltaTime);
    bool IsBlocking();
    bool IsFinished();
    bool HasEventFunction();
    IStoryboardEvent Transform(Storyboard storyboard);
}

public class WaitEvent : IStoryboardEvent
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


public class TweenEvent<T> : IStoryboardEvent
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

public class BlockUntilEvent : IStoryboardEvent
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

public class AnimateEvent : IStoryboardEvent
{
    private bool startedAnimating;
    private string animation;
    private Character character;

    public AnimateEvent(Character character, string animation)
    {
        this.character = character;
        this.animation = animation;
    }

    public void Execute(float deltaTime)
    {
        if (startedAnimating)
            return;
        character.PlayAnimation(animation);
        startedAnimating = true;
    }

    public bool IsBlocking()
    {
        return true;
    }

    public bool IsFinished()
    {
        return character.IsAnimationFinished(animation);
    }

    public bool HasEventFunction() { return false; }
    public IStoryboardEvent Transform(Storyboard storyboard) {  return this; }
}

public class TimedTextBoxEvent : IStoryboardEvent
{
    private float time;
    private float countDown;
    private Textbox textbox;

    public TimedTextBoxEvent(Textbox.Config config, float time, TextBoxAnchor anchor)
    {
        textbox = ServiceManager.Get<UIController>().GetTextbox(anchor);
        textbox.Init(config);
        textbox.Enter();
        this.time = time;
        countDown = time;
    }

    public void Execute(float deltaTime)
    {
        textbox.Execute(deltaTime);
        textbox.HandleInput();
        countDown -= deltaTime;
        if (countDown <= 0)
        {
            textbox.OnClick();
            // If the textbox advances the text, reset our countdown
            if (!textbox.IsDead() && !textbox.IsTurningOff())
                countDown = time;
        }
    }

    public bool IsBlocking()
    {
        return !textbox.IsDead();
    }

    public bool IsFinished()
    {
        return textbox.IsDead();
    }

    public bool HasEventFunction() { return false; }
    public IStoryboardEvent Transform(Storyboard storyboard) {  return this; }
}

public class EmptyStoryboardEvent : IStoryboardEvent
{
    public void Execute(float deltaTime) { }

    public bool HasEventFunction() { return false; }

    public bool IsBlocking() { return false; }

    public bool IsFinished() { return true; }

    public IStoryboardEvent Transform(Storyboard storyboard) {  return this; }
}

public class StoryboardFunctionEvent : IStoryboardEvent
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

public class NonBlockingEvent : IStoryboardEvent
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

public class FollowPathEvent : IStoryboardEvent
{
    private Character character;

    public FollowPathEvent(Character character)
    {
        this.character = character;
    }

    public void Execute(float deltaTime)
    {
        character.Controller.Update(deltaTime);
    }

    public bool IsBlocking()
    {
        return true;
    }

    public bool IsFinished()
    {
        return character.PathIndex > character.PathToMove.Count;
    }

    public bool HasEventFunction() { return false; }
    public IStoryboardEvent Transform(Storyboard storyboard) {  return this; }
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
                var uiController = ServiceManager.Get<UIController>();
                var renderer = uiController.ScreenImage;
                var color = renderer.color;
                renderer.color = new Color(color.r, color.b, color.g, 0);
                uiController.gameObject.SafeSetActive(true);
                var screenState = new ScreenState(renderer, Color.black);
                storyboard.PushState(id, screenState);
                return EmptyEvent;
            }
        };
    }

    public static IStoryboardEvent PlaySound(string soundName, float volume)
    {
        if (soundName.IsEmptyOrWhiteSpace())
        {
            LogManager.LogWarn("Empty Sound passed to PlaySound.");
            return EmptyEvent;
        }
        if (volume < 0 || volume > 1)
            volume = 1;
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                ServiceManager.Get<AudioManager>().PlaySound(soundName, volume);
                storyboard.AddSound(soundName);
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


    //public static IStoryboardEvent FadeOutChararcter(string npcId, float duration = 1)
    //{
    //    return new StoryboardFunctionEvent
    //    {
    //        Function = (storyboard) =>
    //        {
    //            var npcs = ServiceManager.Get<NPCManager>();
    //            if (!npcs.HasNPC(npcId))
    //            {
    //                LogManager.LogError($"NPC Manager does not contain npc [{npcId}]. Returning from FadeOutCharacterEvent.");
    //                return EmptyEvent;
    //            }
    //            var npc = npcs.GetNPC(npcId);
    //            if (npc.IsHero())
    //                npc = ((ExploreState)storyboard.States[Constants.EXPLORE_STATE]).Hero;
    //            return new TweenEvent<Character>
    //            {
    //                Function = (target, value) =>
    //                {
    //                    if (target.GetComponent<SpriteRenderer>() is var renderer && renderer != null)
    //                    {
    //                        var color = renderer.color;
    //                        color.a = value;
    //                        renderer.color = color;
    //                    }
    //                },
    //                Target = npc,
    //                Start = 1,
    //                Distance = 1,
    //                Duration = 1,
    //            };
    //        }
    //    };
    //}

    public static IStoryboardEvent MoveCameraToTile(string state, int endX, int endY, float duration = 1.0f)
    {
       return new StoryboardFunctionEvent
       {
           Function = (storyboard) =>
           {
               var camera = Camera.main.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
               var startX = camera.transform.position.x;
               var startY = camera.transform.position.y;
               var xDistance = endX - startX;
               var yDistance = endY - startY;

               return new TweenEvent<Cinemachine.CinemachineVirtualCamera>
               {
                   Target = camera,
                   Function = (target, value) =>
                   {
                       var dX = startX + xDistance * value;
                       var dY = startY + xDistance * value;
                       var position = new Vector3(dX, dY, 0.0f);
                       target.transform.position = position;
                   },
                   Start = 0,
                   Duration = duration
               };
           }
       };
    }

    public static IStoryboardEvent FadeSound(string name, float duration)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                ServiceManager.Get<AudioManager>().ForceFadeOut(name, duration);
                return EmptyEvent;
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

    public static IStoryboardEvent FadeScreenIn(string state = "blackscreen", float duration = 3.0f)
    {
        return FadeScreen(0, 1, duration, state);
    }

    public static IStoryboardEvent FadeScreenOut(string state = "blackscreen", float duration = 3.0f)
    {
        return FadeScreen(1, 0, duration, state);
    }

    // public static IStoryboardEventCaption(string text)
    // {
    //    return function(storyboard)
    //        local style = ShallowClone(CaptionStyles [style])
    //        local caption = CaptionState:Create(style, text)
    //        storyboard:PushState(id, caption)

    //        return TweenEvent:Create(
    //                Tween:Create(0, 1, style.duration),
    //                style,
    //                style.ApplyFunc)

    //    end
    // }

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

    public static IStoryboardEvent LoadScene(string scene, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        return new StoryboardFunctionEvent
        {
            Function = (_) =>
            {
                var loaded = false;
                var camera = Camera.main;
                var listener = camera.GetComponent<AudioListener>();
                GameObject.Destroy(listener);
                SceneManager.sceneLoaded += (x, y) => loaded = true;
                SceneManager.LoadScene(scene, mode);
                return new BlockUntilEvent
                {
                    UntilFunction = () =>
                    {
                        return loaded;
                    }
                };
            }
        };
    }

    public static IStoryboardEvent AddExploreStateToCurrentMap(string mapName, StateStack stack)
    {
        return new StoryboardFunctionEvent
        {
            Function = (_) =>
            {
                var obj = GameObject.Find($"{mapName}Map");
                var map = obj.GetComponent<Map>();
                var exploreState = map.gameObject.AddComponent<ExploreState>();
                exploreState.Init(map, stack, Vector2.zero);
                //stack.Push(exploreState);
                return EmptyEvent;
            }
        };
    }

    public static IStoryboardEvent MoveHeroToPosition(string mapName, Vector2 positon)
    {
        return new StoryboardFunctionEvent
        {
            Function = (_) =>
            {
                 var map = GameObject.Find($"{mapName}Map");
                var exploreState = map.gameObject.GetComponent<ExploreState>();
                exploreState.SetHeroPosition(positon);
                return EmptyEvent;
            }
        };
    }

    public static IStoryboardEvent MoveCharacterToPosition(Transform character, Vector2 positon)
    {
        return new StoryboardFunctionEvent
        {
            Function = (_) =>
            {
                character.position = positon;
                return EmptyEvent;
            }
        };
    }

    public static IStoryboardEvent DeleteScene(string currentScene, string nextScene)
    {
        return new StoryboardFunctionEvent
        {
            Function = (_) =>
            {
                var scene = SceneManager.GetSceneByName(nextScene);
                SceneManager.SetActiveScene(scene);
                var unload = SceneManager.UnloadSceneAsync(currentScene);
                return new BlockUntilEvent
                {
                    UntilFunction = () =>
                    {
                        return unload.isDone;
                    }
                };
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

    //        -- Allows the following inclassion to be carried out
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

    public static IStoryboardEvent ReplaceExploreState(string explore, StateStack stack, Map map)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                var exploreState = storyboard.States[explore] as ExploreState;
                LogManager.LogDebug($"Replacing ExploreState {exploreState.name} with : {map.MapName}");
                storyboard.States.Remove(explore);
                var newExploreState = map.gameObject.AddComponent<ExploreState>();
                storyboard.States[map.MapName] = newExploreState;
                newExploreState.Init(map, stack, map.HeroStartingPosition);
                return NoBlock(Wait(0.1f));
            }
        };
    }

    public static IStoryboardEvent ReplaceExploreState(string explore, StateStack stack, Func<Map> function)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                var map = function();
                var exploreState = storyboard.States[explore] as ExploreState;
                LogManager.LogDebug($"Replacing ExploreState {exploreState.name} with : {map.MapName}");
                storyboard.States.Remove(explore);
                var exploreStateComponent = map.GetComponent<ExploreState>();
                if (exploreStateComponent == null)
                    exploreStateComponent = map.gameObject.AddComponent<ExploreState>();
                var newExploreState = exploreStateComponent;
                storyboard.States[explore] = newExploreState;
                newExploreState.Init(map, stack, map.HeroStartingPosition);
                return NoBlock(Wait(0.1f));
            }
        };
    }


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

   public static IStoryboardEvent Say(string text, float time = Constants.DEFAULT_TIMED_TEXTBOX_TIME, TextBoxAnchor anchor = TextBoxAnchor.Bottom)
   {
       return new StoryboardFunctionEvent
       {
           Function = (storyboard) =>
           {
               
               var config = new Textbox.Config 
                {
                    InitialDelay = 0.5f,
                    AdvanceTime = time,
                    Text = ServiceManager.Get<LocalizationManager>().Localize(text)
                };
               ServiceManager.Get<GameLogic>().Stack.PushTextbox(config, anchor);
               return EmptyEvent;
           }
       };
   }

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

    public static IStoryboardEvent MoveCharacter(Character character, List<Direction> path)
    {
       return new StoryboardFunctionEvent
       {
           Function = (storyboard) =>
           {
               character.FollowPath(path);
               return new FollowPathEvent(character);
           }
       };
    }

    public static IStoryboardEvent SetCharacterDirection(Character character, Direction direction)
    {
       return new StoryboardFunctionEvent
       {
           Function = (storyboard) =>
           {
               character.UpdateDirection(direction);
               return EmptyEvent;
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

    public static IStoryboardEvent HandOffToExploreState(string mapName)
    {
        return new StoryboardFunctionEvent
        {
            Function = (storyboard) =>
            {
                var obj = GameObject.Find($"{mapName}Map");
                var map = obj.GetComponent<Map>();
                var exploreState = map.gameObject.GetComponent<ExploreState>();
                LogManager.LogDebug($"Handing off to explore state.");
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