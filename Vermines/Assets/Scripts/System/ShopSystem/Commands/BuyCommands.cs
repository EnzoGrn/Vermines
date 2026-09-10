using Fusion;
using OMGG.DesignPattern;
using UnityEngine;

namespace Vermines.ShopSystem.Commands
{

    using System.Linq;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Core.Player;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;

    public class CLIENT_BuyCommand : ACommand
    {

        private PlayerController _Player;
        private ShopArgs _Args;

        public CLIENT_BuyCommand(PlayerController player, ShopArgs shopInfo)
        {
            _Player = player;
            _Args = shopInfo;
        }

        public override CommandResponse Execute()
        {
            ICard card = _Args.Shop.BuyCard(_Args.ShopType, _Args.CardId);

            if (card.Data.Type == CardType.Equipment)
                _Player.AddEquipment(card);
            else
                _Player.AddCardToDiscard(card);
            card.Owner = _Player.Object.InputAuthority;

            return new CommandResponse(CommandStatus.Success, $"Player {_Player} bought the card {card.Data.Name}.");
        }

        public override void Undo() { }
    }

    public class ADMIN_CheckBuyCommand : ACommand
    {

        private PlayerController _Player;

        private readonly PhaseType _CurrentPhase;

        private ShopArgs _Parameters;

        public ADMIN_CheckBuyCommand(PlayerController player, PhaseType phase, ShopArgs parameters)
        {
            _Player = player;
            _CurrentPhase = phase;
            _Parameters = parameters;
        }

        public override CommandResponse Execute()
        {
            // 0. Check if the shop exist
            if (_Parameters.Shop == null)
                return new CommandResponse(CommandStatus.CriticalError, "Shop_ShopNotExist", _Parameters.ShopType.ToString());

            // 1. Check if the shop and card is in shop
            if (!_Parameters.Shop.Sections.ContainsKey(_Parameters.ShopType))
                return new CommandResponse(CommandStatus.CriticalError, "Shop_ShopNotExist", _Parameters.ShopType.ToString());
            if (!_Parameters.Shop.HasCard(_Parameters.ShopType, _Parameters.CardId))
                return new CommandResponse(CommandStatus.CriticalError, "Shop_SlotEmpty", _Parameters.ShopType.ToString());

            // 2. Check phase
            if (_CurrentPhase == PhaseType.Gain && _Parameters.Shop.HasCard(_Parameters.ShopType, _Parameters.CardId))
            {
                ICard freeCard = CardSetDatabase.Instance.GetCardByID(_Parameters.CardId);

                if (freeCard.Data.IsFree)
                    return new CommandResponse(CommandStatus.Success, string.Empty);
            }
            else if (_CurrentPhase != PhaseType.Action)
            {
                return new CommandResponse(CommandStatus.Invalid, "Shop_Buy_WrongPhase");
            }

            // 3. Get the wanted card
            ICard card = CardSetDatabase.Instance.GetCardByID(_Parameters.CardId);

            // 4. Check if player has enough eloquence
            int canPay = CanPurchase(_Player.Statistics, card);

            if (canPay < 0)
                return new CommandResponse(CommandStatus.Failure, "Shop_Buy_NotEnoughEloquence", card.Data.Name, (-canPay).ToString());

            // 5. Check if it's an equipment and if he already have it.
            if (_Player.Equipments.Count > 0 && card.Data.Type == CardType.Equipment)
            {
                ICard found = _Player.Equipments.FirstOrDefault(c => c.Data.Name == card.Data.Name);

                if (found != null)
                    return new CommandResponse(CommandStatus.Failure, "Shop_Buy_AlreadyHasThisEquipment", card.Data.Name);
            }

            return new CommandResponse(CommandStatus.Success, string.Empty);
        }

        private int CanPurchase(PlayerStatistics playerData, ICard card)
        {
            int playerEloquence = playerData.Eloquence;
            int cardCost = card.Data.CurrentEloquence;

            if (card.Data.IsFree)
                return playerData.Eloquence;
            return playerEloquence - cardCost;
        }
    }

    public class ADMIN_BuyCommand : ACommand
    {

        private PlayerController _Player;
        private ShopArgs _Parameters;

        public ADMIN_BuyCommand(PlayerController player, ShopArgs parameters)
        {
            _Player = player;
            _Parameters = parameters;
        }

        public override CommandResponse Execute()
        {
            ICard card = _Parameters.Shop.BuyCard(_Parameters.ShopType, _Parameters.CardId);

            int remainingE = Purchase(_Player, _Player.Statistics, card);

            if (card.Data.Type == CardType.Equipment)
                _Player.AddEquipment(card);
            else
                _Player.AddCardToDiscard(card);

            card.Owner = _Player.Object.InputAuthority;

            return new CommandResponse(CommandStatus.Success, $"Player {_Player} bought the card {card.Data.Name}.", remainingE.ToString());
        }

        public override void Undo() { }

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
