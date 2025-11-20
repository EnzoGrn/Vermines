using Fusion ;
using OMGG.Network.Fusion;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

namespace Vermines.Gameplay.Phases {
    
    using Vermines.CardSystem.Elements;
    using Vermines.Core;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;
    using Vermines.UI;
    using Vermines.UI.Screen;

    [System.Serializable]
    public class PhaseEntry {
        public PhaseType phaseType;
        public PhaseAsset phaseAsset;
    }

    public class PhaseManager : ContextBehaviour {

        #region Phases

        [Networked]
        public PhaseType CurrentPhase { get; set; }

        [SerializeField]
        private List<PhaseEntry> phaseEntries;

        private Dictionary<PhaseType, PhaseAsset> _Phases;
        public Dictionary<PhaseType, PhaseAsset> Phases => _Phases;

        #endregion

        #region Network Methods

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            Deinitialize();

            base.Despawned(runner, hasState);
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            SetUpPhases();
            SetUpUI();
            SetUpEvents();

            GameEvents.OnGameInitialized.AddListener(OnGameStart);
        }

        public void Deinitialize()
        {
            GameEvents.OnAttemptNextPhase.RemoveListener(OnPhaseCompleted);

            if (_Phases != null) {
                foreach (var kvp in _Phases)
                    kvp.Value.Deinitialize();
            }
        }

        private void SetUpPhases()
        {
            _Phases = new();

            foreach (var entry in phaseEntries) {
                entry.phaseAsset.Initialize(Context, this);

                _Phases[entry.phaseType] = entry.phaseAsset;
            }

            if (Runner.IsServer)
                CurrentPhase = _Phases.Keys.First();
        }

        public void OnGameStart()
        {
            if (HasStateAuthority)
                RPC_ProcessPhase(CurrentPhase, Context.GameplayMode.PlayerTurnOrder.Get(Context.GameplayMode.CurrentPlayerIndex));
            GameEvents.OnGameInitialized.RemoveListener(OnGameStart);
        }

        private void SetUpUI()
        {
            GameEvents.OnPlayerInitialized.Invoke();
        }

        private void SetUpEvents()
        {
            GameEvents.OnAttemptNextPhase.AddListener(OnPhaseCompleted);
        }

        #endregion

        public void NextTurn()
        {
            ResetCardActivations();

            if (!Runner.IsServer)
                return;
            CurrentPhase = _Phases.Keys.First();

            Context.GameplayMode.CurrentPlayerIndex = (Context.GameplayMode.CurrentPlayerIndex + 1) % Context.NetworkGame.ActivePlayers.Count;

            if (Context.GameplayMode.CurrentPlayerIndex == 0)
                Context.GameplayMode.TotalTurnPlayed++;
            RPC_UpdateTurnUI();
        }

        private IEnumerator SacrificeRoutine()
        {
            GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>(FindObjectsInactive.Include);

            if (gameplayUIController != null)
                gameplayUIController.Show<GameplayUITurn>();

            yield return new WaitForSeconds(3f);

            if (gameplayUIController != null &&
                gameplayUIController.GetActiveScreen(out GameplayUIScreen activeScreen) &&
                activeScreen is GameplayUITurn)
            {
                gameplayUIController.Hide();
            }

            if (!Runner.IsServer)
                yield break;
            RPC_ProcessPhase(CurrentPhase, Context.GameplayMode.PlayerTurnOrder.Get(Context.GameplayMode.CurrentPlayerIndex));
        }

        private void ResetCardActivations()
        {
            PlayerRef playerRef = Context.GameplayMode.PlayerTurnOrder.Get(Context.GameplayMode.CurrentPlayerIndex);

            foreach (ICard card in Context.NetworkGame.GetPlayer(playerRef).Deck.PlayedCards)
                card.HasBeenActivatedThisTurn = false;
        }

        public void ProcessPhase(PhaseType currentPhase, PlayerRef playerRef)
        {
            _Phases[currentPhase].Run(playerRef);
        }

        public void PhaseCompleted()
        {
            if (!Runner.IsServer)
                return;

            // Check if the player did every phases.
            if (CurrentPhase == PhaseType.Resolution) {
                NextTurn();

                RPC_TurnAnnounced();
            } else {
                CurrentPhase++;

                Debug.Log($"[SERVER]: Next phase is {CurrentPhase}.");

                RPC_UpdatePhaseUI();
                RPC_ProcessPhase(CurrentPhase, Context.GameplayMode.PlayerTurnOrder.Get(Context.GameplayMode.CurrentPlayerIndex));
            }
        }

        #region RPC

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_UpdatePhaseUI()
        {
            GameEvents.OnPhaseChanged.Invoke(CurrentPhase);
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_UpdateTurnUI()
        {
            GameEvents.OnTurnChanged.Invoke(Context.GameplayMode.CurrentPlayerIndex);
            GameEvents.OnPhaseChanged.Invoke(CurrentPhase);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentPhase">The currentPhase may intefer since it is a network variable, this argument avoid desync between clients</param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_ProcessPhase(PhaseType currentPhase, PlayerRef playeRef)
        {
            ProcessPhase(currentPhase, playeRef);
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
        public void RPC_PhaseCompleted()
        {
            PhaseCompleted();
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_TurnAnnounced()
        {
            StartCoroutine(SacrificeRoutine());
        }

        #endregion

        #region Events

        public void OnStartPhases()
        {
            if (!Runner.IsServer || Context.GameplayMode.State != GameplayMode.GState.Active)
                return;
            List<PlayerController> players = Context.Runner.GetAllBehaviours<PlayerController>();

            foreach (var player in players)
                GameEvents.OnPlayerUpdated.Invoke(player);
            RPC_TurnAnnounced();
        }

        public void OnPhaseCompleted()
        {
            Debug.Log("$[SERVER]: Event triggered | Asking for a phase completion.");

            RPC_PhaseCompleted();
        }

        #endregion
    }
}
