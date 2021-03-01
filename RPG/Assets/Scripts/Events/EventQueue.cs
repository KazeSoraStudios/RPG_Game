using System.Collections.Generic;
using UnityEngine;

public interface IEvent
{
    void Execute(EventQueue queue);
    void Update();
    bool IsFinished();
    string GetName();
    Actor GetActor();
    int GetPriority();
    void SetPriority(int value);
    int CalculatePriority(EventQueue queue);
}

public class EmptyEvent : IEvent
{
    private int priority;
    private Actor actor;

    public EmptyEvent()
    {
        priority = 0;
        actor = new Actor();
    }

    void IEvent.Execute(EventQueue queue) { }
    void IEvent.Update() { }
    bool IEvent.IsFinished() { return true;  }
    Actor IEvent.GetActor() { return actor; }
    string IEvent.GetName() { return "EmptyEvent"; }
    int IEvent.GetPriority() { return priority; }
    void IEvent.SetPriority(int value) { }
    int IEvent.CalculatePriority(EventQueue queue) { return 0; }
}


public class EventQueue : MonoBehaviour
{
    private IEvent currentEvent;
    private readonly EmptyEvent emptyEvent = new EmptyEvent();
    private List<IEvent> events = new List<IEvent>();

    void Start()
    {
        currentEvent = emptyEvent;
        events = new List<IEvent>();
    }

    public void Add(IEvent e, int points = -1)
    {
        if (points < 0)
        {
            e.SetPriority(-1);
            events.Add(e);
            return;
        }
        e.SetPriority(points);
        int index = 0;
        for (; index < events.Count; index++)
            if (points > events[index].GetPriority())
                break;
        events.Insert(index, e);
    }

    public int SpeedToPoints(int speed)
    {
        speed = Mathf.Min(speed, Constants.MAX_SPEED);
        return (int)Mathf.Floor(Constants.MAX_SPEED - speed);
    }

    public void Execute()
    {
        foreach (var e in events)
        {
            var countDown = e.GetPriority();
            e.SetPriority(Mathf.Max(0, --countDown));
        }

        if (currentEvent.GetPriority() != Constants.EMPTY_EVENT_COUNTDOWN)
        {
            currentEvent.Update();
            if (currentEvent.IsFinished())
                currentEvent = emptyEvent;
            else
                return;
        }
        if (IsEmpty())
            return;
        var firstEvent = events[0];
        events.RemoveAt(0);
        LogManager.LogDebug($"Removing first event from combat queue. Name [{firstEvent.GetName()}] Speed [{firstEvent.GetPriority()}]");
        firstEvent.Execute(this);
        currentEvent = firstEvent;
    }

    public void Clear()
    {
        events.Clear();
        currentEvent = emptyEvent;
    }

    public bool IsEmpty()
    {
        return events == null || events.Count < 1;
    }

    public bool ActorHasEvent(int actorId)
    {
        if (actorId == currentEvent.GetActor().Id)
            return true;
        foreach (var e in events)
            if (actorId == e.GetActor().Id)
                return true;
        return false;
    }

    public void RemoveEventsForActor(int actorId)
    {
        for (int i = events.Count - 1; i > -1; i--)
            if (actorId == events[i].GetActor().Id)
                events.RemoveAt(i);
    }

    public void PrintEvents()
    {
        if (IsEmpty())
        {
            LogManager.LogDebug("Event queue is empty.");
            return;
        }
        LogManager.LogDebug("Event Queue:");
        LogManager.LogDebug($"CurrentEvent: {currentEvent.GetName()}");
        for(int i = 0; i < events.Count; i++)
        {
            var message = $"{i} Event: [{events[i].GetPriority()}][{events[i].GetName()}]";
            LogManager.LogDebug(message);
        }
    }

#if UNITY_EDITOR
    public bool ShowEvents = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            events.Add(new EmptyEvent());
        }
    }

    public void OnGUI()
    {
        if (!ShowEvents)
            return;
        float yDiff = 30.0f;
        var position = Vector2.zero;
        GUI.Label(new Rect(position, Vector2.one * 500.0f), "Event Queue:");
        position.y += yDiff;
        GUI.Label(new Rect(position, Vector2.one * 500.0f), $"Current Event: {currentEvent.GetName()}");
        position.y += yDiff;

        if (IsEmpty())
            GUI.Label(new Rect(position, Vector2.one * 100.0f), "Empty!");

        for (int i = 0; i < events.Count; i++)
        {
            var message = $"{i} Event: [{events[i].GetPriority()}][{events[i].GetName()}]";
            GUI.Label(new Rect(position, Vector2.one * 500.0f), message);
            position.y += yDiff;
        }
    }
#endif
}