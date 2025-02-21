using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands.Deck {
    using System.Linq;
    using UnityEngine;
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

            bool resDraw = deck.Draw();

            Debug.LogWarning($"[SERVER]: {_Player}, draw card is {resDraw}. Is {_Player} the same as the executor {PlayerController.Local.PlayerRef}");

            // TODO: Check why it is working on every clients
            if (resDraw)
            {
                GameDataStorage.Instance.PlayerDeck[_Player] = deck;

                if (PlayerController.Local.PlayerRef == _Player)
                    GameEvents.InvokeOnDrawCard(deck.Hand.Last());
            }
            else
            {
                return false;
            }

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
