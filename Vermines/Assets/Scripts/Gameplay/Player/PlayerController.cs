using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Player {

    using Vermines.Network.Utilities;
    using Vermines.ShopSystem.Commands;
    using Vermines.ShopSystem.Enumerations;

    public class PlayerController : NetworkBehaviour {

        public static PlayerController Local { get; private set; }

        public PlayerRef PlayerRef => Object.InputAuthority;

        #region Override Methods

        public override void Spawned()
        {
            name = NetworkNameTools.GiveNetworkingObjectName(Object.InputAuthority, HasInputAuthority, HasStateAuthority);

            if (HasInputAuthority)
                Local = this;
        }

        #endregion

        #region Methods

        public void Buy(ShopType shopType, int slot)
        {
            if (!HasStateAuthority)
                return;
            BuyParameters parameters = new() {
                Decks    = GameDataStorage.Instance.PlayerDeck,
                Player   = Object.InputAuthority,
                Shop     = GameDataStorage.Instance.Shop,
                ShopType = shopType,
                Slot     = slot
            };
            // TODO: Check if the player can buy the card
            // Because I think, only the host can buy without problem
            // Wait the shop is implemented to try to buy a card
            // If it's not work, it's because the CheckBuyCommand edit the data of the player and only the host can (I think)
            ICommand buyCommand = new CheckBuyCommand(parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            if (CommandInvoker.State == true)
                RPC_BuyCard(Object.InputAuthority.RawEncoded, shopType, slot);
        }

        #endregion

        #region Player's Commands

        #region Shop Commands

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_BuyCard(int playerRef, ShopType shopType, int slot)
        {
            BuyParameters parameters = new() {
                Decks    = GameDataStorage.Instance.PlayerDeck,
                Player   = PlayerRef.FromEncoded(playerRef),
                Shop     = GameDataStorage.Instance.Shop,
                ShopType = shopType,
                Slot     = slot
            };
            ICommand buyCommand = new BuyCommand(parameters);

            CommandInvoker.ExecuteCommand(buyCommand);
        }

        #endregion

        #endregion
    }
}
