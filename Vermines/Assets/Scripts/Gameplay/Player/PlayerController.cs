using UnityEngine;
using Fusion;

namespace Vermines.Player {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.Core;
    using Vermines.Core.Player;
    using Vermines.Core.Scene;
    using Vermines.Menu.Screen;
    using Vermines.Network.Utilities;
    using Vermines.Service;
    using Vermines.ShopSystem.Enumerations;

    public partial class PlayerController : NetworkBehaviour, IPlayer, IContextBehaviour {

        #region Player's value

        public string UserID { get; private set; }
        public string UnityID { get; private set; }
        public string Nickname { get; private set; }

        public SceneContext Context { get; set; }

        [Networked]
        private NetworkString<_64> NetworkedUserID { get; set; }

        [Networked]
        private NetworkString<_32> NetworkedNickname { get; set; }

        [Networked]
        public PlayerStatistics Statistics { get; private set; }

        public PlayerDeck Deck { get; private set; }

        #endregion

        #region Methods

        public void UpdateStatistics(PlayerStatistics statistics)
        {
            Statistics = statistics;
        }

        public void UpdateDeck(PlayerDeck deck)
        {
            Deck = deck;
        }

        #endregion

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

        public void OnEffectChoice(ICard card, AEffect effect)
        {
            int index = -1;

            foreach (AEffect e in card.Data.Effects) {
                if (e.Equals(effect))
                    break;
                index++;
            }

            if (index == -1)
                return;
            GameManager.Instance.RPC_EffectChosen(Object.InputAuthority.RawEncoded, card.ID, index + 1);
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

        public void OnBuy(ShopType shopType, int cardId)
        {
            GameManager.Instance.RPC_BuyCard(Object.InputAuthority.RawEncoded, shopType, cardId);
        }

        public void OnActiveEffectActivated(int cardID)
        {
            GameManager.Instance.RPC_ActivateEffect(Object.InputAuthority.RawEncoded, cardID);
        }

        public void OnShopReplaceCard(ShopType shopType, int cardId)
        {
            GameManager.Instance.RPC_ReplaceCardInShop(Object.InputAuthority.RawEncoded, shopType, cardId);
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
