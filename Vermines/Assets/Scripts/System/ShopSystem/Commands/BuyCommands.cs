using OMGG.DesignPattern;
using Fusion;

namespace Vermines.ShopSystem.Commands {

    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.Player;

    public class BuyCommand : ICommand {

        private readonly PlayerRef _Player;
        private readonly ShopType  _ShopType;
        private readonly int       _CardID;

        public BuyCommand(PlayerRef player, ShopType shop, int cardID)
        {
            _Player   = player;
            _ShopType = shop;
            _CardID   = cardID;
        }

        public void Execute()
        {
            if (!GameDataStorage.Instance.PlayerDeck.TryGetValue(_Player, out _) || !CardSetDatabase.Instance.CardExist(_CardID))
                return;
            ShopData shop = GameDataStorage.Instance.Shop;

            if (!shop.HasCard(_ShopType, _CardID)) // Check if the shop has the card proposed
                return;
            if (CanPurchase()) {
                PlayerData playerData = GameDataStorage.Instance.PlayerData[_Player];
                PlayerDeck playerDeck = GameDataStorage.Instance.PlayerDeck[_Player];
                ICard            card = shop.BuyCard(_ShopType, _CardID);

                int newEloquence = playerData.Eloquence - card.Data.Eloquence;

                GameDataStorage.Instance.SetEloquence(_Player, newEloquence);

                playerDeck.Discard.Add(card);
            } else {
                // TODO: Log the clients that player can't by it.
            }
        }

        public void Undo()
        {
            // TODO: Implement the undo command
        }

        private bool CanPurchase()
        {
            int playerEloquence = GameDataStorage.Instance.PlayerData[_Player].Eloquence;
            int cardCost        = CardSetDatabase.Instance.GetCardByID(_CardID).Data.Eloquence;

            return playerEloquence >= cardCost;
        }
    }
}
