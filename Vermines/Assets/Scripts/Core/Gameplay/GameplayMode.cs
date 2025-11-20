using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Newtonsoft.Json;
using WebSocketSharp;
using UnityEngine;
using Fusion;

namespace Vermines.Core {
    using OMGG.Chronicle;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.Core.Network;
    using Vermines.Core.Player;
    using Vermines.Gameplay;
    using Vermines.Gameplay.Chronicle;
    using Vermines.Gameplay.Phases;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;
    using Vermines.ShopSystem.Data;
    using Vermines.ShopSystem.Enumerations;

    public abstract class GameplayMode : ContextBehaviour {

        public enum GState {
            None,
            Active,
            Finished
        }

        #region Attributes

        [SerializeField]
        private GameplayType _Type;

        public int SoulsLimit         = 100;
        public int MaxEloquence       = 20;
        public int BonusSoulsOnFamily = 5;

        private DefaultPlayerComparer _PlayerComparer = new();

        public PhaseManager PhaseManager;

        public Action<PlayerRef> OnPlayerJoinedGame;
        public Action<string> OnPlayerLeftGame;

        private readonly Dictionary<PlayerRef, CardFamily> _PlayerFamilies = new();

        private readonly HashSet<PlayerRef> _PendingPlayers = new();

        protected GameModeInitializer _Initializer;

        private bool _IsInitialized;

        [SerializeField]
        private ShopData _BaseShopData;

        private ShopData _RuntimeShopData;

        [Networked, Capacity(4)]
        public NetworkArray<PlayerRef> PlayerTurnOrder { get; }

        [Networked]
        public int CurrentPlayerIndex { get; set; } = 0;

        [Networked]
        public int TotalTurnPlayed { get; set; } = 0;

        public ChronicleManager Announcer = new();

        #endregion

        #region Getters & Setters

        [Networked, HideInInspector]
        public GState State { get; private set; }

        public GameplayType Type => _Type;

        public ShopData Shop => _RuntimeShopData;

        public PlayerRef CurrentPlayer => PlayerTurnOrder.Get(CurrentPlayerIndex);

        public PhaseType CurrentPhase => PhaseManager.CurrentPhase;

        public bool IsMyTurn => PlayerTurnOrder.Get(CurrentPlayerIndex) == Runner.LocalPlayer;

        public void SetPlayerTurnOrder(List<PlayerRef> orderedPlayers)
        {
            if (!HasStateAuthority)
                return;
            for (int i = 0; i < orderedPlayers.Count; i++)
                PlayerTurnOrder.Set(i, orderedPlayers[i]);
        }

        #endregion

        #region Networked Methods

        public override void Spawned()
        {
            Context.GameplayMode = this;

            if (HasStateAuthority) {
                _PendingPlayers.Clear();

                foreach (PlayerRef player in Runner.ActivePlayers) {
                    if (!player.IsRealPlayer)
                        continue;
                    _PendingPlayers.Add(player);
                }
            }

            if (_BaseShopData != null) {
                _RuntimeShopData = _BaseShopData.DeepCopy();

                _RuntimeShopData.Initialize();
            }

            Announcer.OnChronicleAdded   += (entry) => Debug.Log($"[Chronicle]: {entry.Id} - {entry.TitleKey}: {entry.MessageKey} {entry.PayloadJson}"); // Some entry can already have their payload when they are adds.
            Announcer.OnChronicleUpdated += (entry) => Debug.Log($"[Chronicle]: {entry.Id} - {entry.PayloadJson}");
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
                return;
            switch (State) {
                case GState.Active:
                    FixedUpdateNetwork_Active();
                    break;

                case GState.Finished:
                    FixedUpdateNetwork_Finished();
                    break;
            }
        }

        #endregion

        #region Methods

        public void Activate()
        {
            if (!Runner.IsServer || State != GState.None)
                return;
            OnActivate();
        }

        public void Initialize(string data)
        {
            _Initializer.Initialize(data);

            _IsInitialized = true;

            RPC_Initialized();
        }

        private void FixedUpdateNetwork_Active() {}

        private void FixedUpdateNetwork_Finished() {}

        public void PlayerJoined(PlayerController player)
        {
            PlayerStatistics stats = player.Statistics;

            if (!stats.IsConnected) {
                stats.IsConnected = true;

                player.UpdateStatistics(stats);
            }

            RPC_PlayerJoinedGame(player.Object.InputAuthority);
        }

        public void PlayerLeft(PlayerController player)
        {
            if (!Runner.IsServer || State == GState.Finished)
                return;
            player.Despawn();

            string nickname = player.Nickname;

            // TODO: Setup something to allow a player to rejoined.

            if (nickname.IsNullOrEmpty())
                nickname = "Unknown";
            RPC_PlayerLeftGame(player.Object.InputAuthority, nickname);
            CheckWinCondition();
        }

        public void StopGame()
        {
            bool isCustom = Context.NetworkGame.IsCustomGame();

            if (isCustom) {
                StopCustomGame();
            } else {
                if (!HasStateAuthority) {
                    Global.Networking.StopGame();

                    return;
                }

                StartCoroutine(StopPublicGameCoroutine());
            }
        }

        protected void FinishGameplay()
        {
            if (State != GState.Active || !Runner.IsServer)
                return;
            State = GState.Finished;
        }

        private void CheckAllPlayersInitialized()
        {
            if (_IsInitialized || _PendingPlayers.Count > 0)
                return;
            string data = JsonConvert.SerializeObject(_PlayerFamilies.ToDictionary(
                kvp => kvp.Key.RawEncoded, kvp => kvp.Value
            ));

            Initialize(data);
            Activate();
        }

        #endregion

        #region Abstract Methods

        protected abstract void CheckWinCondition();

        protected virtual void SortPlayers(List<PlayerStatistics> allStatistics)
        {
            allStatistics.Sort(_PlayerComparer);
        }

        protected virtual void PreparePlayerStatistics(ref PlayerStatistics stats) {}

        #endregion

        #region Enumerator Methods

        private IEnumerator StopPublicGameCoroutine()
        {
            RPC_StopPublicGame();

            yield return new WaitForSecondsRealtime(0.25f);

            Global.Networking.StopGame();
        }

        private void StopCustomGame()
        {
            var players = Context.NetworkGame.GetConnectedPlayer();

            RPC_ShowLoadingScreen(true);

            Context.SceneChangeController.RPC_RequestSceneChange(Context.CustomGameScenePath, true, false, Type, Context.GameScenePath, players.Count);
        }

        #endregion

        #region Events

        protected virtual void OnInitialize()
        {
            PhaseManager.Initialize();

            State = GState.Active;

            GameEvents.OnGameInitialized.Invoke();
        }

        protected virtual void OnActivate() {}

        public void OnPlayerDataReceived(PlayerRef player, CardFamily family)
        {
            if (_IsInitialized)
                return;
            _PlayerFamilies[player] = family;

            _PendingPlayers.Remove(player);

            CheckAllPlayersInitialized();
        }

        #region RPCs Events

        public virtual void OnBuyCard(PlayerRef player, ShopType shopType, int cardID) { }
        public virtual void OnReplaceCardInShop(PlayerRef player, ShopType shopType, int cardID) { }

        public virtual void OnCardPlayed(PlayerRef player, int cardID) { }
        public virtual void OnDiscardCard(PlayerRef player, int cardID, bool hasEffect = true) { }

        public virtual void OnCardSacrified(PlayerRef player, int cardID) { }

        public virtual void OnCardRecycled(PlayerRef player, int cardID) { }

        public virtual void OnActivateEffect(PlayerRef player, int cardID) { }
        public virtual void OnReducedInSilenced(PlayerRef player, int cardToBeSilenced) { }
        public virtual void OnRemoveReducedInSilenced(PlayerRef player, int cardID, int originalSouls) { }
        public virtual void OnNetworkEventCardEffect(PlayerRef player, int cardID, string data) { }
        public virtual void OnEffectChosen(PlayerRef player, int cardID, int effectIndex) { }

        protected abstract void OnInitializeCards(List<CardFamily> families);
        protected abstract void OnInitializeShop(ShopType shopType, string shopData);

        #endregion

        #endregion

        #region RPCs

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_Initialized()
        {
            OnInitialize();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_PlayerJoinedGame(PlayerRef playerRef)
        {
            OnPlayerJoinedGame?.Invoke(playerRef);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_PlayerLeftGame(PlayerRef playerRef, string nickname)
        {
            OnPlayerLeftGame?.Invoke(nickname);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies, Channel = RpcChannel.Reliable)]
        private void RPC_StopPublicGame()
        {
            Global.Networking.StopGame(Networking.STATUS_SERVER_CLOSED);
        }

        [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_ShowLoadingScreen(bool show)
        {
            StartCoroutine(Global.Networking.ShowLoadingSceneCoroutine(show));
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_AskPayload(int playerID, string id)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(PlayerRef.FromEncoded(playerID));

            if (ChroniclePayloadStorage.TryGet(id, out string payloadJson))
                player.RPC_ReceivePayload(id, payloadJson);
        }

        #region RPCs Initializer

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        public void RPC_InitializeCards(int[] familiesIds)
        {
            List<CardFamily> families = FamilyUtils.FamiliesIdsToList(familiesIds);

            OnInitializeCards(families);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        public void RPC_InitializeShop(ShopType shopType, string shopData)
        {
            OnInitializeShop(shopType, shopData);
        }

        #endregion

        #endregion

        #region Helpers

        private class DefaultPlayerComparer : IComparer<PlayerStatistics> {

            public int Compare(PlayerStatistics x, PlayerStatistics y)
            {
                return y.Souls.CompareTo(x.Souls);
            }
        }

        #endregion
    }
}
