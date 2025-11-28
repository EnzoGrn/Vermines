using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands {

    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.Player;

    public class ADMIN_CheckDiscardCommand : ACommand {

        private PlayerController _Player;
        private readonly PhaseType _CurrentPhase;

        private readonly ICard _Card;
        private readonly int _CardId;

        public ADMIN_CheckDiscardCommand(PlayerController player, PhaseType currentPhase, int cardId)
        {
            _Player       = player;
            _CurrentPhase = currentPhase;
            _Card         = CardSetDatabase.Instance.GetCardByID(cardId);
            _CardId       = cardId;
        }

        public override CommandResponse Execute()
        {
            // 0. Check if we are in the action phase.
            if (_CurrentPhase != PhaseType.Action)
                return new CommandResponse(CommandStatus.Invalid, "Table_Discard_WrongPhase", _CardId.ToString());

            // 1. Check if the card exist.
            if (_Card == null)
                return new CommandResponse(CommandStatus.CriticalError, "CardNotExist", _CardId.ToString());

            // 2. Check if the card is in the player hand.
            if (!_Player.Deck.Hand.Contains(_Card))
                return new CommandResponse(CommandStatus.CriticalError, "Table_Discard_CardNotInHand", _CardId.ToString(), _Card.Data.Name);
            return new CommandResponse(CommandStatus.Success, "", _Card.Data.Name);
        }
    }

    public class CLIENT_DiscardCommand : ACommand {

        private PlayerController _Player;

        private readonly ICard _Card;
        private readonly int _CardId;

        public CLIENT_DiscardCommand(PlayerController player, int cardID)
        {
            _Player = player;
            _Card   = CardSetDatabase.Instance.GetCardByID(cardID);
            _CardId = cardID;
        }

        public override CommandResponse Execute()
        {
            PlayerDeck deck = _Player.Deck;
            ICard      card = deck.DiscardCard(_CardId);

            _Player.UpdateDeck(deck);

            return new CommandResponse(CommandStatus.Success, "");
        }

        public override void Undo() {}
    }
}
