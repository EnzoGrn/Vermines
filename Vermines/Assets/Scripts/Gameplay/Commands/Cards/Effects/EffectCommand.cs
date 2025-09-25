using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;

    public class ADMIN_CheckEffectCommand : ACommand {

        private readonly PlayerRef _Player;

        private readonly ICard _Card;
        private readonly int _CardId;

        public ADMIN_CheckEffectCommand(PlayerRef player, int cardId)
        {
            _Player = player;
            _Card   = CardSetDatabase.Instance.GetCardByID(cardId);
            _CardId = cardId;
        }

        public override CommandResponse Execute()
        {
            // 0. Check if the card exist.
            if (_Card == null)
                return new CommandResponse(CommandStatus.CriticalError, "CardNotExist", _CardId.ToString());

            // 1. Check if the card is own by the player.
            if (_Card.Owner != PlayerRef.None && _Card.Owner != _Player)
                return new CommandResponse(CommandStatus.CriticalError, "CardWrongOwner", _Card.Data.Name);
            return new CommandResponse(CommandStatus.Success, "");
        }
    }
}
