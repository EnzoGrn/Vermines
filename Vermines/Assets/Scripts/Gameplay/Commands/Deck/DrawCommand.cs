using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands.Deck {

    using Vermines.Player;

    public class DrawCommand : ICommand {

        private readonly PlayerRef _Player;

        private PlayerDeck? _OldDeck = null;

        public DrawCommand(PlayerRef player)
        {
            _Player = player;
        }

        public void Execute()
        {
            if (GameDataStorage.Instance.PlayerDeck.TryGetValue(_Player, out _) == false)
                return;
            PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[_Player];

            _OldDeck = deck;

            deck.Draw();
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
