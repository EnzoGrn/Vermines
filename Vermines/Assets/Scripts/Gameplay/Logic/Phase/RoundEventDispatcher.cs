using System.Collections.Generic;
using System;
using Fusion;

public static class RoundEventDispatcher {

    private static Dictionary<PlayerRef, List<Action<PlayerRef>>> Events = new();

    public static void RegisterEvent(PlayerRef player, Action<PlayerRef> action)
    {
        if (!Events.ContainsKey(player))
            Events.Add(player, new List<Action<PlayerRef>>());
        Events[player].Add(action);
    }

    public static void ExecutePlayerEvents(PlayerRef player)
    {
        if (Events.TryGetValue(player, out var actions)) {
            foreach (var eventAction in actions)
                eventAction?.Invoke(player);
            actions.Clear();
        }
    }
}
