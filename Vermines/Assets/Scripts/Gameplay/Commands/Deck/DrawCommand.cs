using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands.Deck {
    using System.Linq;
    using Vermines.Player;

    public class DrawCommand : ICommand {

        private readonly PlayerRef _Player;

        private PlayerDeck? _OldDeck = null;

        public DrawCommand(PlayerRef player)
        {
            _Player = player;
        }

        public bool Execute()
        {
            if (GameDataStorage.Instance.PlayerDeck.TryGetValue(_Player, out _) == false)
                return false;
            PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[_Player];

            _OldDeck = deck;

            deck.Draw();

            if (PlayerController.Local.PlayerRef == _Player)
            {
                GameEvents.InvokeOnDrawCard(deck.Hand.Last());
            }

            GameDataStorage.Instance.PlayerDeck[_Player] = deck;
            return true;
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
