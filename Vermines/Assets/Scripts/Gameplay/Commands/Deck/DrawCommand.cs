using OMGG.DesignPattern;
using System.Linq;
using Fusion;

namespace Vermines.Gameplay.Commands.Deck {

    using Vermines.CardSystem.Elements;
    using Vermines.Player;

    public class DrawCommand : ICommand {

        private readonly PlayerRef _Player;

        private PlayerDeck? _OldDeck = null;

        public DrawCommand(PlayerRef player)
        {
            _Player = player;
        }

        public CommandResponse Execute()
        {
            if (GameDataStorage.Instance.PlayerDeck.TryGetValue(_Player, out _) == false)
                return new CommandResponse(CommandStatus.Invalid, $"Player {_Player} does not have a deck.");
            PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[_Player];

            _OldDeck = deck;

            ICard card = deck.Draw();

            if (card == null)
                return new CommandResponse(CommandStatus.Failure, $"Player {_Player} does not have any card left in his deck.");
            GameDataStorage.Instance.PlayerDeck[_Player] = deck;

            return new CommandResponse(CommandStatus.Success, $"Player {_Player} drew a card.");
        }

        public void Undo()
        {
            if (_OldDeck == null)
                return;
            PlayerDeck deck = (PlayerDeck)_OldDeck;

            if (GameDataStorage.Instance.PlayerDeck.TryGetValue(_Player, out _) == true) {
                GameDataStorage.Instance.PlayerDeck[_Player] = deck;

                return;
            }

            GameDataStorage.Instance.PlayerDeck.Add(_Player, deck);
        }
    }
}
