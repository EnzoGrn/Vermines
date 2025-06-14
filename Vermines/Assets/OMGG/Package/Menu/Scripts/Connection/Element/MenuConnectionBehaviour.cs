using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine.Events;
using Fusion;

namespace OMGG.Menu.Connection.Element {

    using OMGG.Menu.Connection.Data;
    using OMGG.Menu.Region;
    using OMGG.Scene.Data;

    public abstract partial class MenuConnectionBehaviour : FusionMonoBehaviour {

        #region Attributes

        /// <summary>
        /// Access the session name / Photon room name.
        /// </summary>
        public abstract string SessionName { get; }

        /// <summary>
        /// Access the custom lobby name.
        /// </summary>
        public abstract string LobbyCustomName { get; }

        /// <summary>
        /// Access the max player count.
        /// </summary>
        public abstract int MaxPlayerCount { get; }

        /// <summary>
        /// Access the actual region connected to.
        /// </summary>
        public abstract string Region { get; }

        /// <summary>
        /// Access the AppVersion used.
        /// </summary>
        public abstract string AppVersion { get; }

        /// <summary>
        /// Get a list of usernames that are inside this session.
        /// </summary>
        public abstract List<string> Usernames { get; }

        /// <summary>
        /// Is connection alive.
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        /// Get current connection ping.
        /// </summary>
        public abstract int Ping { get; }

        #endregion

        #region Events

        /// <summary>
        /// A shortcut to inject any code to change the connection args before the connection is started.
        /// </summary>
        public UnityEvent<ConnectionArgs> OnBeforeConnect;

        /// <summary>
        /// A shortcut to easily get notified about an impending disconnection.
        /// </summary>
        public UnityEvent<int> OnBeforeDisconnect;

        /// <summary>
        /// A shortcut to easily get notified about the connection progress.
        /// </summary>
        public UnityEvent<string> OnProgress;

        #endregion

        #region Methods

        /// <summary>
        /// Connect using <see cref="ConnectionArgs"/>.
        /// </summary>
        /// <param name="connectionArgs">Connection arguments.</param>
        /// <returns>When the connection is established</returns>
        public virtual async Task<ConnectResult> ConnectAsync(ConnectionArgs connectionArgs, SceneRef sceneRef, bool isCustom = false)
        {
            if (OnBeforeConnect != null) {
                try {
                    OnBeforeConnect.Invoke(connectionArgs);
                } catch (Exception e) {
                    UnityEngine.Debug.LogException(e);

                    return new ConnectResult() {
                        FailReason = ConnectFailReason.Disconnect,
                        DebugMessage = e.Message
                    };
                }
            }

            return await ConnectAsyncInternal(connectionArgs, sceneRef, isCustom);
        }

        public virtual async Task<ConnectResult> ChangeScene(SceneInformation sceneInfo, List<SceneInformation> sceneInfos)
        {
            if (sceneInfo.SceneName == null) {
                return new ConnectResult() {
                    FailReason = ConnectFailReason.ArgumentError,
                    DebugMessage = "Scene name is null"
                };
            }

            return await ChangeSceneInternal(sceneInfo, sceneInfos);
        }

        public virtual async Task<ConnectResult> ChangeScene(SceneRef sceneRef)
        {
            if (sceneRef == null || !sceneRef.IsValid) {
                return new ConnectResult() {
                    FailReason   = ConnectFailReason.ArgumentError,
                    DebugMessage = "Scene reference is null or invalid"
                };
            }

            return await ChangeSceneInternal(sceneRef);
        }

        public virtual async Task<bool> UnloadScene(SceneRef sceneRef)
        {
            if (sceneRef == null || !sceneRef.IsValid)
                return false;
            return await UnloadSceneInternal(sceneRef);
        }

        /// <summary>
        /// Disconnect the current connection.
        /// </summary>
        /// <param name="reason">The disconnect reason <see cref="ConnectFailReason"/></param>
        /// <returns></returns>
        public virtual async Task DisconnectAsync(int reason)
        {
            if (OnBeforeDisconnect != null) {
                try {
                    OnBeforeDisconnect.Invoke(reason);
                } catch (Exception e) {
                    UnityEngine.Debug.LogException(e);
                }
            }

            await DisconnectAsyncInternal(reason);
        }

        /// <summary>
        /// Requests a list of available regions from the name server.
        /// </summary>
        /// <param name="connectionArgs">Connection arguments</param>
        /// <returns>List of available region configured in the dashboard for this app.</returns>
        public abstract Task<List<OnlineRegion>> RequestAvailableOnlineRegionsAsync(ConnectionArgs connectionArgs);

        /// <summary>
        /// The connection task.
        /// </summary>
        /// <param name="connectArgs">Connection args.</param>
        /// <param name="sceneRef">The scene reference to connect to.</param>
        /// <param name="isCustom">Is this a custom game?</param>
        /// <returns>When the connection is established and the game ready.</returns>
        protected abstract Task<ConnectResult> ConnectAsyncInternal(ConnectionArgs connectArgs, SceneRef sceneRef, bool isCustom);

        /// <summary>
        /// The scene change task.
        /// </summary>
        /// <param name="sceneInfo">Scene info.</param>
        /// <param name="sceneInfos">All scene infos (in case of random).</param>
        /// <returns>When the scene change is completed.</returns>
        protected abstract Task<ConnectResult> ChangeSceneInternal(SceneInformation sceneInfo, List<SceneInformation> sceneInfos);
        protected abstract Task<ConnectResult> ChangeSceneInternal(SceneRef sceneRef);

        /// <summary>
        /// Unload the scene task.
        /// </summary>
        /// <param name="sceneRef">The scene reference to unload.</param>
        /// <returns>When the scene is unload.</returns>
        protected abstract Task<bool> UnloadSceneInternal(SceneRef sceneRef);

        /// <summary>
        /// Disconnect task.
        /// </summary>
        /// <param name="reason">Disconnect reason <see cref="ConnectFailReason"/></param>
        /// <returns>When the connection has terminated gracefully.</returns>
        protected abstract Task DisconnectAsyncInternal(int reason);

        protected void ReportProgress(string message)
        {
            OnProgress?.Invoke(message);
        }

        #endregion
    }
}
