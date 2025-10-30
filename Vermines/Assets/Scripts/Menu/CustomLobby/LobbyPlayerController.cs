using Fusion;

namespace Vermines.Menu.CustomLobby {

    using Vermines.Core;
    using Vermines.Core.Player;
    using Vermines.Core.Scene;

    public partial class LobbyPlayerController : NetworkBehaviour, IPlayer, IContextBehaviour {

        #region Player's value

        public string UserID { get; private set; }
        public string UnityID { get; private set; }
        public string Nickname { get; private set; }

        public SceneContext Context { get; set; }

        [Networked]
        private NetworkString<_64> NetworkedUserID { get; set; }

        [Networked]
        private NetworkString<_32> NetworkedNickname { get; set; }

        [Networked]
        public CultistSelectState State { get; private set; }

        public NetworkCultistSelectDisplay UI;

        [Networked]
        private byte SyncToken { get; set; }

        private byte _LocalSyncToken;

        private bool _PlayerDataSent;

        #endregion

        #region Methods

        public void UpdateState(CultistSelectState state)
        {
            State = state;

            LobbyUIView view = Context.UI.Get<LobbyUIView>();

            if (view != null)
                view.OnPlayerStatesChanged();
        }

        private void UpdateLocalState()
        {
            if (_LocalSyncToken != SyncToken) {
                _LocalSyncToken = SyncToken;

                UserID   = NetworkedUserID.Value;
                Nickname = NetworkedNickname.Value;
            }
        }

        #endregion

        #region NetworkBehaviour

        public override void Spawned()
        {
            _LocalSyncToken = default;
            _PlayerDataSent = false;

            if (HasInputAuthority)
                Context.LocalPlayerRef = Object.InputAuthority;
            UI.Initialize(Context, Object.HasInputAuthority);
            UpdateLocalState();

            Runner.SetIsSimulated(Object, true);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            UI.Deinitialize();

            base.Despawned(runner, hasState);
        }

        public override void FixedUpdateNetwork()
        {
            UpdateLocalState();

            if (HasInputAuthority) {
                if (!_PlayerDataSent && Runner.IsForward && Context.PlayerData != null) {
                    string unityID = Context.PlayerData.UnityID ?? string.Empty;

                    RPC_SendPlayerData(Context.PeerUserID, Context.PlayerData.Nickname, unityID);

                    _PlayerDataSent = true;
                }
            }
        }

        #endregion

        #region RPC

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_SendPlayerData(string userId, string nickname, string unityId)
        {
            #if UNITY_EDITOR
                nickname += $" {Object.InputAuthority}";
            #endif

            SyncToken++;

            if (SyncToken == default)
                SyncToken = 1;
            _LocalSyncToken = SyncToken;

            UserID            = userId;
            Nickname          = nickname;
            NetworkedUserID   = userId;
            NetworkedNickname = nickname;
            UnityID           = unityId;
        }

        #endregion
    }
}
