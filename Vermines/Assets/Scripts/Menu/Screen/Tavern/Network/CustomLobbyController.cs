using OMGG.Menu.Connection;
using UnityEngine;
using Fusion;

namespace Vermines.Menu.Screen.Tavern.Network {

    using Vermines.Menu.Connection.Element;
    using Vermines.Characters;
    using Vermines.Service;
    using Vermines.Configuration.Network;
    using System.Threading.Tasks;
    using System.Collections;

    [RequireComponent(typeof(SettingsManager))]
    public class CustomLobbyController : NetworkBehaviour, IPlayerLeft {

        #region Player Selection

        [SerializeField]
        [Tooltip("Prefab reference for the player selection controller. An object that will be instantiated to manage the player selection in a custom game.")]
        private NetworkPrefabRef _PlayerSelectionController;

        [SerializeField]
        [Tooltip("All the player card (already instantiated) that will be used to display the players in the lobby.")]
        private PlayerCard[] _PlayerCards;

        [Networked, Capacity(4), OnChangedRender(nameof(OnPlayerStatesChanged))]
        public NetworkArray<CultistSelectState> Players { get; }

        [SerializeField]
        private CultistDatabase _CultistDatabase;

        #endregion

        #region Rules Configuration

        private Coroutine _HideCoroutine;

        [SerializeField]
        private Animator _BookAnimator;

        [SerializeField]
        private GameObject _RulesPanels;

        [SerializeField]
        private GameObject _Content;

        #endregion

        #region Game

        [SerializeField]
        [Tooltip("This scene must be the game scene. It will be loaded when all players are ready.")]
        private SceneRef SceneToLoad;

        [SerializeField]
        [Tooltip("This scene must be the lobby scene.")]
        private SceneRef SceneToUnload;

        #endregion

        #region Methods

        private void DisconnectPlayer(PlayerRef playerRef)
        {
            if (Runner.TryGetPlayerObject(playerRef, out NetworkObject player))
                Runner.Despawn(player);
        }

        public bool IsCultistTaken(int cultistID, bool checkAll)
        {
            for (int i = 0; i < Players.Length; i++) {
                CultistSelectState state = Players.Get(i);

                if (!checkAll && state.ClientID == Runner.LocalPlayer)
                    continue;
                if (state.IsLockedIn && state.CultistID == cultistID)
                    return true;
            }

            return false;
        }

        private async Task StartGame()
        {
            // Check if everyone is locked in
            foreach (CultistSelectState state in Players)
                if (!state.IsLockedIn && state.ClientID != default(PlayerRef))
                    return;
            // -- Change to loading view
            VMUI_Loading loading = FindFirstObjectByType<VMUI_Loading>(FindObjectsInactive.Include);

            loading.Controller.Show<VMUI_Loading>();

            // -- Hide the session info
            Runner.SessionInfo.IsVisible = false;

            // -- Load the Game Scene
            VerminesConnectionBehaviour connection = FindFirstObjectByType<VerminesConnectionBehaviour>(FindObjectsInactive.Include);

            for (int i = 0; i < Players.Length; i++) {
                CultistSelectState playerState = Players.Get(i);

                if (playerState.ClientID == default(PlayerRef))
                    continue;
                // -- Remove player's network object
                Runner.Despawn(Runner.GetPlayerObject(playerState.ClientID));
            }

            ConnectResult result = await connection.ChangeScene(SceneToLoad);

            if (result.Success) {
                // -- Update the player states in the VerminesPlayerService
                VerminesPlayerService services = FindFirstObjectByType<VerminesPlayerService>(FindObjectsInactive.Include);

                for (int i = 0; i < Players.Length; i++) {
                    CultistSelectState playerState = Players.Get(i);

                    if (playerState.ClientID == default(PlayerRef))
                        continue;
                    Cultist cultist = _CultistDatabase.GetCultistByID(playerState.CultistID);

                    services.UpdatePlayerState(playerState.ClientID, playerState.Name.Value, cultist.family);
                }

                GameManager manager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);

                if (manager != null) {
                    SettingsManager settingsManager = GetComponent<SettingsManager>();

                    manager.SettingsData = settingsManager.NetworkConfig;
                }

                // -- Unload the lobby scene
                await connection.UnloadScene(SceneToUnload);

                // -- Change UI to gameplay
                loading.Controller.Show<VMUI_Gameplay>(loading);
            } else {
                Debug.LogError($"[CustomLobbyController] RPC_LockIn() - Failed to change scene: {result.DebugMessage}");

                loading.Controller.Show<VMUI_CustomTavern>();
            }
        }

        private IEnumerator ShowCoroutine()
        {
            if (_HideCoroutine != null) {
                if (_BookAnimator.gameObject.activeInHierarchy && _BookAnimator.HasState(0, Animator.StringToHash("Open")))
                    _BookAnimator.Play(Animator.StringToHash("Show"), 0, 0);
                yield return _BookAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            }

            yield break;
        }

        private IEnumerator HideAnimCoroutine()
        {
            _BookAnimator.Play(Animator.StringToHash("Hide"), 0, 0);

            yield return null;

            while (_BookAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
            _RulesPanels.SetActive(false);
        }

        private IEnumerator HideCoroutine()
        {
            if (_BookAnimator && gameObject.activeSelf) {
                if (_HideCoroutine != null)
                    StopCoroutine(_HideCoroutine);
                _HideCoroutine = StartCoroutine(HideAnimCoroutine());

                yield return _HideCoroutine;

                _HideCoroutine = null;
            }

            yield break;
        }

        #endregion

        #region Overrides Methods

        public override void Spawned()
        {
            Runner.SessionInfo.IsVisible = true;

            base.Spawned();

            RPC_AddPlayer(Runner.LocalPlayer);

            OnPlayerStatesChanged();

            _RulesPanels.SetActive(false);
            _Content.SetActive(false);
        }

        public override void Despawned(NetworkRunner runner, bool hasState) {}

        #endregion

        #region Events

        public void PlayerLeft(PlayerRef player)
        {
            for (int i = 0; i < Players.Length; i++) {
                if (Players.Get(i).ClientID != player)
                    continue;
                for (int j = i; j < Players.Length - 1; j++)
                    Players.Set(j, Players.Get(j + 1));
                Players.Set(Players.Length - 1, default(CultistSelectState));
            }

            DisconnectPlayer(player);

            // Un-ready everyone if a player leaves
            for (int i = 0; i < Players.Length; i++) {
                CultistSelectState state = Players.Get(i);

                state.IsLockedIn = false;

                Players.Set(i, state);
            }
        }

        private void OnPlayerStatesChanged()
        {
            for (int i = 0; i < _PlayerCards.Length; i++) {
                CultistSelectState state = Players.Get(i);

                if (state.ClientID != default(PlayerRef))
                    _PlayerCards[i].UpdateDisplay(state, state.ClientID == Runner.LocalPlayer);
                else
                    _PlayerCards[i].DisableDisplay();
            }

            foreach (NetworkCultistSelectDisplay display in FindObjectsByType<NetworkCultistSelectDisplay>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                display.HandleStatesChanged();
        }

        private async void OnRulesButtonOpenedPressed()
        {
            StartCoroutine(ShowCoroutine());

            _RulesPanels.SetActive(true);

            AnimatorStateInfo stateInfo = _BookAnimator.GetCurrentAnimatorStateInfo(0);

            float duration = stateInfo.length;

            await Task.Delay((int)(duration * 1000));

            _Content.SetActive(true);
        }

        private void OnRulesButtonClosedPressed()
        {
            _Content.SetActive(false);

            StartCoroutine(HideCoroutine());
        }

        #endregion

        #region RPCs

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddPlayer(PlayerRef player)
        {
            NetworkObject playerSelectionController = Runner.Spawn(_PlayerSelectionController, inputAuthority: player);

            Runner.SetPlayerObject(player, playerSelectionController);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_OnPlayerConnected(PlayerRef playerRef, string playerName)
        {
            for (int i = 0; i < Players.Length; i++) {
                if (Players.Get(i).Equals(default(CultistSelectState))) {
                    Players.Set(i, new CultistSelectState(playerRef, playerName));

                    return;
                }
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_Select(PlayerRef player, int cultistID, bool force = false)
        {
            for (int i = 0; i < Players.Length; i++) {
                if (Players.Get(i).ClientID != player)
                    continue;
                if (!_CultistDatabase.IsValidCultistID(cultistID) && !force)
                    return;
                if (IsCultistTaken(cultistID, true) && !force)
                    return;
                Players.Set(i, new CultistSelectState(player, Players.Get(i).Name.Value, cultistID, Players.Get(i).IsLockedIn));
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public async void RPC_LockIn(PlayerRef player, bool isLockedIn = true)
        {
            for (int i = 0; i < Players.Length; i++) {
                CultistSelectState state = Players.Get(i);

                if (state.ClientID != player)
                    continue;
                if (isLockedIn) {
                    if (!_CultistDatabase.IsValidCultistID(state.CultistID))
                        return;
                    if (IsCultistTaken(state.CultistID, true))
                        return;
                }

                Players.Set(i, new CultistSelectState(player, state.Name.Value, state.CultistID, isLockedIn));
            }

            if (isLockedIn)
                await StartGame();
        }

        #endregion
    }
}
