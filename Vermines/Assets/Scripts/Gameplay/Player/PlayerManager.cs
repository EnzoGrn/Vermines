using System.Collections.Generic;
using System;
using UnityEngine;
using Fusion;

namespace Vermines.Player {

    /// <summary>
    /// PlayerManager is a helper class
    /// </summary>
    static public class PlayerManager<T> where T : NetworkBehaviour {

        static readonly private List<PlayerRef> _TempSpawnPlayers   = new();
        static readonly private List<T>         _TempSpawnedPlayers = new();

        static private void Reset()
        {
            _TempSpawnPlayers.Clear();
            _TempSpawnedPlayers.Clear();
        }

        static public void UpdatePlayerConnections(NetworkRunner runner, Action<PlayerRef> spawnPlayer, Action<PlayerRef, T> despawnPlayer)
        {
            // 0. Clear all temporary lists.
            Reset();

            // 1. Get all connected players, marking them as pending spawn.
            _TempSpawnPlayers.AddRange(runner.ActivePlayers);

            // 2. Get all player objects with component of type T.
            runner.GetAllBehaviours(_TempSpawnedPlayers);

            for (int i = 0; i < _TempSpawnedPlayers.Count; i++) {
                T         player    = _TempSpawnedPlayers[i];
                PlayerRef playerRef = player.Object.InputAuthority;

                // 3. Remove PlayerRef of existing player object from pending spawn list.
                _TempSpawnPlayers.Remove(playerRef);

                // 4. If a player is not valid (disconnected) execute the despawn callback.
                if (runner.IsPlayerValid(playerRef) == false) {
                    try {
                        despawnPlayer(playerRef, player);
                    } catch (Exception exception) {
                        Debug.LogException(exception);
                    }
                }
            }

            // 5. Execute spawn callback for all players pending spawn (recently connected).
            for (int i = 0; i < _TempSpawnPlayers.Count; i++) {
                try {
                    spawnPlayer(_TempSpawnPlayers[i]);
                } catch (Exception exception) {
                    Debug.LogException(exception);
                }
            }

            // 6. Cleanup
            Reset();
        }
    }
}
