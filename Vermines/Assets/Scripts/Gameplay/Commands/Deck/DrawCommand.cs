using OMGG.DesignPattern;
using System.Linq;
using Fusion;

namespace Vermines.Gameplay.Commands.Deck {

    using Vermines.CardSystem.Elements;
    using Vermines.Player;

    public class DrawCommand : ICommand {

        private PlayerController _Player;

        private PlayerDeck? _OldDeck = null;

        public DrawCommand(PlayerController player)
        {
            _Player = player;
        }

        public CommandResponse Execute()
        {
            PlayerDeck deck = _Player.Deck;

            _OldDeck = deck.DeepCopy();

            ICard card = deck.Draw();

            if (card == null)
                return new CommandResponse(CommandStatus.Failure, $"Player {_Player.Object.InputAuthority} does not have any card left in his deck.");
            _Player.UpdateDeck(deck);

            return new CommandResponse(CommandStatus.Success, $"Player {_Player.Object.InputAuthority} drew a card.");
        }

        public void Undo()
        {
            if (_OldDeck == null)
                return;
            _Player.UpdateDeck((PlayerDeck)_OldDeck);
        }
    }
}
