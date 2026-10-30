using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands.Deck
{

    using Vermines.CardSystem.Elements;
    using Vermines.Player;

    public class DrawCommand : ICommand
    {

        private PlayerController _Player;

        public DrawCommand(PlayerController player)
        {
            _Player = player;
        }

        public CommandResponse Execute()
        {
            ICard card = _Player.DrawOneCard();

            if (card == null)
                return new CommandResponse(CommandStatus.Failure, $"Player {_Player.Object.InputAuthority} does not have any card left in his deck.");

            _Player.DrawCardToHand(card);

            return new CommandResponse(CommandStatus.Success, $"Player {_Player.Object.InputAuthority} drew a card.");
        }

        public void Undo() { }

        // Note : Undo n'est plus fonctionnel (nécessiterait de remettre la
        // carte en tête de Deck ET de la retirer de Hand). CommandInvoker.
        // UndoCommand() n'est appelé nulle part dans le projet (vérifié) --
        // sans conséquence en l'état.
    }
}
