using Fusion;
using Vermines.Core.Scene;

namespace Vermines.Core.Network {

    using UnityScene = UnityEngine.SceneManagement.Scene;

    public sealed class GamePeer {

        #region Attributes

        public int    ID;
        public string UserID;

        public NetworkRunner       Runner;
        public GameMode            GameMode;
        public SessionRequest      Request;
        public NetworkSceneInfo    Scene;
        public UnityScene          LoadedScene;
        public NetworkSceneManager SceneManager;
        public SceneContext        Context;

        public int ConnectionTries   = 3;
        public int ReconnectionTries = 1;

        #endregion

        #region Constructor

        public GamePeer(int id)
        {
            ID = id;
        }

        #endregion

        public bool Loaded;
        public bool WasConnected;
        public bool CanConnect => WasConnected ? ReconnectionTries > 0 : ConnectionTries > 0;

        public bool IsConnected
        {
            get
            {
                if (Runner == null)
                    return false;
                if (Request.GameMode == GameMode.Single)
                    return true;
                if (!Runner.IsCloudReady || !Runner.IsRunning)
                    return false;
                return GameMode == GameMode.Client ? Runner.IsConnectedToServer : true;
            }
        }
    }
}
