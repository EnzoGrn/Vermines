using OMGG.DesignPattern;

namespace Vermines.Gameplay.Commands {

    using Fusion;
    using System.Linq;
    using Vermines.CardSystem.Elements;
    using Vermines.Player;

    public class RebornCommand : ACommand {

        private PlayerController _Player;

        private readonly ICard     _CardToReborn;

        public RebornCommand(PlayerController player, ICard cardToReborn)
        {
            _Player       = player;
            _CardToReborn = cardToReborn;
        }

        public override CommandResponse Execute()
        {
            if (!_Player.Graveyard.Contains(_CardToReborn))
                return new CommandResponse(CommandStatus.Invalid, $"Card {_CardToReborn.ID} does not exist in the graveyard.");

            _Player.RemoveCardFromGraveyard(_CardToReborn);
            _Player.AddCardToPlayedCards(_CardToReborn);

            return new CommandResponse(CommandStatus.Success, "");
        }
    }
}
