using OMGG.DesignPattern;
using Fusion;

namespace Vermines.ShopSystem.Commands {

    using Vermines.Gameplay.Phases.Enumerations;

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;

    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem.Data;

    using Vermines.Player;

    public struct BuyParameters {

        /// <summary>
        /// The player reference of the buyer.
        /// </summary>
        public PlayerRef Player;

        /// <summary>
        /// The shop.
        /// </summary>
        public ShopData Shop;

        /// <summary>
        /// The shop section where he bought the card.
        /// </summary>
        public ShopType ShopType;

        /// <summary>
        /// The slot of the shop he bought.
        /// </summary>
        public int Slot;
    }

    /// <summary>
    /// This buy command is simulate on all clients but not on the authority object client.
    /// It simulate the result of the purchase that was already checked and processed by the authority object client.
    /// </summary>
    public class CLIENT_BuyCommand : ACommand {

        private BuyParameters _Parameters;

        public CLIENT_BuyCommand(BuyParameters parameters)
        {
            _Parameters   = parameters;
        }

        public override CommandResponse Execute()
        {
            PlayerDeck playerDeck = GameDataStorage.Instance.PlayerDeck[_Parameters.Player];
            ICard      card       = _Parameters.Shop.BuyCardAtSlot(_Parameters.ShopType, _Parameters.Slot);

            if (card.Data.Type == CardType.Equipment)
                playerDeck.Equipments.Add(card);
            else
                playerDeck.Discard.Add(card);
            card.Owner = _Parameters.Player;

            return new CommandResponse(CommandStatus.Success, $"Player {_Parameters.Player} bought the card {card.Data.Name}.");
        }

        public override void Undo() {}
    }

    /// <summary>
    /// This check command simulate only on authority object client.
    /// Check if we can execute a buy command, and simulate the network operation.
    /// </summary>
    public class ADMIN_CheckBuyCommand : ACommand {

        private readonly PhaseType _CurrentPhase;

        private BuyParameters _Parameters;

        public ADMIN_CheckBuyCommand(PhaseType phase, BuyParameters parameters)
        {
            _CurrentPhase = phase;
            _Parameters   = parameters;
        }

        public override CommandResponse Execute()
        {
            PlayerDeck playerDeck = GameDataStorage.Instance.PlayerDeck[_Parameters.Player];

            // 0. Check if the shop exist
            if (_Parameters.Shop == null)
                return new CommandResponse(CommandStatus.CriticalError, "Shop_Buy_ShopNotExist", _Parameters.ShopType.ToString());

            // 1. Check phase
            if (_CurrentPhase == PhaseType.Gain && _Parameters.Shop.HasCardAtSlot(_Parameters.ShopType, _Parameters.Slot)) {
                ICard freeCard = _Parameters.Shop.Sections[_Parameters.ShopType].GetCardAtSlot(_Parameters.Slot);

                if (freeCard.Data.IsFree)
                    return new CommandResponse(CommandStatus.Success, string.Empty);
            } else if (_CurrentPhase != PhaseType.Action) {
                return new CommandResponse(CommandStatus.Invalid, "Shop_Buy_WrongPhase");
            }

            // 2. Check if the shop and slot exist
            if (!_Parameters.Shop.Sections.ContainsKey(_Parameters.ShopType))
                return new CommandResponse(CommandStatus.CriticalError, "Shop_Buy_ShopNotExist", _Parameters.ShopType.ToString());
            if (!_Parameters.Shop.HasCardAtSlot(_Parameters.ShopType, _Parameters.Slot))
                return new CommandResponse(CommandStatus.CriticalError, "Shop_Buy_SlotEmpty", _Parameters.ShopType.ToString());

            // 3. Get the wanted card
            ICard card = _Parameters.Shop.Sections[_Parameters.ShopType].GetCardAtSlot(_Parameters.Slot);

            // 4. Check if player has enough eloquence
            int canPay = CanPurchase(GameDataStorage.Instance.PlayerData[_Parameters.Player], card);

            if (canPay < 0)
                return new CommandResponse(CommandStatus.Failure, "Shop_Buy_NotEnoughEloquence", card.Data.Name, (-canPay).ToString());

            // 5. Check if it's an equipment and if he already have it.
            if (playerDeck.Equipments.Count > 0 && card.Data.Type == CardType.Equipment) {
                ICard found = playerDeck.Equipments.Find(c => c.Data.Name == card.Data.Name);

                if (found != null)
                    return new CommandResponse(CommandStatus.Failure, "Shop_Buy_AlreadyHasThisEquipment", card.Data.Name);
            }

            return new CommandResponse(CommandStatus.Success, string.Empty);
        }

        /// <summary>
        /// Return the difference between player eloquence and card cost.
        /// </summary>
        private int CanPurchase(PlayerData playerData, ICard card)
        {
            int playerEloquence = playerData.Eloquence;
            int cardCost        = card.Data.CurrentEloquence;

            if (card.Data.IsFree)
                return playerData.Eloquence;
            return playerEloquence - cardCost;
        }
    }

    /// <summary>
    /// This buy command simulate only on authority object client.
    /// Process it only after doing the check command.
    /// It process the purchase of a card from the shop to the player's deck.
    /// </summary>
    public class ADMIN_BuyCommand : ACommand {

        private BuyParameters _Parameters;

        public ADMIN_BuyCommand(BuyParameters parameters)
        {
            _Parameters   = parameters;
        }

        public override CommandResponse Execute()
        {
            PlayerData playerData = GameDataStorage.Instance.PlayerData[_Parameters.Player];
            PlayerDeck playerDeck = GameDataStorage.Instance.PlayerDeck[_Parameters.Player];
            ICard      card       = _Parameters.Shop.Sections[_Parameters.ShopType].GetCardAtSlot(_Parameters.Slot);

            // 1. Process the purchase
            int remainingE = Purchase(_Parameters.Player, playerData, card);

            // 2. Move the card to the player's deck
            if (card.Data.Type == CardType.Equipment)
                playerDeck.Equipments.Add(card);
            else
                playerDeck.Discard.Add(card);

            // 3. Attribute the owner of the card
            card.Owner = _Parameters.Player;

            return new CommandResponse(CommandStatus.Success, $"Player {_Parameters.Player} bought the card {card.Data.Name}.", remainingE.ToString());
        }

        public override void Undo() {}

        /// <summary>
        /// Process the purchase and return the new eloquence of the player.
        /// </summary>
        private int Purchase(PlayerRef player, PlayerData playerData, ICard card)
        {
            if (card.Data.IsFree)
                return playerData.Eloquence;
            int newEloquence = playerData.Eloquence - card.Data.CurrentEloquence;

            GameDataStorage.Instance.SetEloquence(player, newEloquence);

            return newEloquence;
        }
    }
}
