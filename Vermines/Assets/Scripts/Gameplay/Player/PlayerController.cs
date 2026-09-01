using Fusion;
using System;
using UnityEngine;

namespace Vermines.Player {
    using System.Collections.Generic;
    using System.Linq;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Characters;
    using Vermines.Core;
    using Vermines.Core.Player;
    using Vermines.ShopSystem.Enumerations;
    using Vermines.UI;
    using Vermines.UI.Card;

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
        private const int HAND_CAPACITY = 15;

        [Networked, Capacity(HAND_CAPACITY)]
        private NetworkArray<int> HandCardIds => default;

        [Networked, OnChangedRender(nameof(RebuildHandCache))]

        private List<ICard> _HandCache = new();

        public IReadOnlyList<ICard> Hand => _HandCache;


        private int HandCount { get; set; }



        public God God { get; private set; }

        private int _InitCounter;

        [Networked, OnChangedRender(nameof(UpdateLocalState))]
        private byte _SyncToken { get; set; }

        private byte _LocalSyncToken;

        private bool _PlayerDataSent;

        private GameplayUIController _gameplayUICache;
        private DiscardDropHandler _discardDropCache;

        private GameplayUIController GameplayUI
            => _gameplayUICache != null ? _gameplayUICache : (_gameplayUICache = FindFirstObjectByType<GameplayUIController>());

        private DiscardDropHandler DiscardDrop
            => _discardDropCache != null ? _discardDropCache : (_discardDropCache = FindFirstObjectByType<DiscardDropHandler>());

        #endregion

        #region Getters & Setters

        public void SetNumberOfSlotOnTable(int amount)
        {
            if (!HasStateAuthority)
                return;
            PlayerStatistics stats = Statistics;

            stats.NumberOfSlotInTable = amount;

            UpdateStatistics(stats);
        }

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

            if (HasStateAuthority)
                RPC_DeckResynchronization(deck.Serialize());
        }

        public void AddCardToHand(ICard card)
        {
            if (!HasStateAuthority || card == null)
                return;

            List<ICard> hand = _HandCache.ToList();

            hand.Add(card);
            WriteHand(hand);
        }

        public void RemoveCardFromHand(ICard card)
        {
            if (!HasStateAuthority || card == null)
                return;

            List<ICard> hand = _HandCache.ToList();

            hand.Remove(card);
            WriteHand(hand);
        }


        public void DrawAuthoritative(int amount)
        {
            if (!HasStateAuthority)
                return;

            PlayerDeck deck = Deck;
            List<int> drawnIds = new();
            List<ICard> drawnCards = new();

            for (int i = 0; i < amount; i++)
            {
                ICard card = deck.Draw();

                if (card == null)
                    break;
                drawnIds.Add(card.ID);
                drawnCards.Add(card);
            }

            UpdateDeck(deck);

            if (drawnCards.Count > 0)
            {
                List<ICard> hand = _HandCache.ToList();

                hand.AddRange(drawnCards);
                WriteHand(hand);
            }

            if (drawnIds.Count > 0)
                RPC_NotifyDrawn(string.Join(",", drawnIds));
        }

        public void SetGod(God god)
        {
            God = god;

            if (god.Effects != null) {
                foreach (var effect in god.Effects)
                    effect.Initialize(Context, null);
            }
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
            UserID = NetworkedUserID.Value;
            Nickname = NetworkedNickname.Value;
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
            RebuildHandCache();
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

                    RPC_SendPlayerData(
                        Object.InputAuthority,
                        Context.PeerUserID ?? string.Empty,
                        Context.PlayerData.Nickname ?? string.Empty,
                        Context.PlayerData.Cultist != null ? Context.PlayerData.Cultist.family : CardFamily.None,
                        unityId ?? string.Empty
                    );

                    _PlayerDataSent = true;
                }
            }
        }

        private void RebuildHandCache()
        {
            _HandCache.Clear();

            for (int i = 0; i < HandCount; i++)
            {
                ICard card = CardSetDatabase.Instance.GetCardByID(HandCardIds[i]);

                if (card != null)
                    _HandCache.Add(card);
            }
        }

        private void WriteHand(List<ICard> hand)
        {
            if (!HasStateAuthority)
            {
                Log.Error("[PlayerController] WriteHand appelé hors StateAuthority — ignoré.");

                return;
            }

            int count = Mathf.Min(hand?.Count ?? 0, HAND_CAPACITY);

            if (hand != null && hand.Count > HAND_CAPACITY)
                Log.Error($"[PlayerController] Hand dépasse HAND_CAPACITY ({hand.Count} > {HAND_CAPACITY}) — cartes en trop tronquées. Augmenter HAND_CAPACITY.");

            for (int i = 0; i < count; i++)
                HandCardIds.Set(i, hand[i].ID);

            HandCount = count; // déclenche OnChangedRender -> RebuildHandCache, y compris localement sur l'autorité

            RebuildHandCache(); // l'autorité ne reçoit pas nécessairement son propre OnChangedRender au même tick ; on force la cohérence immédiate côté serveur
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

            if (Context.GameplayMode != null)
                Context.GameplayMode.OnPlayerDataReceived(playerRef, family);
            else
                Log.Error("[PlayerController] RPC_SendPlayerData : Context.GameplayMode is null");
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        public void RPC_DeckResynchronization(string data)
        {
            if (HasStateAuthority)
                return;
            Deck = PlayerDeck.Deserialize(data);
        }

        public void NotifyDrawnToOwner(int cardId)
        {
            if (!HasStateAuthority)
                return;
            RPC_NotifyDrawn(cardId.ToString());
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_NotifyDrawn(string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return;

            Debug.Log($"[REVEAL] {UserID} ids={ids} localPlayer={Runner.LocalPlayer}");

            string[] parts = ids.Split(',');

            for (int i = 0; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out int id))
                {
                    ICard card = CardSetDatabase.Instance.GetCardByID(id);

                    if (card != null)
                        GameEvents.InvokeOnDrawCard(card);
                }
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

        public void OnRequestNewCardInCourtyard(int level)
        {
            Context.NetworkGame.RPC_AddCardInCourtyard(Object.InputAuthority.RawEncoded, level);
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
