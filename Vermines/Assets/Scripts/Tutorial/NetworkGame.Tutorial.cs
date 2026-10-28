using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Core {

    using Vermines.Player;

    public partial class NetworkGame {

        /// <summary>
        /// Scripted opponents spawned for the current lesson, by the PlayerRef they were given.
        /// </summary>
        private readonly Dictionary<PlayerRef, PlayerController> _TutorialPlayers = new();

        /// <summary>
        /// Opponents already reported as lost, so the Console gets one message per problem.
        /// </summary>
        private readonly HashSet<PlayerRef> _ReportedTutorialPlayers = new();

        /// <summary>
        /// Tutorial only (state authority): spawns the PlayerController of a scripted opponent.
        /// A tutorial match runs offline, so the opponent has no client behind its PlayerRef, the tutorial bridge plays its turns on the host.
        /// </summary>
        public PlayerController SpawnTutorialPlayer(PlayerRef playerRef)
        {
            if (!HasStateAuthority || _PlayerPrefab == null)
                return null;
            PlayerController player = Runner.Spawn(_PlayerPrefab, inputAuthority: playerRef);

            if (player != null)
                _TutorialPlayers[playerRef] = player;

            #if UNITY_EDITOR
                if (player != null)
                    player.gameObject.name = $"Player Tutorial {playerRef}";
            #endif

            return player;
        }

        /// <summary>
        /// True for a scripted opponent: it has no client, so it is never in Runner.ActivePlayers,
        /// and ResyncExistingPlayers (run by Activate, after the lesson spawned it) must keep it.
        /// </summary>
        private bool IsTutorialPlayer(PlayerController player)
        {
            return player != null && _TutorialPlayers.ContainsValue(player);
        }

        /// <summary>
        /// Tutorial only: checks that every scripted opponent still exists and still has its PlayerRef.
        /// Called every frame by the tutorial bridge, so the Console says when an opponent was lost.
        /// </summary>
        public void CheckTutorialPlayers()
        {
            if (_TutorialPlayers.Count == 0)
                return;
            List<PlayerRef> refs = new(_TutorialPlayers.Keys);

            foreach (PlayerRef playerRef in refs)
                FindTutorialPlayer(playerRef);
        }

        /// <summary>
        /// Tutorial only: the scripted opponent of this PlayerRef, when the lookup by input authority misses it.
        /// If it lost its PlayerRef, the host gives it back (the opponent has no client, so nobody else should own it).
        /// </summary>
        private PlayerController FindTutorialPlayer(PlayerRef playerRef)
        {
            if (!_TutorialPlayers.TryGetValue(playerRef, out PlayerController player))
                return default;
            if (player == null || player.Object == null || !player.Object.IsValid) {
                _TutorialPlayers.Remove(playerRef);

                Debug.LogError($"[Tutorial] The scripted opponent {playerRef} was despawned (frame {Time.frameCount}, tick {Runner.Tick}).");

                return default;
            }

            PlayerRef current = player.Object.InputAuthority;

            if (current == playerRef)
                return player;
            if (_ReportedTutorialPlayers.Add(playerRef))
                Debug.LogError($"[Tutorial] The scripted opponent {playerRef} lost its input authority: now {current} (frame {Time.frameCount}, tick {Runner.Tick}, topology {Runner.Config.Simulation.Topology}).");
            if (!HasStateAuthority)
                return player;
            if (Runner.Config.Simulation.Topology != Topologies.ClientServer)
                Runner.Config.Simulation.Topology = Topologies.ClientServer;
            player.Object.AssignInputAuthority(playerRef);

            return player;
        }
    }
}
