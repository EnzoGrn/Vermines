using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands.Deck {

    using Vermines.CardSystem.Elements;
    using Vermines.Player;

    public class CardPlayedCommand : ICommand {

        private readonly PlayerRef _Player;
        private readonly int _CardId;

        private PlayerDeck? _OldDeck = null;

        public CardPlayedCommand(PlayerRef player, int cardID)
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

            ICard card = deck.PlayCard(_CardId);

            if (card == null)
                return new CommandResponse(CommandStatus.Invalid, $"Player {_Player} tried to play a card that he does not have.");
            GameDataStorage.Instance.PlayerDeck[_Player] = deck;
 
            return new CommandResponse(CommandStatus.Success, $"Player {_Player} played the card {_CardId}.");
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
