using Fusion ;
using OMGG.Network.Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vermines.HUD;

namespace Vermines
{
    public enum PhaseType
    {
        Sacrifice = 0,
        Gain = 1,
        Action = 2,
        Resolution = 3
    }

    public class PhaseManager : NetworkBehaviour
    {
        // Singleton
        public static PhaseManager Instance => NetworkSingleton<PhaseManager>.Instance;

        [Networked, Capacity(4)] private NetworkArray<PlayerRef> _PlayerTurnOrder { get; }

        [Networked] private bool _IsPlaying { get; set; } = false;
        [Networked] private int _PlayerTurnCount { get; set; } = 0;
        [Networked] private int _PlayerTurnIndex { get; set; }

        [Networked] private PhaseType _ActualTurnPhase { get;  set; }

        private Dictionary<PhaseType, GameObject> _Phases;
        [SerializeField] public List<GameObject> PhaseList;

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

            RPC_InittializePhases();

            int idx = 0;

            _ActualTurnPhase = PhaseType.Sacrifice;
            _PlayerTurnIndex = 0;
            _PlayerTurnCount = 0;

            foreach (var playerDate in GameDataStorage.Instance.PlayerData)
            {
                _PlayerTurnOrder.Set(idx, playerDate.Key);
                idx++;
            }
            
            // Loop on every Phase of the enum and create a instantiate a phase object
            RPC_SetUpUI();
        }

        public void SetUpEvents()
        {
            Debug.LogWarning("Add Listener for the button and endPhase call.");

            // Next Phase Button
            GameEvents.OnAttemptNextPhase.AddListener(OnPhaseCompleted);
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
            PlayerRef playerRef = _PlayerTurnOrder.Get(_PlayerTurnIndex);

            // Compare the Player turn index with the LocalPlayer ID in the list of player Data to process some logic if needed
            if (Runner.LocalPlayer == playerRef)
            {
                // TODO : Implement game logic
                Debug.Log("ProcessPhase, you (" + _PlayerTurnIndex + ") are playing the phase " + _ActualTurnPhase.ToString() + ".");

                // Run Actual Phase Logic
                Debug.Log($"PlayerRef of ProcessPhase {playerRef}.");

                // Dump all the key in playerDeck
                //foreach (var playerKey in GameDataStorage.Instance.PlayerDeck)
                //{
                //    Debug.Log($"PlayerRef of PlayerDeck {playerRef}.");
                //}

                switch (_ActualTurnPhase)
                {
                    case PhaseType.Sacrifice:
                        // Sacrifise phase component
                        SacrificePhase phase = _Phases[_ActualTurnPhase].gameObject.GetComponent<SacrificePhase>();

                        if (phase != null)
                        {
                            Debug.Log($"Phase {_ActualTurnPhase} not null, run the phase");
                            phase.RunPhase(playerRef);

                        }
                        else
                        {
                            Debug.LogError("Cannot retreive the phase script :(");
                        }
                        break;
                    case PhaseType.Gain:
                        GainPhase gainPhase = _Phases[_ActualTurnPhase].gameObject.GetComponent<GainPhase>();

                        if (gainPhase != null)
                        {
                            Debug.Log($"Phase {_ActualTurnPhase} not null, run the phase");
                            gainPhase.RunPhase(playerRef);
                        
                        } else {
                            Debug.LogError("Cannot retreive the phase script :(");
                                    }
                        break;
                    case PhaseType.Action:
                        ActionPhase actionPhase = _Phases[_ActualTurnPhase].gameObject.GetComponent<ActionPhase>();

                        if (actionPhase != null)
                        {
                            Debug.Log($"Phase {_ActualTurnPhase} not null, run the phase");
                            actionPhase.RunPhase(playerRef);
                        
                        } else {
                            Debug.LogError("Cannot retreive the phase script :(");
                                            }
                        break;
                    case PhaseType.Resolution:
                        ResolutionPhase resolutionPhase = _Phases[_ActualTurnPhase].gameObject.GetComponent<ResolutionPhase>();

                        if (resolutionPhase != null)
                        {
                            Debug.Log($"Phase {_ActualTurnPhase} not null, run the phase");
                            resolutionPhase.RunPhase(playerRef);
                        
                        } else {
                            Debug.LogError("Cannot retreive the phase script :(");
                                                    }
                        break;
                    default:
                        break;
                }
                //_Phases[_ActualTurnPhase].RunPhase(playerRef);
            }
        }

        private void PhaseCompleted()
        {
            Debug.LogWarning($"PhaseCompleted PhaseType {Runner.IsServer} + {_ActualTurnPhase.ToString()}");

            if (!Runner.IsServer)
            {
                return;
            }

            // Check if the turn of the actual player is done
            if (_ActualTurnPhase == PhaseType.Resolution) {
                RPC_NextTurn();
                RPC_ProcessPhase();
            }
            else
            {
                Debug.Log($"Need to change phase, is runner is the server {Runner.IsServer}.");
                // A phase is completed, go on next one
                _ActualTurnPhase++;
                RPC_UpdatePhaseUI();
                RPC_ProcessPhase();
            }
        }

        #region RPC
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_InittializePhases()
        {
            // TODO: maybe do this in the game manager instead of here ?
            HUDManager.instance.SetPlayers(GameDataStorage.Instance.PlayerData);

            _Phases = new Dictionary<PhaseType, GameObject>();

            _Phases[PhaseType.Sacrifice] = PhaseList[0];
            _Phases[PhaseType.Gain] = PhaseList[1];
            _Phases[PhaseType.Action] = PhaseList[2];
            _Phases[PhaseType.Resolution] = PhaseList[3];


            // Dump the PhaseList
            //foreach (var phase in PhaseList)
            //{


            //Debug.Log($"Phase type -> {phase.PhaseType}");

            //}

            foreach (var phase in _Phases)
            {
                Debug.LogWarning("Add Listener for the endphase call by phases.");
                //phase.Value.OnEndPhase.AddListener(OnPhaseCompleted);
            }

            //// Set up Listener on events invoked by the HudManager
            SetUpEvents();
        }

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

        [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_PhaseCompleted()
        {
            Debug.LogWarning("RPC_PhaseCompleted CALLED");
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
            if (!Runner.IsServer || _IsPlaying || Runner.ActivePlayers.Count() < GameManager.Instance.Config.MinPlayers.Value)
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
            Debug.LogWarning("OnPhaseCompleted CALLED");
            
            // Call the EndPhase function of the actual phase

            RPC_PhaseCompleted();
        }
        #endregion
    }
}