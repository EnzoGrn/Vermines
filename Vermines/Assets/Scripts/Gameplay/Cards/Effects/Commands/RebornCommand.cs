using OMGG.DesignPattern;

namespace Vermines.Gameplay.Commands {

    using Vermines.CardSystem.Elements;
    using Fusion;
    using Vermines.Player;

    public class RebornCommand : ACommand {

        private readonly PlayerRef _Player;
        private readonly ICard     _CardToReborn;

        public RebornCommand(PlayerRef player, ICard cardToReborn)
        {
            _Player       = player;
            _CardToReborn = cardToReborn;
        }

        public override CommandResponse Execute()
        {
            PlayerDeck playerDeck = GameDataStorage.Instance.PlayerDeck[_Player];

            if (!playerDeck.Graveyard.Contains(_CardToReborn))
                return new CommandResponse(CommandStatus.Invalid, $"Card {_CardToReborn.ID} does not exist in the graveyard.");
            playerDeck.Graveyard.Remove(_CardToReborn);
            playerDeck.PlayedCards.Add(_CardToReborn);

            return new CommandResponse(CommandStatus.Success, "");
        }
    }
}
