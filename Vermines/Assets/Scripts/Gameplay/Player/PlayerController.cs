using OMGG.DesignPattern;
using Fusion;
using UnityEngine;

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

        public void OnBuy(ShopType shopType, int slot)
        {
            GameManager.Instance.RPC_BuyCard(shopType, slot, Object.InputAuthority.RawEncoded);
        }

        public void BuyCard(ShopType shopType, int slot)
        {
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

            if (parameters.Shop == null)
            {
                Debug.LogError("Shop is null");
            }

            ICommand buyCommand = new BuyCommand(parameters);

            CommandInvoker.ExecuteCommand(buyCommand);

            Debug.Log($"[SERVER]: Player {playerRef} bought a card at slot {slot} in {shopType}");
        }

        #endregion

        #endregion
    }
}
