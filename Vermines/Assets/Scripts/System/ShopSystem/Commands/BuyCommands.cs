using System.Collections.Generic;
using OMGG.DesignPattern;
using Fusion;
using UnityEngine;

namespace Vermines.ShopSystem.Commands {

    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.Player;
    using System.Security.Cryptography;

    public struct BuyParameters {

        /// <summary>
        /// The deck data of every player in the game.
        /// </summary>
        public Dictionary<PlayerRef, PlayerDeck> Decks;

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
    /// This buy command simulate in other client the buy command.
    /// So it's not a network command but only a logic simulation.
    /// The real network command is on the player that did the action.
    /// </summary>
    public class BuyCommand : ICommand {

        private BuyParameters _Parameters;

        private ShopData _OriginalShop;

        private ShopData    _OldShop;
        private PlayerDeck? _OldPlayerDeck = null;

        public BuyCommand(BuyParameters parameters)
        {
            _Parameters   = parameters;
            _OriginalShop = parameters.Shop;
            _OldShop      = parameters.Shop?.DeepCopy() ?? null;

            if (parameters.Decks.TryGetValue(parameters.Player, out PlayerDeck playerDeck))
                _OldPlayerDeck = playerDeck.DeepCopy();
        }

        public CommandResponse Execute()
        {
            _OldShop = _OriginalShop?.DeepCopy() ?? null;

            if (!_Parameters.Decks.TryGetValue(_Parameters.Player, out PlayerDeck playerDeck))
                return new CommandResponse(CommandStatus.Invalid, $"Player {_Parameters.Player} does not have a deck.");
            _OldPlayerDeck = playerDeck.DeepCopy();

            if (!_Parameters.Shop.Sections.ContainsKey(_Parameters.ShopType) || !_Parameters.Shop.Sections[_Parameters.ShopType].AvailableCards.ContainsKey(_Parameters.Slot))
                return new CommandResponse(CommandStatus.Invalid, $"Shop {_Parameters.ShopType} and slot {_Parameters.Slot} does not exist.");
            ICard card = _Parameters.Shop.BuyCardAtSlot(_Parameters.ShopType, _Parameters.Slot);

            if (card == null)
                return new CommandResponse(CommandStatus.Failure, $"Shop {_Parameters.ShopType} have slot {_Parameters.Slot} empty.");
            playerDeck.Discard.Add(card);

            return new CommandResponse(CommandStatus.Success, $"Player {_Parameters.Player} bought the card.");
        }

        public void Undo()
        {
            if (_OriginalShop != null)
                _OriginalShop.Sections = _OldShop.Sections;
            if (_OldPlayerDeck != null) {
                PlayerDeck old = (PlayerDeck)_OldPlayerDeck;

                if (_Parameters.Decks.TryGetValue(_Parameters.Player, out PlayerDeck playerDeck)) {
                    playerDeck = old;

                    _Parameters.Decks[_Parameters.Player] = playerDeck;
                }
            }
        }
    }

    /// <summary>
    /// This check command simulate only on authority object client.
    /// Check if we can execute a buy command, and simulate the network operation.
    /// </summary>
    /// <warning>
    /// Don't use it has a checker only, because it does operation like money management.
    /// </warning>
    public class CheckBuyCommand : ICommand {

        private BuyParameters _Parameters;

        public CheckBuyCommand(BuyParameters parameters)
        {
            _Parameters = parameters;
        }

        public CommandResponse Execute()
        {
            if (!_Parameters.Decks.TryGetValue(_Parameters.Player, out PlayerDeck playerDeck))
                return new CommandResponse(CommandStatus.Invalid, $"Player {_Parameters.Player} does not have a deck.");
            if (!_Parameters.Shop.Sections.ContainsKey(_Parameters.ShopType) || !_Parameters.Shop.Sections[_Parameters.ShopType].AvailableCards.ContainsKey(_Parameters.Slot))
                return new CommandResponse(CommandStatus.Invalid, $"Shop {_Parameters.ShopType} and slot {_Parameters.Slot} does not exist.");
            if (!_Parameters.Shop.HasCardAtSlot(_Parameters.ShopType, _Parameters.Slot))
                return new CommandResponse(CommandStatus.Invalid, $"Shop {_Parameters.ShopType} have slot {_Parameters.Slot} empty.");
            ICard card = _Parameters.Shop.Sections[_Parameters.ShopType].AvailableCards[_Parameters.Slot];

            bool res = CanPurchase(GameDataStorage.Instance.PlayerData[_Parameters.Player], card);

            if (!res)
                return new CommandResponse(CommandStatus.Failure, $"Player {_Parameters.Player} does not have enough eloquence to buy the card.");
            Purchase(GameDataStorage.Instance.PlayerData[_Parameters.Player], card);

            return new CommandResponse(CommandStatus.Success, $"Player {_Parameters.Player} can buy the card.");
        }

        public void Undo() {}

        private bool CanPurchase(PlayerData playerData, ICard card)
        {
            int playerEloquence = GameDataStorage.Instance.PlayerData[_Parameters.Player].Eloquence;
            int cardCost        = card.Data.Eloquence;

            return playerEloquence >= cardCost;
        }

        private void Purchase(PlayerData playerData, ICard card)
        {
            int newEloquence = playerData.Eloquence - card.Data.CurrentEloquence;

            GameDataStorage.Instance.SetEloquence(playerData.PlayerRef, newEloquence);
        }
    }
}
