using Fusion ;
using OMGG.Network.Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vermines.HUD;

namespace Vermines.Gameplay.Phases {

    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.UI;

    public class PhaseManager : NetworkBehaviour {

        #region Singleton

        public static PhaseManager Instance => NetworkSingleton<PhaseManager>.Instance;

        #endregion

        #region Phases

        [Networked]
        public PhaseType CurrentPhase { get; set; }

        private Dictionary<PhaseType, IPhase> _Phases;

        #endregion

        #region Fields
        #endregion

        #region Methods

        public void Initialize()
        {
            SetUpPhases();
            SetUpUI();
            SetUpEvents();
        }

        private void SetUpPhases()
        {
            _Phases = new Dictionary<PhaseType, IPhase>() {
                { PhaseType.Sacrifice , new SacrificePhase() },
                { PhaseType.Gain      , new GainPhase() },
                { PhaseType.Action    , new ActionPhase() },
                { PhaseType.Resolution, new ResolutionPhase() }
            };

            if (Runner.IsServer)
                CurrentPhase = PhaseType.Sacrifice;
        }

        private void SetUpUI()
        {
            TurnManager.Instance.Init(GameDataStorage.Instance.PlayerData);
        }

        private void SetUpEvents()
        {
            Debug.Log("[UI]: Listener correctly add to the 'OnAttemptNextPhase' event.");

            GameEvents.OnAttemptNextPhase.AddListener(OnPhaseCompleted);
        }

        #endregion

        public void NextTurn()
        {
            if (!Runner.IsServer)
                return;
            CurrentPhase = PhaseType.Sacrifice;

            GameManager.Instance.CurrentPlayerIndex = (GameManager.Instance.CurrentPlayerIndex + 1) % GameDataStorage.Instance.PlayerData.Count;

            // TODO: Add to the config a limit of turn and end the game if reach
            if (GameManager.Instance.CurrentPlayerIndex == 0)
                GameManager.Instance.TotalTurnPlayed++;
            RPC_UpdateTurnUI();
        }

        public void ProcessPhase(PhaseType currentPhase, PlayerRef playerRef)
        {
            Debug.Log($"[SERVER]: Processing the phase for {playerRef}, currently playing {currentPhase}");

            _Phases[currentPhase].Run(playerRef);
        }

        public void PhaseCompleted()
        {
            Debug.Log($"[SERVER]: Phase {CurrentPhase} completed");
            Debug.Log($"[SERVER]: Waiting for the next phase...");

            if (!Runner.IsServer)
                return;
            // Check if the player did every phases.
            if (CurrentPhase == PhaseType.Resolution) {
                NextTurn();
            } else {
                CurrentPhase++;

                Debug.Log($"[SERVER]: Next phase is {CurrentPhase}.");

                RPC_UpdatePhaseUI();
            }

            RPC_ProcessPhase(CurrentPhase, GameManager.Instance.PlayerTurnOrder.Get(GameManager.Instance.CurrentPlayerIndex));
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
            GameEvents.OnTurnChanged.Invoke(GameManager.Instance.CurrentPlayerIndex);
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

        #endregion

        #region Events

        public void OnStartPhases()
        {
            if (!Runner.IsServer || !GameManager.Instance.Start || Runner.ActivePlayers.Count() < GameManager.Instance.Config.MinPlayers.Value)
                return;
            TurnManager.Instance.UpdateAllPlayers(GameDataStorage.Instance.PlayerData);
            RPC_ProcessPhase(CurrentPhase, GameManager.Instance.PlayerTurnOrder.Get(GameManager.Instance.CurrentPlayerIndex));
        }

        public void OnPhaseCompleted()
        {
            Debug.Log("$[SERVER]: Event triggered | Asking for a phase completion.");

            RPC_PhaseCompleted();
        }

        #endregion
    }
}
