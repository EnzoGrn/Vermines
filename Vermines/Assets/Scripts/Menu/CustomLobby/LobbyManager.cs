using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Fusion;
using System;

namespace Vermines.Menu.CustomLobby {

    using Vermines.Characters;
    using Vermines.Core;
    using Vermines.Core.Network;

    public class LobbyManager : ContextBehaviour {

        #region Attributes

        public CultistDatabase CultistDatabase;

        public Action<PlayerRef> OnPlayerJoinedGame;

        #endregion

        #region Methods

        public override void Spawned()
        {
            Context.Lobby = this;
        }

        public void PlayerJoined(LobbyPlayerController p)
        {
            CultistSelectState state = p.State;

            p.UpdateState(state);

            RPC_PlayerJoinedGame(p.Object.InputAuthority);
        }

        public void PlayerLeft(LobbyPlayerController p)
        {
            List<LobbyPlayerController> players = Context.Runner.GetAllBehaviours<LobbyPlayerController>();

            // Un-ready everyone if a player leaves
            foreach (LobbyPlayerController player in players) {
                CultistSelectState state = player.State;

                state.IsLockedIn = false;

                player.UpdateState(state);
            }
        }

        public bool IsCultistTaken(int cultistID, bool checkAll)
        {
            List<LobbyPlayerController> players = Context.Runner.GetAllBehaviours<LobbyPlayerController>();

            foreach (LobbyPlayerController player in players) {
                CultistSelectState state = player.State;

                if (!checkAll && state.ClientID == Context.Runner.LocalPlayer)
                    continue;
                if (state.IsLockedIn && state.CultistID == cultistID)
                    return true;
            }

            return false;
        }

        public void StopGame()
        {
            if (!HasStateAuthority) {
                Global.Networking.StopGame();

                return;
            }

            StartCoroutine(StopGameCoroutine());
        }

        private IEnumerator StopGameCoroutine()
        {
            RPC_StopGame();

            yield return new WaitForSecondsRealtime(0.25f);

            Global.Networking.StopGame();
        }

        #endregion

        #region Methods

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_PlayerJoinedGame(PlayerRef playerRef)
        {
            OnPlayerJoinedGame?.Invoke(playerRef);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies, Channel = RpcChannel.Reliable)]
        private void RPC_StopGame()
        {
            Global.Networking.StopGame(Networking.STATUS_SERVER_CLOSED);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_Select(PlayerRef playerRef, int cultistID, bool force = false)
        {
            List<LobbyPlayerController> players = Context.Runner.GetAllBehaviours<LobbyPlayerController>();

            foreach (LobbyPlayerController player in players) {
                if (player.State.ClientID != playerRef)
                    continue;
                if (!CultistDatabase.IsValidCultistID(cultistID) && !force)
                    return;
                if (IsCultistTaken(cultistID, true) && !force)
                    return;
                player.UpdateState(new CultistSelectState(playerRef, cultistID, player.State.IsLockedIn));
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public async void RPC_LockIn(PlayerRef playerRef, bool isLockedIn = true)
        {
            List<LobbyPlayerController> players = Context.Runner.GetAllBehaviours<LobbyPlayerController>();

            foreach (LobbyPlayerController player in players) {
                CultistSelectState state = player.State;

                if (state.ClientID != playerRef)
                    continue;
                if (isLockedIn) {
                    if (!CultistDatabase.IsValidCultistID(state.CultistID))
                        return;
                    if (IsCultistTaken(state.CultistID, true))
                        return;
                }

                player.UpdateState(new CultistSelectState(playerRef, state.CultistID, isLockedIn));
            }

            Debug.Log("TODO: Start the game by calling the Global.Networking");
        }

        #endregion
    }
}
