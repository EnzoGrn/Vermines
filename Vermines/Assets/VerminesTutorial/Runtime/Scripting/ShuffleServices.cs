using System;
using System.Collections.Generic;

namespace Vermines.Tutorial
{
    /// Les piles que le jeu mélange.
    public enum PileKind
    {
        PlayerDeck,
        CourtI,
        CourtII,
        Other
    }

    /// Identifie une pile précise : le deck du joueur 1, la pioche de cour II...
    public readonly struct PileKey : IEquatable<PileKey>
    {
        public readonly PileKind Kind;
        public readonly int PlayerIndex;
        public readonly string Name;

        public PileKey(PileKind kind, int playerIndex = -1, string name = null)
        {
            Kind = kind;
            PlayerIndex = playerIndex;
            Name = name;
        }

        public static PileKey Deck(int playerIndex) => new PileKey(PileKind.PlayerDeck, playerIndex);
        public static readonly PileKey CourtI = new PileKey(PileKind.CourtI);
        public static readonly PileKey CourtII = new PileKey(PileKind.CourtII);
        public static PileKey Custom(string name) => new PileKey(PileKind.Other, -1, name);

        public bool Equals(PileKey other) => Kind == other.Kind && PlayerIndex == other.PlayerIndex && Name == other.Name;
        public override bool Equals(object obj) => obj is PileKey k && Equals(k);
        public override int GetHashCode() => ((int)Kind * 397) ^ PlayerIndex ^ (Name?.GetHashCode() ?? 0);
        public override string ToString() => Kind == PileKind.PlayerDeck ? $"Deck(J{PlayerIndex + 1})" : Kind == PileKind.Other ? Name : Kind.ToString();
    }

    /// Tout le hasard du jeu passe par ici. En partie normale c'est un mélange aléatoire
    /// (avec une seed, utile aussi pour rejouer un bug) ; dans une leçon du tutoriel
    /// c'est un ordre écrit à l'avance.
    ///
    /// Convention : après Shuffle, l'index 0 de la liste est le DESSUS de la pile
    /// (la prochaine carte piochée). Si le jeu pioche par la fin de la liste, passer
    /// topIsFirstIndex: false au ScriptedShuffleService.
    public interface IShuffleService
    {
        void Shuffle<T>(IList<T> cards, PileKey pile, Func<T, string> getCardId);

        /// Tirage d'un entier dans [minInclusive, maxExclusive) pour tout autre hasard
        /// (premier joueur, choix de l'IA...). reason sert aux logs et au scripting.
        int Range(int minInclusive, int maxExclusive, string reason);
    }

    /// Mélange de Fisher-Yates avec une seed. Comportement d'une partie normale.
    public sealed class SeededShuffleService : IShuffleService
    {
        private readonly Random _random;

        public int Seed { get; }

        public SeededShuffleService(int seed)
        {
            Seed = seed;
            _random = new Random(seed);
        }

        public void Shuffle<T>(IList<T> cards, PileKey pile, Func<T, string> getCardId)
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }

        public int Range(int minInclusive, int maxExclusive, string reason) => _random.Next(minInclusive, maxExclusive);
    }

    /// Point d'accès unique au hasard pour le code du jeu. C'est LA modification à
    /// faire dans le jeu : remplacer chaque mélange / Random.Range par GameRandom.
    ///
    ///   GameRandom.Shuffle(player.Deck, PileKey.Deck(player.Index), c => c.Id);
    ///   int first = GameRandom.Range(0, playerCount, "first_player");
    ///
    /// Le TutorialManager remplace Current par un ScriptedShuffleService le temps
    /// d'une leçon, puis remet le service précédent.
    public static class GameRandom
    {
        private static IShuffleService _current;

        public static IShuffleService Current
        {
            get => _current ?? (_current = new SeededShuffleService(Environment.TickCount));
            set => _current = value;
        }

        public static void Shuffle<T>(IList<T> cards, PileKey pile, Func<T, string> getCardId) =>
            Current.Shuffle(cards, pile, getCardId);

        public static int Range(int minInclusive, int maxExclusive, string reason = null) =>
            Current.Range(minInclusive, maxExclusive, reason);
    }

    /// Mélange "scripté" : chaque fois qu'une pile est mélangée, on lui applique le
    /// prochain ordre prévu pour elle (deck de départ, puis 1er remélange de la
    /// défausse, 2e remélange...). Les cartes non citées dans l'ordre sont placées
    /// après, mélangées par le service de repli. Quand il n'y a plus d'ordre prévu,
    /// le service de repli prend le relais.
    public sealed class ScriptedShuffleService : IShuffleService
    {
        private readonly Dictionary<PileKey, Queue<IReadOnlyList<string>>> _orders =
            new Dictionary<PileKey, Queue<IReadOnlyList<string>>>();
        private readonly Dictionary<string, Queue<int>> _rolls =
            new Dictionary<string, Queue<int>>(StringComparer.OrdinalIgnoreCase);
        private readonly IShuffleService _fallback;
        private readonly bool _topIsFirstIndex;

        /// Avertissements de cohérence (carte prévue absente de la pile, etc.).
        /// Branché sur Debug.LogWarning par le TutorialManager.
        public event Action<string> OnWarning;

        public ScriptedShuffleService(IShuffleService fallback, bool topIsFirstIndex = true)
        {
            _fallback = fallback ?? new SeededShuffleService(0);
            _topIsFirstIndex = topIsFirstIndex;
        }

        /// Construit le service à partir de la mise en place d'une leçon.
        public static ScriptedShuffleService FromSetup(ScriptedMatchSetup setup, bool topIsFirstIndex = true)
        {
            var service = new ScriptedShuffleService(new SeededShuffleService(setup?.fallbackSeed ?? 0), topIsFirstIndex);
            if (setup == null)
                return service;

            for (int p = 0; p < setup.players.Count; p++)
            {
                var player = setup.players[p];
                if (player.deckTopToBottom.Count > 0)
                    service.EnqueueOrder(PileKey.Deck(p), player.deckTopToBottom);
                foreach (var reshuffle in player.reshuffleOrders)
                    service.EnqueueOrder(PileKey.Deck(p), reshuffle.cardIds);
            }
            if (setup.courtPileI.Count > 0)
                service.EnqueueOrder(PileKey.CourtI, setup.courtPileI);
            if (setup.courtPileII.Count > 0)
                service.EnqueueOrder(PileKey.CourtII, setup.courtPileII);
            foreach (var roll in setup.scriptedRolls)
                service.EnqueueRoll(roll.reason, roll.value);
            return service;
        }

        public void EnqueueOrder(PileKey pile, IReadOnlyList<string> topToBottom)
        {
            if (!_orders.TryGetValue(pile, out var queue))
                _orders[pile] = queue = new Queue<IReadOnlyList<string>>();
            queue.Enqueue(topToBottom);
        }

        public void EnqueueRoll(string reason, int value)
        {
            reason = reason ?? string.Empty;
            if (!_rolls.TryGetValue(reason, out var queue))
                _rolls[reason] = queue = new Queue<int>();
            queue.Enqueue(value);
        }

        public int PendingOrders(PileKey pile) => _orders.TryGetValue(pile, out var q) ? q.Count : 0;

        public void Shuffle<T>(IList<T> cards, PileKey pile, Func<T, string> getCardId)
        {
            if (!_orders.TryGetValue(pile, out var queue) || queue.Count == 0)
            {
                _fallback.Shuffle(cards, pile, getCardId);
                return;
            }

            IReadOnlyList<string> order = queue.Dequeue();
            var remaining = new List<T>(cards);
            var result = new List<T>(cards.Count);

            foreach (string id in order)
            {
                int index = remaining.FindIndex(c => string.Equals(getCardId(c), id, StringComparison.OrdinalIgnoreCase));
                if (index < 0)
                {
                    OnWarning?.Invoke($"[Tutoriel] {pile} : la carte \"{id}\" prévue dans l'ordre scripté n'est pas dans la pile.");
                    continue;
                }
                result.Add(remaining[index]);
                remaining.RemoveAt(index);
            }

            if (remaining.Count > 0)
            {
                OnWarning?.Invoke($"[Tutoriel] {pile} : {remaining.Count} carte(s) non prévue(s) par le script, placées sous les autres dans un ordre aléatoire.");
                _fallback.Shuffle(remaining, pile, getCardId);
                result.AddRange(remaining);
            }

            if (!_topIsFirstIndex)
                result.Reverse();

            for (int i = 0; i < cards.Count; i++)
                cards[i] = result[i];
        }

        public int Range(int minInclusive, int maxExclusive, string reason)
        {
            if (_rolls.TryGetValue(reason ?? string.Empty, out var queue) && queue.Count > 0)
            {
                int value = queue.Dequeue();
                if (value >= minInclusive && value < maxExclusive)
                    return value;
                OnWarning?.Invoke($"[Tutoriel] Tirage \"{reason}\" scripté à {value}, hors de [{minInclusive}, {maxExclusive}) : ignoré.");
            }
            return _fallback.Range(minInclusive, maxExclusive, reason);
        }
    }
}
