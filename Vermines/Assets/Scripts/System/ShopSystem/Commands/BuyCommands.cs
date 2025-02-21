using System.Collections.Generic;
using OMGG.DesignPattern;
using Fusion;
using UnityEngine;

namespace Vermines.ShopSystem.Commands {

    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.Player;

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

            if (_OriginalShop != null)
            {
                _OldShop = parameters.Shop.DeepCopy();
            }

            if (parameters.Decks.TryGetValue(parameters.Player, out PlayerDeck playerDeck))
                _OldPlayerDeck = playerDeck.DeepCopy();
        }

        public bool Execute()
        {
            if (_OriginalShop != null)
            {
                _OldShop = _Parameters.Shop.DeepCopy();
            }

            if (!_Parameters.Decks.TryGetValue(_Parameters.Player, out PlayerDeck playerDeck))
                return false;
            _OldPlayerDeck = playerDeck.DeepCopy();

            if (!_Parameters.Shop.Sections.ContainsKey(_Parameters.ShopType) || !_Parameters.Shop.Sections[_Parameters.ShopType].AvailableCards.ContainsKey(_Parameters.Slot))
                return false;
            ICard card = _Parameters.Shop.BuyCardAtSlot(_Parameters.ShopType, _Parameters.Slot);

            playerDeck.Discard.Add(card);

            return true;
        }

        public void Undo()
        {
            if (_OriginalShop != null)
            {
                _OriginalShop.Sections = _OldShop.Sections;
            }

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

        public bool Execute()
        {
            if (!_Parameters.Decks.TryGetValue(_Parameters.Player, out PlayerDeck playerDeck))
                return false;
            if (!_Parameters.Shop.Sections.ContainsKey(_Parameters.ShopType) || !_Parameters.Shop.Sections[_Parameters.ShopType].AvailableCards.ContainsKey(_Parameters.Slot))
                return false;
            if (!_Parameters.Shop.HasCardAtSlot(_Parameters.ShopType, _Parameters.Slot))
                return false;
            ICard card = _Parameters.Shop.Sections[_Parameters.ShopType].AvailableCards[_Parameters.Slot];

            bool res = CanPurchase(GameDataStorage.Instance.PlayerData[_Parameters.Player], card);

            if (res)
                Purchase(GameDataStorage.Instance.PlayerData[_Parameters.Player], card);
            return res;
        }

        public void Undo() {}

        private bool CanPurchase(Vermines.Player.PlayerData playerData, ICard card)
        {
            int playerEloquence = GameDataStorage.Instance.PlayerData[_Parameters.Player].Eloquence;
            int cardCost        = card.Data.Eloquence;

            return playerEloquence >= cardCost;
        }

        private void Purchase(Vermines.Player.PlayerData playerData, ICard card)
        {
            int newEloquence = playerData.Eloquence - card.Data.Eloquence;

            GameDataStorage.Instance.SetEloquence(playerData.PlayerRef, newEloquence);
        }
    }
}
