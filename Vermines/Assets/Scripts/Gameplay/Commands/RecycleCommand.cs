using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands {

    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.Player;
    using Vermines.CardSystem.Enumerations;
    using Vermines.ShopSystem.Data;

    public class ADMIN_CheckRecycleCommand : ACommand {

        private readonly PlayerRef _Player;
        private readonly PhaseType _CurrentPhase;

        private readonly ICard _Card;
        private readonly int _CardId;

        public ADMIN_CheckRecycleCommand(PlayerRef player, PhaseType currentPhase, int cardId)
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
                return new CommandResponse(CommandStatus.Invalid, "Recycle_WrongPhase", _CardId.ToString());

            // 1. Check if the card exist.
            if (_Card == null)
                return new CommandResponse(CommandStatus.CriticalError, "CardNotExist", _CardId.ToString());

            // 2. Check if the card is a Tool card.
            if (_Card.Data.Type != CardType.Tools)
                return new CommandResponse(CommandStatus.CriticalError, "Recycle_WrongCardType", _CardId.ToString());

            // 3. Check if the card is in the player hand.
            PlayerDeck playerDeck = GameDataStorage.Instance.PlayerDeck[_Player];

            if (!playerDeck.Hand.Contains(_Card))
                return new CommandResponse(CommandStatus.CriticalError, "Recycle_CardNotInHand", _CardId.ToString(), _Card.Data.Name);
            return new CommandResponse(CommandStatus.Success, "", _Card.Data.Name);
        }
    }

    public class CLIENT_CardRecycleCommand : ACommand
    {

        private readonly PlayerRef _Player;

        private readonly ICard _Card;
        private readonly int   _CardId;

        private ShopSectionBase _Shop;

        public CLIENT_CardRecycleCommand(PlayerRef player, int cardID, ShopSectionBase section)
        {
            _Player = player;
            _Card   = CardSetDatabase.Instance.GetCardByID(cardID);
            _CardId = cardID;
            _Shop   = section;
        }

        public override CommandResponse Execute()
        {
            PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[_Player];

            deck.Hand.Remove(_Card);

            GameDataStorage.Instance.PlayerDeck[_Player] = deck;

            _Shop.ReturnCard(_Card);

            PlayerData playerData = GameDataStorage.Instance.PlayerData[_Player];

            GameDataStorage.Instance.SetEloquence(_Player, playerData.Eloquence + _Card.Data.RecycleEloquence);

            return new CommandResponse(CommandStatus.Success, "");
        }

        public override void Undo() {}
    }
}
