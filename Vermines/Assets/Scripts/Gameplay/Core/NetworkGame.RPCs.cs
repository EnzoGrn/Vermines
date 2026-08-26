using Fusion;

namespace Vermines.Core {

    using Vermines.ShopSystem.Enumerations;
    
    public partial class NetworkGame : ContextBehaviour, IPlayerJoined, IPlayerLeft {

        #region RPCs Shop

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_BuyCard(int playerID, ShopType shopType, int cardID, RpcInfo info = default)
        {
            if (!IsRpcSourceValid(info, playerID)) return;
            if (!IsGameplayReady()) return;

            _Gameplay.OnBuyCard(PlayerRef.FromEncoded(playerID), shopType, cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_AddCardInCourtyard(int playerID, int level, RpcInfo info = default)
        {
            if (!IsRpcSourceValid(info, playerID)) return;
            if (!IsGameplayReady()) return;

            _Gameplay.OnCardAddInCourtyard(PlayerRef.FromEncoded(playerID), level);
        }

        #endregion

        #region RPCs Table (Play, Discard)

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_CardPlayed(int playerID, int cardID, RpcInfo info = default)
        {
            if (!IsRpcSourceValid(info, playerID)) return;
            if (!IsGameplayReady()) return;

            _Gameplay.OnCardPlayed(PlayerRef.FromEncoded(playerID), cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_DiscardCard(int playerID, int cardID, bool hasEffect = true, RpcInfo info = default)
        {
            if (!IsRpcSourceValid(info, playerID)) return;
            if (!IsGameplayReady()) return;

            _Gameplay.OnDiscardCard(PlayerRef.FromEncoded(playerID), cardID, hasEffect);
        }

        #endregion

        #region RPCs Sacrifice

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_CardSacrified(int playerID, int cardID, RpcInfo info = default)
        {
            if (!IsRpcSourceValid(info, playerID)) return;
            if (!IsGameplayReady()) return;

            _Gameplay.OnCardSacrified(PlayerRef.FromEncoded(playerID), cardID);
        }


        #endregion

        #region RPCs Recycle

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_CardRecycled(int playerID, int cardID, RpcInfo info = default)
        {
            if (!IsRpcSourceValid(info, playerID)) return;
            if (!IsGameplayReady()) return;

            _Gameplay.OnCardRecycled(PlayerRef.FromEncoded(playerID), cardID);
        }

        #endregion

        #region RPCs Effects

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_ActivateEffect(int playerID, int cardID, RpcInfo info = default)
        {
            if (!IsRpcSourceValid(info, playerID)) return;
            if (!IsGameplayReady()) return;

            _Gameplay.OnActivateEffect(PlayerRef.FromEncoded(playerID), cardID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_ReducedInSilenced(int playerID, int cardToBeSilenced, RpcInfo info = default)
        {
            if (!IsRpcSourceValid(info, playerID)) return;
            if (!IsGameplayReady()) return;

            _Gameplay.OnReducedInSilenced(PlayerRef.FromEncoded(playerID), cardToBeSilenced);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_RemoveReducedInSilenced(int playerID, int cardID, int originalSouls, RpcInfo info = default)
        {
            if (!IsRpcSourceValid(info, playerID)) return;
            if (!IsGameplayReady()) return;

            _Gameplay.OnRemoveReducedInSilenced(PlayerRef.FromEncoded(playerID), cardID, originalSouls);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_NetworkEventCardEffect(int playerID, int cardID, string data, RpcInfo info = default)
        {
            if (!IsRpcSourceValid(info, playerID)) return;
            if (!IsGameplayReady()) return;

            _Gameplay.OnNetworkEventCardEffect(PlayerRef.FromEncoded(playerID), cardID, data);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_EffectChosen(int playerID, int cardID, int effectIndex, RpcInfo info = default)
        {
            if (!IsRpcSourceValid(info, playerID)) return;
            if (!IsGameplayReady()) return;

            _Gameplay.OnEffectChosen(PlayerRef.FromEncoded(playerID), cardID, effectIndex);
        }

        #endregion
    }
}
