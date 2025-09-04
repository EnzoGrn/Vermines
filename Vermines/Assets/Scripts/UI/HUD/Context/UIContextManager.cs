using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vermines.UI;

/// <summary>
/// Manages UI context transitions using a stack-based system.
/// </summary>
public class UIContextManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the UIContextManager.
    /// </summary>
    public static UIContextManager Instance { get; private set; }

    [Header("UI References")]

    /// <summary>
    /// Banner GameObject that displays the current context name.
    /// </summary>
    [InlineHelp, SerializeField]
    private GameObject _bannerPrefab;

    /// <summary>
    /// Transform for the banner container where banners will be displayed.
    /// </summary>
    [InlineHelp, SerializeField]
    private Transform _bannerContainer;

    /// <summary>
    /// The number of banners to display at once.
    /// This number represents the N most recent contexts.
    [InlineHelp, SerializeField]
    private int _maxBanners = 3;

    protected Stack<IUIContext> _contextStack = new();
    private readonly List<GameObject> _activeBanners = new();

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

        if (_bannerContainer != null)
        {
            foreach (Transform child in _bannerContainer)
                Destroy(child.gameObject);
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
        RefreshContextBanners();
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
        RefreshContextBanners();
    }

    /// <summary>
    /// Pushes a new UI context of type <typeparamref name="T"/> onto the context stack and enters it.
    /// </summary>
    /// <typeparam name="T">The type of UI context to push. Must be a reference type implementing <see cref="IUIContext"/>.</typeparam>
    /// <remarks>
    /// This method retrieves the specified context via <c>GetContext&lt;T&gt;()</c>, adds it to the internal stack,
    /// logs the action for debugging, and calls <c>Enter()</c> on the context to activate it.
    /// </remarks>
    public void PushContext<T>() where T : class, IUIContext, new()
    {
        var context = GetContext<T>() ?? new T();
        Debug.LogFormat(gameObject, "[{0}] Pushing context: {1}", nameof(UIContextManager), context);
        _contextStack.Push(context);
        context.Enter();
        RefreshContextBanners();
    }

    /// <summary>
    /// Pops the current top context from the stack.
    /// </summary>
    public void PopContext()
    {
        if (_contextStack.Count > 0)
        {
            var context = _contextStack.Pop();
            context.Exit();
            RefreshContextBanners();
        }
    }

    /// <summary>
    /// Clears all contexts from the stack.
    /// </summary>
    public void ClearContext()
    {
        Debug.LogFormat(gameObject, "[{0}] Clearing all contexts.", nameof(UIContextManager));
        while (_contextStack.Count > 0)
        {
            Debug.LogFormat(gameObject, "[{0}] Popping context: {1}", nameof(UIContextManager), _contextStack.Peek());
            var context = _contextStack.Pop();
            context.Exit();
        }
        ClearBanners();
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

        RefreshContextBanners();

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

    private string GetContextName(IUIContext context)
    {
        // TODO: Implement Localization support for context names
        return context.GetName();
    }

    private void RefreshContextBanners()
    {
        StartCoroutine(RefreshBannersCoroutine());
    }

    private IEnumerator RefreshBannersCoroutine()
    {
        var tempStack = new Stack<IUIContext>(_contextStack);
        var recentContexts = new List<IUIContext>();
        while (tempStack.Count > 0 && recentContexts.Count < _maxBanners)
        {
            recentContexts.Insert(0, tempStack.Pop());
        }

        for (int i = _activeBanners.Count - 1; i >= 0; i--)
        {
            var bannerGO = _activeBanners[i];
            var banner = bannerGO.GetComponent<ContextBanner>();
            var context = banner?.AssociatedContext;

            if (!recentContexts.Contains(context))
            {
                _activeBanners.RemoveAt(i);
                if (banner != null)
                    yield return StartCoroutine(banner.PlayHideAnimation());
                GameObject.Destroy(bannerGO);
            }
        }

        for (int i = 0; i < recentContexts.Count; i++)
        {
            var context = recentContexts[i];
            bool found = false;

            for (int j = 0; j < _activeBanners.Count; j++)
            {
                var banner = _activeBanners[j].GetComponent<ContextBanner>();
                if (banner != null && banner.AssociatedContext == context)
                {
                    if (j != i)
                    {
                        _activeBanners[j].transform.SetSiblingIndex(i);
                    }
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var bannerGO = Instantiate(_bannerPrefab, _bannerContainer);
                var banner = bannerGO.GetComponent<ContextBanner>();
                var textComponent = bannerGO.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = GetContextName(context);
                }

                if (banner != null)
                {
                    banner.AssociatedContext = context;
                    yield return StartCoroutine(banner.PlayShowAnimCoroutine());
                }

                _activeBanners.Insert(i, bannerGO);
                bannerGO.transform.SetSiblingIndex(i);
            }
        }
    }

    private void ClearBanners()
    {
        foreach (var banner in _activeBanners)
        {
            Destroy(banner);
        }
        _activeBanners.Clear();
    }
}
