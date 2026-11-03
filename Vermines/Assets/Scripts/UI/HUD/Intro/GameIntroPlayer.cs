namespace Vermines.UI {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Characters;

    /// <summary>
    /// What the game intro shows about one player: nickname, insect family (cultist) and god.
    /// </summary>
    /// <remarks>
    /// Kept apart from <c>PlayerController</c> so the intro can be played without network (playground, tests).
    /// </remarks>
    public struct GameIntroPlayer {

        /// <summary>
        /// Nickname of the player.
        /// </summary>
        public string Nickname;

        /// <summary>
        /// Insect family of the player, it gives the cultist and the colors.
        /// </summary>
        public CardFamily Family;

        /// <summary>
        /// God drawn by the player (can be null while unknown).
        /// </summary>
        public God God;

        /// <summary>
        /// True for the player of this client.
        /// </summary>
        public bool IsLocal;

        /// <summary>
        /// True for the player who plays the first turn.
        /// </summary>
        public bool IsFirst;

        public GameIntroPlayer(string nickname, CardFamily family, God god, bool isLocal = false, bool isFirst = false)
        {
            Nickname = nickname;
            Family   = family;
            God      = god;
            IsLocal  = isLocal;
            IsFirst  = isFirst;
        }
    }
}
