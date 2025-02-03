using UnityEngine;

namespace Vermines.Config {

    using Vermines.Config.Utils;

    /// <summary>
    /// Game Settings.
    /// 
    /// Contains all the settings for the game.
    /// 
    /// For now settings must be grouped by category.
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "New Game Config", menuName = "Vermines/Game/Create a new game config")]
    public class GameConfiguration : ScriptableObject {

        #region Game information

        [Header("Game Information")]

        private int _Seed = 0;

        public int Seed
        {
            get => _Seed;
            set
            {
                if (value != _Seed)
                {
                    _Seed = value;
                    Rand = new System.Random(value);
                }
            }
        }

        public System.Random Rand = new(0);

        #endregion

        #region Player configuration

        [Header("Player Settings")]

        [Tooltip("Maximum number of players allowed in the game.")]
        public IntSetting MaxPlayers = new("Max Players", 4, 2, 4, "Player Settings");

        [Tooltip("Minimum number of players required to start the game.")]
        public IntSetting MinPlayers = new("Min Players", 2, 2, 4, "Player Settings");

        #endregion

        #region Game flow configuration

        [Header("Game Flow Settings")]

        [Tooltip("Toggle if you want the game have limited rounds.")]
        public BoolSetting IsRoundLimited = new("Is Round Based", false, "Game Flow Settings");

        [Tooltip("Maximum number of rounds per game.")]
        public IntSetting MaxRounds = new("Max Rounds", 15, 1, 100, "Game Flow Settings");

        [Tooltip("Time (in seconds) allowed per player's turn.")]
        public IntSetting MaxTurnTime = new("Max Turn Time", 120, 60, 300, "Game Flow Settings");

        #endregion

        #region Hand configuration

        [Header("Hand Settings")]

        [Tooltip("Number of cards each player can draw at the beginning of the game.")]
        public IntSetting NumberOfCardsToStartWith = new("Number Of Cards To Start With", 2, 1, 5, "Hand Settings");

        [Tooltip("Number of cards each player draw at the end of their turn.")]
        public IntSetting NumberOfCardsToDrawAtEndOfTurn = new("Number Of Cards To Draw At End Of Turn", 3, 1, 5, "Hand Settings");

        [Tooltip("Toggle whether to enforce a maximum number of cards per player in hand.")]
        public BoolSetting IsMaxCardsPerPlayerInHandBased = new("Is Max Cards Per Player In Hand Based", false, "Hand Settings");

        [Tooltip("Maximum number of cards a player can hold in their hand.")]
        public IntSetting MaxCardsPerPlayerInHand = new("Max Cards Per Player In Hand", 10, 5, 25, "Hand Settings");

        #endregion

        #region Deck configuration

        [Header("Deck Settings")]

        [Tooltip("Number of cards drawn per turn.")]
        public IntSetting NumberOfCardsToDrawPerTurn = new("Number Of Cards To Draw Per Turn", 1, 1, 5, "Deck Settings");

        [Tooltip("Toggle whether to enforce a maximum number of cards per player.")]
        public BoolSetting IsMaxCardsPerPlayerInDeckBased = new("Is Max Cards Number Based", false, "Deck Settings");

        [Tooltip("Maximum number of cards a player can hold in their deck.")]
        public IntSetting MaxCardsNumberPerDeck = new("Max Cards Number", 20, 10, 25, "Deck Settings");

        #endregion

        #region Game rules configuration

        [Header("Game Rules Settings")]

        [Tooltip("Maximum amount of eloquence a player can have.")]
        public IntSetting MaxEloquence = new("Max Eloquence", 20, 1, 50, "Game Rules Settings");

        [Tooltip("The eloquence that players have at the beginning of the game.\n\n" +
         "• Player 1 starts with this value.\n" +
         "• Player 2 starts with this value + 1.\n" +
         "• Player 3 starts with this value + 2.\n" +
         "• Player 4 also starts with this value + 2 (same as Player 3).\n\n" +
         "Example:\n" +
         "If the value is 0:\n" +
         "  - Player 1: 0\n" +
         "  - Player 2: 1\n" +
         "  - Player 3: 2\n" +
         "  - Player 4: 2"
        )]
        public IntSetting EloquenceToStartWith = new("Eloquence To Start With", 0, 0, 50, "Game Rules Settings");

        [Tooltip("Amount of eloquence a player receives at the start of each turn.")]
        public IntSetting NumberOfEloquencesToStartTheTurnWith = new("Number Of Eloquences To Start The Turn With", 2, 1, 50, "Game Rules Settings");

        [Tooltip("Maximum number of souls needed to win.")]
        public IntSetting MaxSoulsToWin = new("Max Souls To Win", 100, 1, 100, "Game Rules Settings");

        [Tooltip("Initial number of souls each player starts with.")]
        public IntSetting SoulsToStartWith = new("Souls To Start With", 0, 0, 100, "Game Rules Settings");

        #endregion

        #region Cards configuration

        [Header("Cards Settings")]

        public IntSetting MaxCardsPartisanToPlay = new("Max Cards To Play", 3, 1, 3, "Cards Settings");

        public IntSetting MaxCardsToPlayPerTurn = new("Max Cards To Play Per Turn", 3, 1, 3, "Cards Settings");

        public BoolSetting IsPlayerHaveLimitedEquipment = new("Is Player Have Limited Equipment", false, "Cards Settings");

        public IntSetting MaxPlayerEquipementCards = new("Max Player Equipement Cards", 3, 1, 3, "Cards Settings");

        #endregion

        #region Sacrifice configuration

        [Header("Sacrifice Settings")]

        [Tooltip("Maximum number of followers a player can sacrifice per turn.")]
        public IntSetting MaxSacrificesPerTurn = new("Max Sacrifices Per Turn", 2, 1, 3, "Sacrifice Settings");

        [Tooltip("Sacrifice multiplicator")]
        public IntSetting SacrificeMultiplicator = new("Sacrifice Multiplicator", 1, 1, 3, "Sacrifice Settings");

        #endregion

        #region Market configuration

        [Header("Market Settings")]

        [Tooltip("Number of objects to replenish in the market at the end of a turn.")]
        public IntSetting MaxMarketCards = new("Max Market Cards", 5, 1, 5, "Market Settings");

        [Tooltip("Indicates whether the market deck reshuffles when exhausted.")]
        public BoolSetting AllowMarketReshuffle = new("Allow Market Reshuffle", false, "Market Settings");

        #endregion

        #region Courtyard configuration

        [Tooltip("Number of partisans to replenish in the courtyard at the end of a turn.")]
        public IntSetting MaxCourtyardCards = new("Max Courtyard Cards", 5, 1, 5, "Courtyard Settings");

        [Tooltip("Indicates whether the courtyard deck reshuffles when exhausted.")]
        public BoolSetting AllowCourtyardReshuffle = new("Allow Market Reshuffle", true, "Courtyard Settings");

        #endregion

        #region Advanced settings

        [Header("Advanced Settings")]

        [Tooltip("Enable debug mode for additional logs and testing features.")]
        public BoolSetting DebugMode = new("Debug Mode", false, "Advanced Settings");

        [Tooltip("Seed for random number generation (set to 0 for random seed).")]
        public BoolSetting RandomSeed = new("Random Seed", true, "Advanced Settings");

        #endregion
    }
}
