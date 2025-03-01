using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands.Deck {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.Player;

    public class CardSacrifiedCommand : ICommand {

        private readonly PlayerRef _Player;
        private readonly int _CardId;

        private PlayerDeck? _OldDeck = null;

        public CardSacrifiedCommand(PlayerRef player, int cardID)
        {
            _Player = player;
            _CardId = cardID;
        }

        public CommandResponse Execute()
        {
            if (GameDataStorage.Instance.PlayerDeck.TryGetValue(_Player, out _) == false)
                return new CommandResponse(CommandStatus.Invalid, $"Player {_Player} does not have a deck.");
            PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[_Player];

            _OldDeck = deck.DeepCopy();

            if (CardSetDatabase.Instance.CardExist(_CardId) == false)
                return new CommandResponse(CommandStatus.Invalid, $"Card {_CardId} does not exist.");

            ICard card = CardSetDatabase.Instance.GetCardByID(_CardId);

            deck.Graveyard.Add(card);
            deck.PlayedCards.Remove(card);

            GameDataStorage.Instance.PlayerDeck[_Player] = deck;

            return new CommandResponse(CommandStatus.Success, $"Player {_Player} sacrified a card.");
        }

        public void Undo()
        {
            if (_OldDeck == null)
                return;
            PlayerDeck deck = (PlayerDeck)_OldDeck;

            if (GameDataStorage.Instance.PlayerDeck.TryGetValue(_Player, out _) == true)
            {
                GameDataStorage.Instance.PlayerDeck[_Player] = deck;

                return;
            }

            GameDataStorage.Instance.PlayerDeck.Add(_Player, deck);
        }
    }
}
