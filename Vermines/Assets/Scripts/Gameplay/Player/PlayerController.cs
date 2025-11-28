using UnityEngine;
using Fusion;
using System;

namespace Vermines.Player {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Core;
    using Vermines.Core.Player;
    using Vermines.ShopSystem.Enumerations;

    public partial class PlayerController : ContextBehaviour, IPlayer {

        public static PlayerController Local { get; private set; }

        #region Player's value

        public string UserID { get; private set; }
        public string UnityID { get; private set; }
        public string Nickname { get; private set; }

        public bool IsInitialized => _InitCounter <= 0;

        [Networked]
        private NetworkString<_64> NetworkedUserID { get; set; }

        [Networked]
        public NetworkString<_32> NetworkedNickname { get; set; }

        [Networked, OnChangedRender(nameof(OnStatisticsChange))]
        public PlayerStatistics Statistics { get; private set; }

        public PlayerDeck Deck { get; private set; }

        private int _InitCounter;

        [Networked]
        private byte _SyncToken { get; set; }

        private byte _LocalSyncToken;

        private bool _PlayerDataSent;

        #endregion

        #region Getters & Setters

        public void SetEloquence(int eloquence)
        {
            if (!HasStateAuthority)
                return;
            int maxEloquence = Context.GameplayMode.MaxEloquence;

            if (maxEloquence < eloquence)
                eloquence = maxEloquence;
            OnEloquenceChanged?.Invoke(Object.InputAuthority, Statistics.Eloquence, eloquence);

            PlayerStatistics stats = Statistics;

            stats.Eloquence = eloquence;

            UpdateStatistics(stats);
        }

        public void SetSouls(int souls)
        {
            if (!HasStateAuthority)
                return;
            OnSoulsChanged?.Invoke(Object.InputAuthority, Statistics.Souls, souls);

            PlayerStatistics stats = Statistics;

            stats.Souls = souls;

            UpdateStatistics(stats);
        }

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

        public void Refresh()
        {
            PlayerStatistics statistics = Statistics;

            statistics.PlayerRef = Object.InputAuthority;

            Statistics = statistics;

            RPC_DeckResynchronization(Deck.Serialize());
        }

        private void UpdateLocalState()
        {
            if (_LocalSyncToken != _SyncToken) {
                UserID   = NetworkedUserID.Value;
                Nickname = NetworkedNickname.Value;
            }
        }

        public override void Spawned()
        {
            _LocalSyncToken = default;
            _PlayerDataSent = false;
            _InitCounter    = 10;

            if (HasInputAuthority) {
                Context.LocalPlayerRef = Object.InputAuthority;

                Local = this;
            }

            UpdateLocalState();

            Runner.SetIsSimulated(Object, true);
        }

        public void Despawn()
        {
            if (!Runner.IsServer)
                return;
            PlayerStatistics stats = Statistics;

            stats.IsConnected = false;

            UpdateStatistics(stats);

            if (HasStateAuthority)
                Local = null;
        }

        public override void FixedUpdateNetwork()
        {
            if (_LocalSyncToken != default && Runner.IsForward)
                _InitCounter = Mathf.Max(0, _LocalSyncToken);
            if (IsProxy)
                return;
            if (HasInputAuthority) {
                Context.LocalPlayerRef = Object.InputAuthority;

                if (!_PlayerDataSent && Runner.IsForward && Context.PlayerData != null) {
                    string unityId = Context.PlayerData.UnityID ?? string.Empty;

                    RPC_SendPlayerData(Object.InputAuthority, Context.PeerUserID, Context.PlayerData.Nickname, Context.PlayerData.Cultist != null ? Context.PlayerData.Cultist.family : CardFamily.None, unityId);

                    _PlayerDataSent = true;
                }
            }
        }

        #endregion

        #region Events

        public Action<PlayerRef, int, int> OnSoulsChanged;     // PlayerRef, oldValue, newValue
        public Action<PlayerRef, int, int> OnEloquenceChanged; // PlayerRef, oldValue, newValue

        public void OnReconnect(PlayerController player)
        {
            UserID            = player.UserID;
            Nickname          = player.Nickname;
            NetworkedUserID   = player.NetworkedUserID;
            NetworkedNickname = player.NetworkedNickname;
            UnityID           = player.UnityID;

            RPC_DeckResynchronization(player.Deck.Serialize());
        }

        private void OnStatisticsChange()
        {
            GameEvents.OnPlayerUpdated.Invoke(this);
        }

        #endregion

        #region RPCs

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_SendPlayerData(PlayerRef playerRef, string userId, string nickname, CardFamily family, string unityId)
        {
            #if UNITY_EDITOR
                nickname += $" {Object.InputAuthority}";
            #endif

            _SyncToken++;

            if (_SyncToken == default)
                _SyncToken = 1;
            _LocalSyncToken = _SyncToken;

            NetworkedUserID   = userId;
            UserID            = userId;
            NetworkedNickname = nickname;
            Nickname          = nickname;
            UnityID           = unityId;

            PlayerStatistics stats = Statistics;

            stats.Family = family;

            UpdateStatistics(stats);

            Context.GameplayMode.OnPlayerDataReceived(playerRef, family);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        public void RPC_DeckResynchronization(string data)
        {
            Deck = PlayerDeck.Deserialize(data);
        }

        [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
        public void RPC_DrawCards(int cardsToDraw)
        {
            for (int i = 0; i < cardsToDraw; i++) {
                ICard drawnCard = Deck.Draw();

                if (drawnCard != null && Object.InputAuthority == Runner.LocalPlayer)
                    GameEvents.InvokeOnDrawCard(drawnCard);
            }
        }

        #region RPCs Ask to Server

        public void OnCardSacrified(int cardId)
        {
            Context.NetworkGame.RPC_CardSacrified(Object.InputAuthority.RawEncoded, cardId);
        }

        public void OnEffectChoice(ICard card, AEffect effect)
        {
            int index = card.Data.Effects.IndexOf(effect);

            if (index < 0)
                return;
            Context.NetworkGame.RPC_EffectChosen(Object.InputAuthority.RawEncoded, card.ID, index);
        }

        public void OnPlay(int cardId)
        {
            Context.NetworkGame.RPC_CardPlayed(Object.InputAuthority.RawEncoded, cardId);
        }

        public void OnDiscard(int cardId)
        {
            Context.NetworkGame.RPC_DiscardCard(Object.InputAuthority.RawEncoded, cardId, true);
        }

        public void OnRecycle(int cardId)
        {
            Context.NetworkGame.RPC_CardRecycled(Object.InputAuthority.RawEncoded, cardId);
        }

        public void OnDiscardNoEffect(int cardId)
        {
            Context.NetworkGame.RPC_DiscardCard(Object.InputAuthority.RawEncoded, cardId, false);
        }

        public void OnBuy(ShopType shopType, int cardId)
        {
            Context.NetworkGame.RPC_BuyCard(Object.InputAuthority.RawEncoded, shopType, cardId);
        }

        public void OnActiveEffectActivated(int cardID)
        {
            Context.NetworkGame.RPC_ActivateEffect(Object.InputAuthority.RawEncoded, cardID);
        }

        public void OnShopReplaceCard(ShopType shopType, int cardId)
        {
            Context.NetworkGame.RPC_ReplaceCardInShop(Object.InputAuthority.RawEncoded, shopType, cardId);
        }

        public void OnReducedInSilenced(ICard cardToBeSilenced)
        {
            Context.NetworkGame.RPC_ReducedInSilenced(Object.InputAuthority.RawEncoded, cardToBeSilenced.ID);
        }

        public void RemoveReducedInSilenced(ICard card, int originalSouls)
        {
            Context.NetworkGame.RPC_RemoveReducedInSilenced(Object.InputAuthority.RawEncoded, card.ID, originalSouls);
        }

        public void NetworkEventCardEffect(int cardID, string data = "")
        {
            Context.NetworkGame.RPC_NetworkEventCardEffect(Object.InputAuthority.RawEncoded, cardID, data);
        }

        #endregion

        #endregion
    }
}
