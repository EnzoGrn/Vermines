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
            Debug.Log($"[TrackedEvent<void>: {_eventName}] Added listener. Total: {_listeners.Count}");
        }
    }

    public void RemoveListener(UnityAction listener)
    {
        if (_listeners.Remove(listener))
        {
            _event.RemoveListener(listener);
            Debug.Log($"[TrackedEvent<void>: {_eventName}] Removed listener. Total: {_listeners.Count}");
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
            Debug.Log($"[TrackedEvent<{typeof(T).Name}>: {_eventName}] Added listener. Total: {_listeners.Count}");
        }
    }

    public void RemoveListener(UnityAction<T> listener)
    {
        if (_listeners.Remove(listener))
        {
            _event.RemoveListener(listener);
            Debug.Log($"[TrackedEvent<{typeof(T).Name}>: {_eventName}] Removed listener. Total: {_listeners.Count}");
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
            Debug.Log($"[TrackedEvent<{typeof(T1).Name}, {typeof(T2).Name}>: {_eventName}] Listener added. Total: {_listeners.Count}");
        }
    }

    public void RemoveListener(UnityAction<T1, T2> listener)
    {
        if (_listeners.Remove(listener))
        {
            _event.RemoveListener(listener);
            Debug.Log($"[TrackedEvent<{typeof(T1).Name}, {typeof(T2).Name}>: {_eventName}] Listener removed. Total: {_listeners.Count}");
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
