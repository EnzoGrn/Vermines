using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages UI context transitions using a stack-based system.
/// </summary>
public class UIContextManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the UIContextManager.
    /// </summary>
    public static UIContextManager Instance { get; private set; }

    protected Stack<IUIContext> _contextStack = new();

    /// <summary>
    /// Initializes the singleton instance.
    /// </summary>
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

    /// <summary>
    /// Returns whether there is any context currently active.
    /// </summary>
    public bool HasContext() => _contextStack.Count > 0;

    /// <summary>
    /// Gets the currently active context, or null if none.
    /// </summary>
    public IUIContext CurrentContext => _contextStack.Count > 0 ? _contextStack.Peek() : null;

    /// <summary>
    /// Clears the current context stack and sets a new root context.
    /// </summary>
    /// <param name="context">The new context to set.</param>
    public void SetContext(IUIContext context)
    {
        ClearContext();
        _contextStack.Push(context);
        context.Enter();
    }

    /// <summary>
    /// Pushes a new context on top of the current stack.
    /// </summary>
    /// <param name="context">The context to push.</param>
    public void PushContext(IUIContext context)
    {
        Debug.LogFormat(gameObject, "[{0}] Pushing context: {1}", nameof(UIContextManager), context);
        _contextStack.Push(context);
        context.Enter();
    }

    /// <summary>
    /// Pops the current top context from the stack.
    /// </summary>
    public void PopContext()
    {
        if (_contextStack.Count > 0)
        {
            var context = _contextStack.Pop();
            Debug.LogFormat(gameObject, "[{0}] Popping context: {1}, remaining: {2}", nameof(UIContextManager), context, _contextStack.Count);
            context.Exit();
        }
    }

    /// <summary>
    /// Clears all contexts from the stack.
    /// </summary>
    public void ClearContext()
    {
        while (_contextStack.Count > 0)
        {
            var context = _contextStack.Pop();
            context.Exit();
        }
    }

    /// <summary>
    /// Checks if a context of type <typeparamref name="T"/> exists in the stack.
    /// </summary>
    /// <typeparam name="T">The type of context to search for.</typeparam>
    /// <returns>True if found, otherwise false.</returns>
    public bool IsInContext<T>() where T : IUIContext
    {
        foreach (var ctx in _contextStack)
        {
            if (ctx is T)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Pops and removes the first found context of type <typeparamref name="T"/> from the stack.
    /// </summary>
    /// <typeparam name="T">The type of context to remove.</typeparam>
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

        Debug.LogFormat(gameObject, "[{0}] Popped context of type {1}. Remaining contexts: {2}", nameof(UIContextManager), typeof(T).Name, _contextStack.Count);
    }

    /// <summary>
    /// Pushes a context of type <typeparamref name="T"/> only if one doesn't already exist.
    /// If it exists, the old one is removed first.
    /// </summary>
    /// <param name="context">The context instance to push.</param>
    /// <typeparam name="T">The context type.</typeparam>
    public void PushUniqueContext<T>(T context) where T : IUIContext
    {
        PopContextOfType<T>();
        PushContext(context);
    }

    /// <summary>
    /// Retrieves a context of the given type <typeparamref name="T"/> from the stack.
    /// </summary>
    /// <typeparam name="T">The type of context to retrieve.</typeparam>
    /// <returns>The context instance if found, or null.</returns>
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
