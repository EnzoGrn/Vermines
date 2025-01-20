using UnityEngine;

namespace Vermines
{
    /// <summary>
    /// Game Settings.
    /// 
    /// Round : A round is a full game played by all players
    /// Turn : A turn is a full player's actions cycle (sacrifice, toggle card effect, draw card, etc.)
    /// 
    /// TODO: Adjust Restictions for settings
    /// - MaxPlayers: 2-4
    /// - MaxRounds: 1-100
    /// - MaxTurnTime: 60-300
    /// - NumberOfCardsToStartWith: 1-20
    /// - MaxCardsPerPlayerInHand: NumberOfCardsToStartWith-25 // (Optional)
    /// - NumberOfCardsToDrawPerTurn: 1-5
    /// - MaxCardsNumber: NumberOfCardsToStartWith-25 // (Optional)
    /// - MaxEloquence: 1-50
    /// - EloquenceToStartWith: 0-MaxEloquence
    /// - NumberOfEloquencesToStartTheTurnWith: 0-MaxEloquence
    /// - MaxAmesToWin: 1-200
    /// - AmesToStartWith: 0-MaxAmesToWin
    /// - MaxCardsToPlay: 1-3
    /// - MaxCardsToPlayPerTurn: 1-MaxCardsToPlay
    /// - MaxPlayerEquipementCards: 1-3
    /// - MaxSacrificesPerTurn: 1-MaxCardsToPlay
    /// - MaxMarketCards: 1-5
    /// - MaxCourtyardCards: 1-5
    /// 
    /// </summary>
    [CreateAssetMenu(menuName = "Vermines/Settings/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Player Settings")]
        [Tooltip("Maximum number of players allowed in the game.")]
        public int MaxPlayers = 4;
        [Tooltip("Minimum number of players required to start the game.")]
        public int MinPlayers = 2;

        [Header("Game Flow")]
        public bool IsRoundBased;
        [Tooltip("Maximum number of rounds per game.")]
        public int MaxRounds = 15;
        [Tooltip("Time (in seconds) allowed per player's turn.")]
        public int MaxTurnTime = 300;

        [Header("Hand Settings")]
        [Tooltip("Number of cards each player starts with in their hand.")]
        public int NumberOfCardsToStartWith = 1;
        [Tooltip("Toggle whether to enforce a maximum number of cards per player in hand.")]
        public bool IsMaxCardsPerPlayerInHandBased = false;
        [Tooltip("Maximum number of cards a player can hold in their hand.")]
        public int MaxCardsPerPlayerInHand = 10;

        [Header("Deck Settings")]
        [Tooltip("Number of cards drawn per turn.")]
        public int NumberOfCardsToDrawPerTurn = 1;
        [Tooltip("Toggle whether to enforce a maximum number of cards per player.")]
        public bool IsMaxCardsNumberBased = false;
        [Tooltip("Maximum number of cards a player can hold in their deck.")]
        public int MaxCardsNumber = 25;

        [Header("General Game Rules")]
        [Tooltip("Maximum amount of eloquence a player can have.")]
        public int MaxEloquence = 20;
        [Tooltip("Initial eloquence for each player at the start of the game.")]
        public int EloquenceToStartWith = 0;
        [Tooltip("Amount of eloquence a player receives at the start of each turn.")]
        public int NumberOfEloquencesToStartTheTurnWith = 2;

        [Tooltip("Maximum number of souls needed to win.")]
        public int MaxSoulsToWin = 100;
        [Tooltip("Initial number of souls each player starts with.")]
        public int SoulsToStartWith = 0;

        [Header("Card To Play Settings")]
        public int MaxCardsToPlay = 3;
        public int MaxCardsToPlayPerTurn = 3;
        public int MaxPlayerEquipementCards = 3;

        [Header("Sacrifice Settings")]
        [Tooltip("Maximum number of followers a player can sacrifice per turn.")]
        public int MaxSacrificesPerTurn = 2;

        [Header("Market Settings")]
        [Tooltip("Number of cards to replenish in the market at the end of a turn.")]
        public int MaxMarketCards = 5;
        [Tooltip("Indicates whether the market deck reshuffles when exhausted.")]
        public bool AllowMarketReshuffle = true;

        [Header("Courtyard  Settings")]
        [Tooltip("Number of equipment cards a player can attach to their cultist.")]
        public int MaxCourtyardCards = 5;
        [Tooltip("The cost of reshuffling the deck if required (optional balance setting).")]
        public bool AllowCourtyardReshuffle = true;

        [Header("Advanced Settings")]
        [Tooltip("Enable debug mode for additional logs and testing features.")]
        public bool DebugMode = false;
        [Tooltip("Seed for random number generation (set to 0 for random seed).")]
        public int RandomSeed = 0;
    }
}


