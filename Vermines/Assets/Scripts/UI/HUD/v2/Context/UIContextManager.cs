using System.Collections.Generic;
using UnityEngine;

public class UIContextManager : MonoBehaviour
{
    public static UIContextManager Instance { get; private set; }

    private Stack<IUIContext> _contextStack = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool HasContext() => _contextStack.Count > 0;

    public IUIContext CurrentContext => _contextStack.Count > 0 ? _contextStack.Peek() : null;

    public void SetContext(IUIContext context)
    {
        ClearContext(); // on vide tout sauf le nouveau
        _contextStack.Push(context);
        context.Enter();
    }

    public void PushContext(IUIContext context)
    {
        Debug.Log($"[UIContextManager] Pushing context: {context}");
        _contextStack.Push(context);
        context.Enter();
    }

    public void PopContext()
    {
        if (_contextStack.Count > 0)
        {
            Debug.Log($"[UIContextManager] Popping context: {_contextStack.Peek()}, remaining: {_contextStack.Count - 1}");
            var context = _contextStack.Pop();
            context.Exit();
        }
    }

    public void ClearContext()
    {
        while (_contextStack.Count > 0)
        {
            var context = _contextStack.Pop();
            context.Exit();
        }
    }

    public bool IsInContext<T>() where T : IUIContext
    {
        foreach (var ctx in _contextStack)
        {
            if (ctx is T)
                return true;
        }
        return false;
    }

    public void PopContextOfType<T>() where T : IUIContext
    {
        var newStack = new Stack<IUIContext>();
        bool found = false;

        while (_contextStack.Count > 0)
        {
            var context = _contextStack.Pop();
            if (!found && context is T)
            {
                context.Exit();
                found = true;
                continue;
            }
            newStack.Push(context);
        }

        while (newStack.Count > 0)
        {
            _contextStack.Push(newStack.Pop());
        }

        Debug.Log($"[UIContextManager] Popped context of type {typeof(T)}. Remaining contexts: {_contextStack.Count}");
    }

    public void PushUniqueContext<T>(T context) where T : IUIContext
    {
        PopContextOfType<T>();
        PushContext(context);
    }

    public T GetContext<T>() where T : class, IUIContext
    {
        foreach (var context in _contextStack)
        {
            if (context is T typedContext)
                return typedContext;
        }
        return null;
    }
}
