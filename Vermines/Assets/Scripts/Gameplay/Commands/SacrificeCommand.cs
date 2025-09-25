using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.Player;

    public class ADMIN_SacrificeCommand : ACommand {

        private readonly PlayerRef _Player;

        private readonly ICard _Card;
        private readonly int   _CardId;

        public ADMIN_SacrificeCommand(PlayerRef player, int cardId)
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

            // 1. Check if the card is a Partisan.
            if (_Card.Data.Type != CardType.Partisan)
                return new CommandResponse(CommandStatus.Invalid, "Sacrifice_WrongCardType");

            // 2. Check if the card is in the player played cards.
            PlayerDeck playerDeck = GameDataStorage.Instance.PlayerDeck[_Player];

            if (!playerDeck.PlayedCards.Contains(_Card))
                return new CommandResponse(CommandStatus.CriticalError, "Sacrifice_CardNotInTable", _Card.Data.Name);
            return new CommandResponse(CommandStatus.Success, "", _Card.Data.Name);
        }
    }

    public class CLIENT_CardSacrifiedCommand : ACommand {

        private readonly PlayerRef _Player;

        private readonly ICard _Card;
        private readonly int _CardId;

        public CLIENT_CardSacrifiedCommand(PlayerRef player, int cardID)
        {
            _Player = player;
            _Card   = CardSetDatabase.Instance.GetCardByID(cardID);
            _CardId = cardID;
        }

        public override CommandResponse Execute()
        {
            PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[_Player];

            deck.Graveyard.Add(_Card);
            deck.PlayedCards.Remove(_Card);

            GameDataStorage.Instance.PlayerDeck[_Player] = deck;

            return new CommandResponse(CommandStatus.Success, "");
        }

        public override void Undo() {}
    }
}
