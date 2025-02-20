using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands.Deck {
    using System.Linq;
    using Vermines.Player;

    public class DiscardCommand : ICommand {

        private readonly PlayerRef _Player;
        private readonly int _CardId;

        private PlayerDeck? _OldDeck = null;

        public DiscardCommand(PlayerRef player, int cardID)
        {
            _Player = player;
            _CardId = cardID;
        }

        public bool Execute()
        {
            if (GameDataStorage.Instance.PlayerDeck.TryGetValue(_Player, out _) == false)
                return false;
            PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[_Player];

            _OldDeck = deck.DeepCopy();

            deck.DiscardCard(_CardId);

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
