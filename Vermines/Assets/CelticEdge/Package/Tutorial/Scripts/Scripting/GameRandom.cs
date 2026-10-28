using System.Collections.Generic;
using System;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// All the game's randomness happens here.
    /// In a normal game, it's a random draw (using a seed, which is also useful for reproducing a bug).
    /// In a tutorial lesson, it's a predefined order.
    ///
    /// A stack is identified by a free-form text string chosen by the game ("deck:0" for player 0's deck, "loot_table", "spawn_points", etc.).
    ///
    /// Convention: after Shuffle, index 0 of the list is the TOP of the stack (the next element to be drawn).
    /// If the game draws from the end of the list, set "_TopOfPileIsIndexZero" to false on the TutorialManager.
    /// </summary>
    public interface IGameRandom {

        void Shuffle<T>(IList<T> items, string pileId, Func<T, string> getId);

        /// <summary>
        /// Generate an integer in the range [minInclusive, maxExclusive) for any other random event (first player, AI choice, critical hit...).
        /// Reason is used for logging and scripting.
        /// </summary>
        int Range(int minInclusive, int maxExclusive, string reason);
    }

    /// <summary>
    /// Fisher-Yates sort with a seed. Behavior of a normal game.
    /// </summary>
    public sealed class SeededGameRandom : IGameRandom {

        private readonly System.Random _Random;

        public int Seed { get; }

        public SeededGameRandom(int seed)
        {
            Seed    = seed;
            _Random = new System.Random(seed);
        }

        public void Shuffle<T>(IList<T> items, string pileId, Func<T, string> getId)
        {
            for (int i = items.Count - 1; i > 0; i--) {
                int j = _Random.Next(i + 1);

                (items[i], items[j]) = (items[j], items[i]);
            }
        }

        public int Range(int minInclusive, int maxExclusive, string reason) => _Random.Next(minInclusive, maxExclusive);
    }

    /// <summary>
    /// Single random access point for the game code.
    /// This is THE change you need to make in the game to be able to script a lesson:
    /// replace every shuffle and every Random.Range that matters with GameRandom.
    ///
    /// <code>
    ///   GameRandom.Shuffle (player.Deck, $"deck:{player.Index}", c => c.Id);
    ///   
    ///   int first = GameRandom.Range(0, playerCount, "first_player");
    /// </code>
    ///
    /// The TutorialManager replaces Current with a ScriptedGameRandom for the duration of a lesson, then restores the previous service.
    /// </summary>
    public static class GameRandom {

        private static IGameRandom _Current;

        public static IGameRandom Current
        {
            get => _Current ??= new SeededGameRandom(Environment.TickCount);
            set => _Current = value;
        }

        public static void Shuffle<T>(IList<T> items, string pileId, Func<T, string> getId) => Current.Shuffle(items, pileId, getId);

        public static int Range(int minInclusive, int maxExclusive, string reason = null) => Current.Range(minInclusive, maxExclusive, reason);
    }

    /// <summary>
    /// The sort order applied to a shuffled deck.
    /// Multiple sort orders for the same deck are applied in the following order: 1st shuffle, 2nd shuffle...
    /// </summary>
    [Serializable]
    public class ScriptedOrder {

        [Tooltip("The deck ID, as passed by the game to GameRandom.Shuffle (e.g., \"deck: 0\").")]
        public string PileId;

        [Tooltip("Element IDs, from the top to the bottom of the stack.")]
        public List<string> TopToBottom = new();

        public ScriptedOrder() {}

        public ScriptedOrder(string pileId, params string[] topToBottom)
        {
            PileId = pileId;

            TopToBottom.AddRange(topToBottom);
        }
    }

    /// <summary>
    /// A GameRandom.Range draw with a predetermined result.
    /// </summary>
    [Serializable]
    public struct ScriptedRoll {

        [Tooltip("The argument passed to GameRandom.Range (e.g., \"first_player\").")]
        public string Reason;
        public int    Value;

        public ScriptedRoll(string reason, int value)
        {
            Reason = reason;
            Value  = value;
        }
    }

    /// <summary>
    /// A Lesson in Chance: Mixing Orders and Forced Draws, and the Seed for the Rest.
    /// </summary>
    [Serializable]
    public class ScriptedRandomSetup {

        public List<ScriptedOrder> Orders = new();
        public List<ScriptedRoll>  Rolls  = new();

        [Tooltip("Seed for unscripted draws: the lesson remains the same from one session to the next.")]
        public int FallbackSeed = 1;
    }

    /// <summary>
    /// "Scripted" randomness: each time a stack is shuffled, the next scheduled order is applied to it.
    /// Elements not listed in the order are placed afterward and shuffled by the fallback service.
    /// 
    /// When there are no more scheduled orders, the fallback service takes over.
    /// </summary>
    public sealed class ScriptedGameRandom : IGameRandom {

        private readonly Dictionary<string, Queue<IReadOnlyList<string>>> _Orders = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Queue<int>>                   _Rolls  = new(StringComparer.OrdinalIgnoreCase);

        private readonly IGameRandom _Fallback;
        private readonly bool _TopIsFirstIndex;

        /// <summary>
        /// Consistency warnings (expected element missing from the stack, etc.).
        /// Linked to Debug.LogWarning by the TutorialManager.
        /// </summary>
        public event Action<string> OnWarning;

        public ScriptedGameRandom(IGameRandom fallback, bool topIsFirstIndex = true)
        {
            _Fallback        = fallback ?? new SeededGameRandom(0);
            _TopIsFirstIndex = topIsFirstIndex;
        }

        public static ScriptedGameRandom FromSetup(ScriptedRandomSetup setup, bool topIsFirstIndex = true)
        {
            var service = new ScriptedGameRandom(new SeededGameRandom(setup?.FallbackSeed ?? 0), topIsFirstIndex);

            if (setup == null)
                return service;
            foreach (var order in setup.Orders) {
                if (order != null)
                    service.EnqueueOrder(order.PileId, order.TopToBottom);
            }

            foreach (var roll in setup.Rolls)
                service.EnqueueRoll(roll.Reason, roll.Value);
            return service;
        }

        public void EnqueueOrder(string pileId, IReadOnlyList<string> topToBottom)
        {
            pileId = pileId ?? string.Empty;

            if (!_Orders.TryGetValue(pileId, out var queue))
                _Orders[pileId] = queue = new Queue<IReadOnlyList<string>>();
            queue.Enqueue(topToBottom ?? Array.Empty<string>());
        }

        public void EnqueueRoll(string reason, int value)
        {
            reason = reason ?? string.Empty;

            if (!_Rolls.TryGetValue(reason, out var queue))
                _Rolls[reason] = queue = new Queue<int>();
            queue.Enqueue(value);
        }

        public int PendingOrders(string pileId) => _Orders.TryGetValue(pileId ?? string.Empty, out var q) ? q.Count : 0;

        public void Shuffle<T>(IList<T> items, string pileId, Func<T, string> getId)
        {
            if (!_Orders.TryGetValue(pileId ?? string.Empty, out var queue) || queue.Count == 0) {
                _Fallback.Shuffle(items, pileId, getId);
                
                return;
            }

            IReadOnlyList<string> order = queue.Dequeue();

            var remaining = new List<T>(items);
            var result    = new List<T>(items.Count);

            foreach (string id in order) {
                int index = remaining.FindIndex(c => string.Equals(getId(c), id, StringComparison.OrdinalIgnoreCase));

                if (index < 0) {
                    OnWarning?.Invoke($"[Tutorial] {pileId}: The \"{id}\" element specified in the scripted order is not on the stack.");

                    continue;
                }

                result.Add(remaining[index]);
                remaining.RemoveAt(index);
            }

            if (remaining.Count > 0) {
                OnWarning?.Invoke($"[Tutorial] {pileId}: {remaining.Count} item(s) not included in the script, placed below the others in random order.");

                _Fallback.Shuffle(remaining, pileId, getId);

                result.AddRange(remaining);
            }

            if (!_TopIsFirstIndex)
                result.Reverse();
            for (int i = 0; i < items.Count; i++)
                items[i] = result[i];
        }

        public int Range(int minInclusive, int maxExclusive, string reason)
        {
            if (_Rolls.TryGetValue(reason ?? string.Empty, out var queue) && queue.Count > 0) {
                int value = queue.Dequeue();

                if (value >= minInclusive && value < maxExclusive)
                    return value;
                OnWarning?.Invoke($"[Tutorial] Scripted \"{reason}\" draw at {value}, outside the range [{minInclusive}, {maxExclusive}): ignored.");
            }

            return _Fallback.Range(minInclusive, maxExclusive, reason);
        }
    }
}
