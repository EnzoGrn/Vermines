using UnityEngine;
using Fusion;

namespace Vermines.Player {

    using Vermines.CardSystem.Elements;
    using Vermines.Menu.Screen;
    using Vermines.Network.Utilities;
    using Vermines.Service;
    using Vermines.ShopSystem.Enumerations;

    public partial class PlayerController : NetworkBehaviour {

        public static PlayerController Local { get; private set; }

        public PlayerRef PlayerRef => Object.InputAuthority;

        #region Override Methods

        public override void Spawned()
        {
            name = NetworkNameTools.GiveNetworkingObjectName(Object.InputAuthority, HasInputAuthority, HasStateAuthority);

            if (HasInputAuthority)
                Local = this;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (HasInputAuthority && Local == this)
                Local = null;
        }

        #endregion

        #region Methods

        public void Update()
        {
            if (!HasInputAuthority)
                return;
            if (Input.GetKeyDown(KeyCode.Escape)) {
                VMUI_Gameplay gameplay = FindFirstObjectByType<VMUI_Gameplay>();
                
                if (gameplay)
                    gameplay.TogglePauseMenu();
            }
        }

        public void ReturnToMenu()
        {
            VerminesPlayerService services = FindFirstObjectByType<VerminesPlayerService>(FindObjectsInactive.Include);

            if (services.IsCustomGame())
                RPC_ForceQuitCustomGame(PlayerRef);
            else
                RPC_ForceQuit(PlayerRef);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public async void RPC_ForceQuitCustomGame(PlayerRef player)
        {
            GameManager manager = FindFirstObjectByType<GameManager>();

            if (!manager)
                return;
            await manager.ForceEndCustomGame(player);
        }


        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public async void RPC_ForceQuit(PlayerRef player)
        {
            GameManager manager = FindFirstObjectByType<GameManager>();

            if (!manager)
                return;
            await manager.ReturnToTavern();
        }

        public void OnCardSacrified(int cardId)
        {
            GameManager.Instance.RPC_CardSacrified(Object.InputAuthority.RawEncoded, cardId);
        }

        public void OnPlay(int cardId)
        {
            GameManager.Instance.RPC_CardPlayed(Object.InputAuthority.RawEncoded, cardId);
        }

        public void OnDiscard(int cardId)
        {
            GameManager.Instance.RPC_DiscardCard(Object.InputAuthority.RawEncoded, cardId, true);
        }

        public void OnRecycle(int cardId)
        {
            GameManager.Instance.RPC_CardRecycled(Object.InputAuthority.RawEncoded, cardId);
        }

        public void OnDiscardNoEffect(int cardId)
        {
            GameManager.Instance.RPC_DiscardCard(Object.InputAuthority.RawEncoded, cardId, false);
        }

        public void OnBuy(ShopType shopType, int slot)
        {
            GameManager.Instance.RPC_BuyCard(shopType, slot, Object.InputAuthority.RawEncoded);
        }

        public void OnActiveEffectActivated(int cardID)
        {
            GameManager.Instance.RPC_ActivateEffect(Object.InputAuthority.RawEncoded, cardID);
        }

        public void OnShopReplaceCard(ShopType shopType, int slot)
        {
            GameManager.Instance.RPC_ReplaceCardInShop(Object.InputAuthority.RawEncoded, shopType, slot);
        }

        public void OnReducedInSilenced(ICard cardToBeSilenced)
        {
            GameManager.Instance.RPC_ReducedInSilenced(Object.InputAuthority.RawEncoded, cardToBeSilenced.ID);
        }

        public void RemoveReducedInSilenced(ICard card, int originalSouls)
        {
            GameManager.Instance.RPC_RemoveReducedInSilenced(Object.InputAuthority.RawEncoded, card.ID, originalSouls);
        }

        public void NetworkEventCardEffect(int cardID, string data = "")
        {
            GameManager.Instance.RPC_NetworkEventCardEffect(Object.InputAuthority.RawEncoded, cardID, data);
        }

        #endregion
    }
}
