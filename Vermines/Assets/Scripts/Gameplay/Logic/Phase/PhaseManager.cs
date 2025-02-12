using Fusion;
using UnityEngine;
using Vermines.HUD;

namespace Vermines
{
    public enum PhaseType
    {
        Sacrifice,
        Gain,
        Action,
        Resolution
    }

    public class PhaseManager : NetworkBehaviour
    {
        [Networked, Capacity(4)] private NetworkArray<PlayerRef> _PlayerTurnOrder { get; }

        [Networked] private bool _IsPlaying { get; set; } = false;
        [Networked] private int _PlayerTurnCount { get; set; } = 0;
        [Networked] private int _PlayerTurnIndex { get; set; }

        [Networked] private PhaseType _ActualTurnPhase { get;  set; }

        public override void Spawned()
        {
            base.Spawned();
        }

        public int GetTurnCount()
        {
            return _PlayerTurnCount;
        }

        public int GetPlayerTurn()
        {
            return _PlayerTurnIndex;
        }

        public bool IsMyTurn()
        {
            // +1 To adjust to Fusion player ID
            return (_PlayerTurnIndex + 1 == Runner.LocalPlayer.PlayerId);
        }

        public bool IsPlaying()
        {
            return _IsPlaying;
        }

        private void SetUpPhases()
        {
            if (!Runner.IsServer)
            {
                return;
            }

            int idx = 0;

            _ActualTurnPhase = PhaseType.Sacrifice;
            _PlayerTurnIndex = 0;
            _PlayerTurnCount = 0;

            foreach (var playerDate in GameDataStorage.Instance.PlayerData)
            {
                _PlayerTurnOrder.Set(idx, playerDate.Key);
                idx++;
            }
            RPC_SetUpUI();
        }

        public void NextTurn()
        {
            if (!Runner.IsServer)
                return;

            _ActualTurnPhase = PhaseType.Sacrifice;

            if (((_PlayerTurnIndex + 1) % GameDataStorage.Instance.PlayerData.Count) == 0)
            {
                _PlayerTurnCount++;
            }

            _PlayerTurnIndex = (_PlayerTurnIndex + 1) % GameDataStorage.Instance.PlayerData.Count;
            RPC_UpdatePhaseUI();
        }

        public void ProcessPhase()
        {
            // Compare the Player turn index with the LocalPlayer ID in the list of player Data to process some logic if needed
            if (Runner.LocalPlayer == _PlayerTurnOrder.Get(_PlayerTurnIndex))
            {
                // TODO : Implement game logic
                Debug.Log("ProcessPhase, you (" + _PlayerTurnIndex + ") are playing the phase " + _ActualTurnPhase.ToString() + ".");
            }
        }

        private void PhaseCompleted()
        {
            // Check if the turn of the actual player is done
            if (_ActualTurnPhase == PhaseType.Resolution)
            {
                if (Runner.IsServer)
                {
                    RPC_NextTurn();
                    RPC_ProcessPhase();
                }
            }
            else
            {
                // A phase is completed, go on next one
                if (Runner.IsServer)
                {
                    _ActualTurnPhase = _ActualTurnPhase + 1;
                    RPC_UpdatePhaseUI();
                    RPC_ProcessPhase();
                }
            }
        }

        #region RPC
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_SetUpUI()
        {
            HUDManager.instance.UpdatePhaseButton(IsMyTurn());
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_UpdatePhaseUI()
        {
            HUDManager.instance.NextPhase();
            HUDManager.instance.UpdatePhaseButton(IsMyTurn());
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_ProcessPhase()
        {
            ProcessPhase();
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
        public void RPC_PhaseCompleted()
        {
            PhaseCompleted();
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_NextTurn()
        {
            NextTurn();
        }
        #endregion

        #region events
        // TODO : Call it using the game manager, this method setUp value for the phase Manager
        public void OnStartPhases()
        {
            if (!Runner.IsServer || _IsPlaying)
                return;

            _IsPlaying = true;

            SetUpPhases();
            RPC_ProcessPhase();
        }

        // TODO : Call this function in the game logic
        public void OnPlayerWin()
        {
            // Reset Game
            SetUpPhases();
            _IsPlaying = false;

            // TODO : Start the end of the game
        }

        // Called by the Phase Button
        public void OnPhaseCompleted()
        {
            RPC_PhaseCompleted();
        }
        #endregion
    }
}