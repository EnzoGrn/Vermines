using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public interface ITrackedEvent
{
    int ListenerCount { get; }
}

public class TrackedEvent : ITrackedEvent
{
    private readonly UnityEvent _event = new();
    private readonly HashSet<UnityAction> _listeners = new();
    private readonly string _eventName;

    public TrackedEvent(string eventName)
    {
        _eventName = eventName;
    }

    public void AddListener(UnityAction listener)
    {
        if (_listeners.Add(listener))
        {
            _event.AddListener(listener);
        }
    }

    public void RemoveListener(UnityAction listener)
    {
        if (_listeners.Remove(listener))
        {
            _event.RemoveListener(listener);
        }
    }

    public void Invoke() => _event.Invoke();

    public int ListenerCount => _listeners.Count;

    public void PrintListeners()
    {
        Debug.Log($"[TrackedEvent<void>: {_eventName}] Printing listeners:");
        foreach (var listener in _listeners)
        {
            var method = listener.Method;
            var target = listener.Target;
            string targetName = target != null ? target.GetType().Name : "Static";
            Debug.Log($"  - {targetName}.{method.Name}");
        }
    }
}

public class TrackedEvent<T> : ITrackedEvent
{
    private readonly UnityEvent<T> _event = new();
    private readonly HashSet<UnityAction<T>> _listeners = new();
    private readonly string _eventName;

    public TrackedEvent(string eventName)
    {
        _eventName = eventName;
    }

    public void AddListener(UnityAction<T> listener)
    {
        if (_listeners.Add(listener))
        {
            _event.AddListener(listener);
        }
    }

    public void RemoveListener(UnityAction<T> listener)
    {
        if (_listeners.Remove(listener))
        {
            _event.RemoveListener(listener);
        }
    }

    public void Invoke(T arg) => _event.Invoke(arg);

    public int ListenerCount => _listeners.Count;

    public void PrintListeners()
    {
        Debug.Log($"[TrackedEvent<{typeof(T).Name}>: {_eventName}] Printing listeners:");
        foreach (var listener in _listeners)
        {
            var method = listener.Method;
            var target = listener.Target;
            string targetName = target != null ? target.GetType().Name : "Static";
            Debug.Log($"  - {targetName}.{method.Name}");
        }
    }
}

public class TrackedEvent<T1, T2> : ITrackedEvent
{
    private readonly UnityEvent<T1, T2> _event = new();
    private readonly HashSet<UnityAction<T1, T2>> _listeners = new();
    private readonly string _eventName;

    public TrackedEvent(string eventName)
    {
        _eventName = eventName;
    }

    public void AddListener(UnityAction<T1, T2> listener)
    {
        if (_listeners.Add(listener))
        {
            _event.AddListener(listener);
        }
    }

    public void RemoveListener(UnityAction<T1, T2> listener)
    {
        if (_listeners.Remove(listener))
        {
            _event.RemoveListener(listener);
        }
    }

    public void Invoke(T1 a, T2 b) => _event.Invoke(a, b);

    public int ListenerCount => _listeners.Count;

    public void PrintListeners()
    {
        Debug.Log($"[TrackedEvent<{typeof(T1).Name}, {typeof(T2).Name}>: {_eventName}] Printing listeners:");
        foreach (var listener in _listeners)
        {
            var method = listener.Method;
            var target = listener.Target;
            string targetName = target != null ? target.GetType().Name : "Static";
            Debug.Log($"  - {targetName}.{method.Name}");
        }
    }
}

/// <summary>
/// Like TrackedEvent&lt;T1, T2&gt;, but remembers its last invocation and
/// immediately replays it to any listener that subscribes afterward. Use ONLY
/// for standalone "here is the current/latest state" announcements where a
/// late subscriber missing it would be a real bug (e.g. a scene-loading race
/// between game logic and UI). Do NOT use this for ordinary gameplay events
/// (a card was played, an effect resolved, etc.) — replaying those to a late
/// subscriber would be actively wrong.
/// </summary>
public class LatestValueEvent<T1, T2>
{
    private readonly TrackedEvent<T1, T2> _event;
    private bool _hasValue;
    private T1 _lastArg1;
    private T2 _lastArg2;

    public LatestValueEvent(string eventName)
    {
        _event = new TrackedEvent<T1, T2>(eventName);
    }

    public void Invoke(T1 a, T2 b)
    {
        _hasValue = true;
        _lastArg1 = a;
        _lastArg2 = b;

        _event.Invoke(a, b);
    }

    // Subscribes AND immediately replays the last known value, if any —
    // covers the case where the emitter already fired before this listener
    // was ready.
    public void AddListenerAndReplay(UnityAction<T1, T2> listener)
    {
        _event.AddListener(listener);

        if (_hasValue)
            listener(_lastArg1, _lastArg2);
    }

    public void RemoveListener(UnityAction<T1, T2> listener) => _event.RemoveListener(listener);

    public int ListenerCount => _event.ListenerCount;
}

public class LatestValueEvent
{
    private readonly TrackedEvent _event;
    private bool _hasFired;

    public LatestValueEvent(string eventName)
    {
        _event = new TrackedEvent(eventName);
    }

    public void Invoke()
    {
        _hasFired = true;
        _event.Invoke();
    }

    public void AddListenerAndReplay(UnityAction listener)
    {
        _event.AddListener(listener);

        if (_hasFired)
            listener();
    }

    public void RemoveListener(UnityAction listener) => _event.RemoveListener(listener);
}
