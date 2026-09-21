/// <summary>
/// A pushable/poppable UI mode, managed as a stack by
/// <see cref="UIContextManager"/> (e.g. "player is choosing a card to
/// sacrifice", "a forced discard is pending"). Contexts can interrupt each
/// other; the manager's banner stack shows the player where they are in a
/// chain of interruptions.
///
/// There are three legitimate ways to implement this interface, depending on
/// what the context actually needs to do. Pick the one that matches - don't
/// invent a fourth shape without a good reason:
///
/// 1. "Show a screen with a parameter, hide it on exit" - the most common
///    case (card selection popups, shop screens, etc). Inherit
///    <see cref="ShowScreenContext{TScreen, TParam}"/> instead of
///    implementing this interface directly. See CardRebornContext,
///    CopyContext, RemoveToEarnContext for examples.
///
/// 2. "React to a game event, then pop myself" - for contexts that don't
///    show their own screen but wait for something to happen elsewhere (e.g.
///    ForceDiscardContext waits for GameEvents.OnCardDiscarded). Subscribe in
///    Enter(), unsubscribe in Exit().
///
/// 3. "Toggle state on an already-visible screen" - for contexts that piggy-
///    back on a screen that's shown independently of this context (e.g.
///    SacrificeContext calls GameplayUITable.UpdateUIForPhase(...) rather
///    than showing/hiding a screen itself). Rare - most cases fit #1 or #2.
///
/// If a context needs no constructor arguments, it can be pushed via
/// <see cref="UIContextManager.PushContext{T}()"/> (requires a parameterless
/// constructor); otherwise construct it yourself and push the instance via
/// <see cref="UIContextManager.PushContext(IUIContext)"/>.
/// </summary>
public interface IUIContext
{
    /// <summary>Called when this context becomes active (pushed, or set as root).</summary>
    void Enter();

    /// <summary>Called when this context is popped or replaced.</summary>
    void Exit();

    /// <summary>
    /// Short, player-facing label shown on the context's banner. NOT
    /// localized today (hardcoded English strings across all current
    /// implementations) - worth confirming whether that's intended before
    /// shipping in other languages, see accompanying message.
    /// </summary>
    string GetName();
}
