using System.Threading.Tasks;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using WebSocketSharp;
using UnityEngine;

namespace Vermines.Core.Services {

    using Vermines.Core.Player;
    using Vermines.Utils;

    public class PlayerService : IGlobalService {

        #region Attributes

        public Action<PlayerData> PlayerDataChanged;

        public PlayerData PlayerData { get; private set; }

        #endregion

        #region Initialize

        async void IGlobalService.Initialize()
        {
            PlayerData = LoadPlayer();

            try {
                PlayerData.UnityID = await GetUnityID();
            } catch (Exception exception) {
                PlayerData.UnityID = default;

                Debug.LogException(exception);
                Debug.LogWarning("Exception raised when initializing Unity Services. Please check if a Unity Project ID is linked in project settings.");
            }

            PlayerData.Lock();

            SavePlayer();
        }

        void IGlobalService.Deinitialize()
        {
            PlayerData.Unlock();

            SavePlayer();

            PlayerDataChanged = null;
        }

        #endregion

        #region Getters & Setters

        private string GetUserID()
        {
            string userID = SystemInfo.deviceUniqueIdentifier;

            #if UNITY_EDITOR
                userID = $"{userID}_{Application.dataPath.GetHashCode()}";
            #endif

            return userID;
        }

        private async Task<string> GetUnityID()
        {
            #if UNITY_EDITOR
                if (UnityEditor.CloudProjectSettings.projectId.IsNullOrEmpty())
                        return default;
            #endif

            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsAuthorized) {
                AuthenticationService.Instance.ClearSessionToken();

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            return AuthenticationService.Instance.PlayerId;
        }

        #endregion

        #region Methods

        public void Tick()
        {
            if (PlayerData.IsDirty == true) {
                SavePlayer();

                PlayerData.ClearDirty();

                PlayerDataChanged?.Invoke(PlayerData);
            }
        }

        private PlayerData LoadPlayer()
        {
            string baseUserID = GetUserID();
            string userID     = baseUserID;

            PlayerData playerData = PersistentStorage.GetObject<PlayerData>($"PlayerData-{userID}");

            if (!Application.isMobilePlatform || Application.isEditor) {
                int clientIndex = 1;

                // We are probably running multiple clients, let's create unique player data for each one.
                while (playerData != null && playerData.IsLocked()) {
                    userID     = $"{baseUserID}.{clientIndex}";
                    playerData = PersistentStorage.GetObject<PlayerData>($"PlayerData-{userID}");

                    clientIndex++;
                }
            }
            playerData ??= new(userID);

            return playerData;
        }

        private void SavePlayer()
        {
            PersistentStorage.SetObject($"PlayerData-{PlayerData.UserID}", PlayerData, true);
        }

        #endregion
    }
}
