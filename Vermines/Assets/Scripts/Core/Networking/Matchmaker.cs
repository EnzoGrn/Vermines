using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Fusion;

namespace Vermines.Core.Network {
    
    using Vermines.Core.Player;
    using Vermines.Core.Scene;

    public sealed class Matchmaker : SceneService {

        /*#region Attributes

        public event Action<string> OnMatchFound;
        public event Action<string> OnMatchmakerFailed;

        private static string _CachedJoinCode;

        public string JoinCode => _JoinCode;
        private string _JoinCode;

        public bool IsHosting => _IsHosting;
        private bool _IsHosting;

        private static readonly List<string> _AvailableJoinCodes = new();

        #endregion*/

        /*#region Methods

        public void CancelMatchmaking()
        {
            _JoinCode = null;
        }

        public async Task QuickPlay(string scenePath, GameplayType gameplayType = GameplayType.Standart, int maxPlayers = 4)
        {
            try {
                // On essaye d’abord de rejoindre une partie disponible
                if (_AvailableJoinCodes.Count > 0) {
                    string codeToJoin = _AvailableJoinCodes[0];

                    await JoinMatch(codeToJoin, scenePath, gameplayType);

                    _AvailableJoinCodes.Remove(codeToJoin);

                    return;
                }

                // Sinon on devient Host
                await HostMatch(scenePath, gameplayType, maxPlayers);

                // On garde le code pour les futurs joueurs
                _AvailableJoinCodes.Add(_JoinCode);
            } catch (Exception e) {
                OnMatchmakerFailed?.Invoke($"Erreur QuickPlay : {e.Message}");
            }
        }

        public async Task HostMatch(string scenePath, GameplayType gameplayType = GameplayType.Standart, int maxPlayers = 4)
        {
            try {
                PlayerData user = Global.PlayerService.PlayerData;

                _IsHosting = true;

                var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

                _JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                SessionRequest request = new() {
                    UserID       = user.UnityID,
                    DisplayName  = user.Nickname,
                    GameMode     = GameMode.Host,
                    GameplayType = gameplayType,
                    SessionName  = $"Session_{user.Nickname}",
                    ScenePath    = scenePath,
                    CustomLobby  = _JoinCode,
                    MaxPlayers   = maxPlayers,
                    IPAddress    = allocation.RelayServer.IpV4,
                    Port         = (ushort)allocation.RelayServer.Port
                };

                Global.Networking.StartGame(request);

                OnMatchFound?.Invoke(_JoinCode);
            } catch (Exception e) {
                OnMatchmakerFailed?.Invoke(e.Message);
            }
        }

        public async Task JoinMatch(string joinCode, string scenePath, GameplayType gameplayType = GameplayType.Standart)
        {
            try {
                PlayerData user = Global.PlayerService.PlayerData;

                _IsHosting = false;

                var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                SessionRequest request = new() {
                    UserID       = user.UnityID,
                    DisplayName  = user.Nickname,
                    GameMode     = GameMode.Client,
                    GameplayType = gameplayType,
                    SessionName  = $"Join_{user.Nickname}",
                    ScenePath    = scenePath,
                    CustomLobby  = joinCode,
                    IPAddress    = allocation.RelayServer.IpV4,
                    Port         = (ushort)allocation.RelayServer.Port
                };

                Global.Networking.StartGame(request);

                OnMatchFound?.Invoke(joinCode);
            } catch (Exception e) {
                OnMatchmakerFailed?.Invoke(e.Message);
            }
        }

        #endregion

        #region Events

        protected override void OnDeinitialize()
        {
            CancelMatchmaking();

            base.OnDeinitialize();
        }

        #endregion*/
    }
}
