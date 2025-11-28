using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.ShopSystem.Commands {

    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.Player;
    using Vermines.CardSystem.Data;
    using Vermines.Core.Player;

    /// <summary>
    /// This buy command is simulate on all clients but not on the authority object client.
    /// It simulate the result of the purchase that was already checked and processed by the authority object client.
    /// </summary>
    public class CLIENT_BuyCommand : ACommand {

        private PlayerController _Player;
        private ShopArgs _Args;

        public CLIENT_BuyCommand(PlayerController player, ShopArgs shopInfo)
        {
            _Player = player;
            _Args   = shopInfo;
        }

        public override CommandResponse Execute()
        {
            ICard card = _Args.Shop.BuyCard(_Args.ShopType, _Args.CardId);

            if (card.Data.Type == CardType.Equipment)
                _Player.Deck.Equipments.Add(card);
            else
                _Player.Deck.Discard.Add(card);
            card.Owner = _Player.Object.InputAuthority;

            return new CommandResponse(CommandStatus.Success, $"Player {_Player} bought the card {card.Data.Name}.");
        }

        public override void Undo() {}
    }

    /// <summary>
    /// This check command simulate only on authority object client.
    /// Check if we can execute a buy command, and simulate the network operation.
    /// </summary>
    public class ADMIN_CheckBuyCommand : ACommand {

        private PlayerController _Player;

        private readonly PhaseType _CurrentPhase;

        private ShopArgs _Parameters;

        public ADMIN_CheckBuyCommand(PlayerController player, PhaseType phase, ShopArgs parameters)
        {
            _Player       = player;
            _CurrentPhase = phase;
            _Parameters   = parameters;
        }

        public override CommandResponse Execute()
        {
            PlayerDeck playerDeck = _Player.Deck;

            // 0. Check if the shop exist
            if (_Parameters.Shop == null)
                return new CommandResponse(CommandStatus.CriticalError, "Shop_ShopNotExist", _Parameters.ShopType.ToString());

            // 1. Check if the shop and card is in shop
            if (!_Parameters.Shop.Sections.ContainsKey(_Parameters.ShopType))
                return new CommandResponse(CommandStatus.CriticalError, "Shop_ShopNotExist", _Parameters.ShopType.ToString());
            if (!_Parameters.Shop.HasCard(_Parameters.ShopType, _Parameters.CardId))
                return new CommandResponse(CommandStatus.CriticalError, "Shop_SlotEmpty", _Parameters.ShopType.ToString());

            // 2. Check phase
            if (_CurrentPhase == PhaseType.Gain && _Parameters.Shop.HasCard(_Parameters.ShopType, _Parameters.CardId)) {
                ICard freeCard = CardSetDatabase.Instance.GetCardByID(_Parameters.CardId);

                if (freeCard.Data.IsFree)
                    return new CommandResponse(CommandStatus.Success, string.Empty);
            } else if (_CurrentPhase != PhaseType.Action) {
                return new CommandResponse(CommandStatus.Invalid, "Shop_Buy_WrongPhase");
            }

            // 3. Get the wanted card
            ICard card = CardSetDatabase.Instance.GetCardByID(_Parameters.CardId);

            // 4. Check if player has enough eloquence
            int canPay = CanPurchase(_Player.Statistics, card);

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
        private int CanPurchase(PlayerStatistics playerData, ICard card)
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

        private PlayerController _Player;
        private ShopArgs _Parameters;

        public ADMIN_BuyCommand(PlayerController player, ShopArgs parameters)
        {
            _Player     = player;
            _Parameters = parameters;
        }

        public override CommandResponse Execute()
        {
            ICard card = _Parameters.Shop.BuyCard(_Parameters.ShopType, _Parameters.CardId);

            // 1. Process the purchase
            int remainingE = Purchase(_Player, _Player.Statistics, card);

            // 2. Move the card to the player's deck
            if (card.Data.Type == CardType.Equipment)
                _Player.Deck.Equipments.Add(card);
            else
                _Player.Deck.Discard.Add(card);

            // 3. Attribute the owner of the card
            card.Owner = _Player.Object.InputAuthority;

            return new CommandResponse(CommandStatus.Success, $"Player {_Player} bought the card {card.Data.Name}.", remainingE.ToString());
        }

        public override void Undo() {}

        /// <summary>
        /// Process the purchase and return the new eloquence of the player.
        /// </summary>
        private int Purchase(PlayerController player, PlayerStatistics stats, ICard card)
        {
            if (card.Data.IsFree)
                return stats.Eloquence;
            int newEloquence = stats.Eloquence - card.Data.CurrentEloquence;

            player.SetEloquence(newEloquence);

            return newEloquence;
        }
    }
}
