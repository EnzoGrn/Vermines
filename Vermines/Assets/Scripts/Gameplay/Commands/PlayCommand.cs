using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands {

    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.Player;

    public class ADMIN_CheckPlayCommand : ACommand {

        private readonly PlayerRef _Player;
        private readonly PhaseType _CurrentPhase;

        private readonly ICard _Card;
        private readonly int _CardId;

        public ADMIN_CheckPlayCommand(PlayerRef player, PhaseType currentPhase, int cardId)
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
                return new CommandResponse(CommandStatus.Invalid, "Table_Play_WrongPhase");

            // 1. Check if the card exist.
            if (_Card == null)
                return new CommandResponse(CommandStatus.CriticalError, "CardNotExist", _CardId.ToString());

            // 2. Check if the card is in the player hand.
            PlayerDeck playerDeck = GameDataStorage.Instance.PlayerDeck[_Player];

            if (!playerDeck.Hand.Contains(_Card))
                return new CommandResponse(CommandStatus.CriticalError, "Table_Play_CardNotInHand", _Card.Data.Name);
            return new CommandResponse(CommandStatus.Success, "", _Card.Data.Name);
        }
    }

    public class CLIENT_PlayCommand : ACommand {

        private readonly PlayerRef _Player;
        private readonly ICard     _Card;
        private readonly int       _CardId;

        public CLIENT_PlayCommand(PlayerRef player, int cardID)
        {
            _Player = player;
            _Card   = CardSetDatabase.Instance.GetCardByID(cardID);
            _CardId = cardID;
        }

        public override CommandResponse Execute()
        {
            PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[_Player];
            ICard      card = deck.PlayCard(_CardId);

            GameDataStorage.Instance.PlayerDeck[_Player] = deck;

            return new CommandResponse(CommandStatus.Success, "");
        }

        public override void Undo() {}
    }
}
