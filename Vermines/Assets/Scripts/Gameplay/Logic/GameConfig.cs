using UnityEngine;

namespace Vermines.Config {

    [CreateAssetMenu(fileName = "New Game Config", menuName = "Vermines/Game/Create a new game config")]
    public class GameConfig : ScriptableObject {

        #region Game Seed

        private int _Seed = 0;
        public int Seed
        {
            get => _Seed;
            set
            {
                if (value != _Seed) {
                    _Seed = value;
                    Rand = new System.Random(value);
                }
            }
        }

        public System.Random Rand = new(0);

        #endregion

        [Header("Limits")]
        [Tooltip("The number of souls needed to win a game.")]
        public int WinCondition = 100;

        [Tooltip("The maximum of eloquence that a player can have (every eloquence above this value will be lost).")]
        public int MaxEloquence = 20;

        [Header("First turn")]
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
        public int FirstEloquence = 0;

        [Tooltip("The number of cards that players draw at the beginning of the game.")]
        public int FirstDraw = 3;

        [Header("Turn")]

        [Tooltip("The eloquence that players gain at the beginning of their turn.")]
        public int EloquencePerTurn = 2;

        [Tooltip("The number of cards that players draw at the end of their turn.")]
        public int DrawPerTurn = 3;
    }
}
