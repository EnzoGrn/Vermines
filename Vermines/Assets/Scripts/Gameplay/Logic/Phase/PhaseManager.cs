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
            Debug.Log("IsMyTurn : " + _PlayerTurnIndex + ", " + Runner.LocalPlayer.PlayerId);
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

            _ActualTurnPhase = PhaseType.Sacrifice;
            _PlayerTurnIndex = 0;
            _PlayerTurnCount = 0;
            int idx = 0;

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

            Debug.Log("NextTurn A player complete his turn");
        }

        public void ProcessPhase()
        {
            Debug.Log("ProcessPhase, localPlayer : " + _PlayerTurnIndex + " starts play.");

            // Must check if the actual playerRef is there one compared to the current player index in the list of player Data
            if (Runner.LocalPlayer == _PlayerTurnOrder.Get(_PlayerTurnIndex))
            {
                Debug.Log("ProcessPhase, you (" + _PlayerTurnIndex + ") are playing the phase + " + _ActualTurnPhase.ToString() + ".");
            }
        }

        private void PhaseCompleted()
        {
            if (_ActualTurnPhase == PhaseType.Resolution)
            {
                Debug.Log("Phase : " + _ActualTurnPhase.ToString() + " is completed next turn.");

                if (Runner.IsServer)
                {
                    RPC_NextTurn();
                    RPC_ProcessPhase();
                }
            }
            else
            {
                Debug.Log("Phase : " + _ActualTurnPhase.ToString() + " is completed.");
                if (Runner.IsServer)
                {
                    _ActualTurnPhase = _ActualTurnPhase + 1;
                    RPC_UpdatePhaseUI();
                    RPC_ProcessPhase();
                }
                Debug.Log("New Phase Running : " + _ActualTurnPhase.ToString());
            }
        }

        #region RPC
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_SetUpUI()
        {
            HUDManager.Instance.UpdatePhaseButton(IsMyTurn());
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_UpdatePhaseUI()
        {
            HUDManager.Instance.NextPhase();
            HUDManager.Instance.UpdatePhaseButton(IsMyTurn());
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_ProcessPhase()
        {
            Debug.Log("RPC_ProcessPhase");
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
        // TODO : Call it through the game manager
        public void OnStartPhases()
        {
            Debug.Log("OnStartPhases");

            if (!Runner.IsServer || _IsPlaying)
                return;

            _IsPlaying = true;

            if (HasStateAuthority == false)
            {
                Debug.Log("HasStateAuthority is false");
                return;
            }

            SetUpPhases();
            RPC_ProcessPhase();
        }

        public void OnPlayerWin()
        {
            // Reset Game
            SetUpPhases();
            _IsPlaying = false;

            // TODO : Start the end of the game
        }

        public void OnPhaseCompleted()
        {
            RPC_PhaseCompleted();
        }
        #endregion
    }
}