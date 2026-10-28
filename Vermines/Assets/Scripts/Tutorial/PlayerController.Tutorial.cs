using System.Collections.Generic;
using System.Linq;
using CelticEdge.Tutorial;

namespace Vermines.Player {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.Tutorial.Integration;
    using Vermines.Core.Player;

    public partial class PlayerController {

        /// <summary>
        /// Tutorial only (state authority):
        ///     fills what RPC_SendPlayerData would send for a real client, for a scripted opponent.
        /// </summary>
        public void SetupAsTutorialOpponent(string nickname, CardFamily family)
        {
            if (!HasStateAuthority)
                return;
            NetworkedUserID   = $"tutorial-opponent-{Object.InputAuthority.RawEncoded}";
            UserID            = NetworkedUserID.Value;
            NetworkedNickname = nickname;
            Nickname          = nickname;

            PlayerStatistics stats = Statistics;

            stats.PlayerRef   = Object.InputAuthority;
            stats.Family      = family;
            stats.IsConnected = true;

            UpdateStatistics(stats);
        }

        /// <summary>
        /// During a tutorial step that restricts actions, refuses the local player's requests the step doesn't allow.
        /// action = an ID of VerminesTutorialActions.
        /// 
        /// Always true outside the tutorial.
        /// </summary>
        private static bool TutorialAllows(string action, int cardId)
        {
            if (!TutorialMatch.IsActive)
                return true;
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            return TutorialInputGate.IsAllowed(action, TutorialCardIds.Key(card));
        }

        /// <summary>
        /// Tutorial only (state authority):
        ///     removes the first card of the deck with this tutorial ID and returns it, or null when the deck has none.
        /// </summary>
        public ICard TutorialTakeFromDeck(string cardId)
        {
            if (!HasStateAuthority)
                return null;
            List<ICard> deck = _DeckCache.ToList();
            ICard       card = deck.FirstOrDefault(c => TutorialCardIds.Is(c, cardId));

            if (card == null)
                return null;
            deck.Remove(card);

            WriteDeck(deck);

            return card;
        }

        /// <summary>
        /// Tutorial only (state authority):
        ///     the next card drawn will be this one.
        /// </summary>
        public void TutorialPutOnTopOfDeck(ICard card)
        {
            if (!HasStateAuthority || card == null)
                return;
            List<ICard> deck = _DeckCache.ToList();

            deck.Remove(card);
            deck.Insert(0, card);

            WriteDeck(deck);
        }
    }
}
