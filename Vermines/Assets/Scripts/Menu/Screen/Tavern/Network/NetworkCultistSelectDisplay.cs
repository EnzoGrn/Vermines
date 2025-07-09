using System.Collections.Generic;
using OMGG.Menu.Screen;
using UnityEngine;
using Fusion;
using TMPro;

namespace Vermines.Menu.Screen.Tavern.Network {
    using UnityEngine.UI;
    using Vermines.Characters;

    public class NetworkCultistSelectDisplay : NetworkBehaviour {

        const string Ready   = "Ready";
        const string UnReady = "Unready";

        #region Attributes

        [Header("Database")]

        [SerializeField]
        private CultistDatabase _CultistDatabase;

        [Header("UI Elements")]

        [SerializeField]
        private Transform _CultistHolder;

        [SerializeField]
        private NetworkCultistSelectButton _SelectButtonPrefab;

        [SerializeField]
        private GameObject _SelectPanel;

        [SerializeField]
        private GameObject _CultistInfoFullPanel;

        [SerializeField]
        private CultistInfoPanel _CultistInfoPanel;

        [SerializeField]
        private TMP_Text _CultistNameText;

        private List<NetworkCultistSelectButton> _CultistButtons = new();

        [SerializeField]
        private TMP_Text _ReadyStateText;

        public Button ReadyButton;

        #endregion

        #region Overrides Methods

        public override void Spawned()
        {
            if (HasInputAuthority) {
                Cultist[] allCultists = _CultistDatabase.GetAllCultists();

                foreach (Cultist cultist in allCultists) {
                    NetworkCultistSelectButton selectedButtonInstance = Instantiate(_SelectButtonPrefab, _CultistHolder);

                    selectedButtonInstance.SetCharacter(this, cultist);

                    _CultistButtons.Add(selectedButtonInstance);
                }

                Reset();

                CustomLobbyController controller = FindFirstObjectByType<CustomLobbyController>(FindObjectsInactive.Include);

                if (controller == null) {
                    OpenErrorPopup("Problem occurred while joining to find the lobby.", "Check your internet connection, disconnect and try again. If the problem persists, please contact the support team.");

                    return;
                }

                controller.RPC_OnPlayerConnected(Runner.LocalPlayer, PlayerPrefs.GetString("OMGG.Profile.Username"));
            } else {
                _SelectPanel.SetActive(false);
                _CultistInfoFullPanel.SetActive(false);
            }
        }

        #endregion

        #region Methods

        private void Reset()
        {
            _CultistInfoPanel.gameObject.SetActive(false);
            _CultistInfoPanel.SetCharacter(ScriptableObject.CreateInstance<Cultist>());

            foreach (NetworkCultistSelectButton button in _CultistButtons) {
                button.SetEnabled();
                button.UnSelect();
            }

            _ReadyStateText.text = UnReady;

            ReadyButton.interactable = false;
        }

        private void OpenErrorPopup(string title, string description)
        {
            var popup = FindAnyObjectByType<Popup>(FindObjectsInactive.Include);

            popup.OpenPopup(description, title);
        }

        public void HandleStatesChanged()
        {
            // Cultist Buttons are only filled if it's yours.
            // This line check if you are the local player.
            if (_CultistButtons.Count == 0)
                return;
            CustomLobbyController controller = FindFirstObjectByType<CustomLobbyController>(FindObjectsInactive.Include);

            foreach (NetworkCultistSelectButton button in _CultistButtons) {
                if (controller.IsCultistTaken(button.Cultist.ID, false))
                    button.SetDisabled();
                else
                    button.SetEnabled();
            }

            for (int i = 0; i < controller.Players.Length; i++) {
                CultistSelectState playerState = controller.Players.Get(i);

                if (playerState.ClientID != Runner.LocalPlayer)
                    continue;
                _ReadyStateText.text = playerState.IsLockedIn ? Ready : UnReady;

                if (playerState.CultistID != -1 && !playerState.IsLockedIn && controller.IsCultistTaken(playerState.CultistID, false)) {
                    _CultistButtons.ForEach(button => {
                        if (button.Cultist.ID == playerState.CultistID)
                            button.UnSelect();
                    });

                    Select(-1, true);

                    _CultistInfoPanel.SetCharacter(ScriptableObject.CreateInstance<Cultist>());
                    _CultistInfoPanel.gameObject.SetActive(false);
                } else if (IsLockdIn()) {
                    _CultistButtons.ForEach(button => {
                        if (button.Cultist.ID == playerState.CultistID)
                            button.SetDisabled();
                    });
                }
            }
        }

        public void Select(int cultistID, bool force = false)
        {
            CustomLobbyController controller = FindFirstObjectByType<CustomLobbyController>(FindObjectsInactive.Include);

            for (int i = 0; i < controller.Players.Length; i++) {
                CultistSelectState playerState = controller.Players.Get(i);

                if (playerState.ClientID != Runner.LocalPlayer)
                    continue;
                if (playerState.IsLockedIn && !force)
                    return;
                if (playerState.CultistID == cultistID)
                    return;
                if (controller.IsCultistTaken(cultistID, false) && !force)
                    return;
                if (playerState.CultistID != -1)
                    _CultistButtons.Find(button => button.Cultist.ID == playerState.CultistID).UnSelect();
            }

            if (_CultistDatabase.IsValidCultistID(cultistID)) {
                Cultist cultist = _CultistDatabase.GetCultistByID(cultistID);

                _CultistNameText.text = cultist.Name;

                _CultistInfoPanel.SetCharacter(cultist);
                _CultistInfoPanel.gameObject.SetActive(true);

                ReadyButton.interactable = true;
            } else
                ReadyButton.interactable = false;

            controller.RPC_Select(Runner.LocalPlayer, cultistID, force);
        }

        public void Select(Cultist cultist, bool force = false)
        {
            CustomLobbyController controller = FindFirstObjectByType<CustomLobbyController>(FindObjectsInactive.Include);

            for (int i = 0; i < controller.Players.Length; i++) {
                CultistSelectState playerState = controller.Players.Get(i);

                if (playerState.ClientID != Runner.LocalPlayer)
                    continue;
                if (playerState.IsLockedIn && !force)
                    return;
                if (playerState.CultistID == cultist.ID)
                    return;
                if (controller.IsCultistTaken(cultist.ID, false) && !force)
                        return;
                if (playerState.CultistID != -1)
                    _CultistButtons.Find(button => button.Cultist.ID == playerState.CultistID).UnSelect();
            }

            if (_CultistDatabase.IsValidCultistID(cultist.ID)) {
                _CultistNameText.text = cultist.Name;

                _CultistInfoPanel.SetCharacter(cultist);
                _CultistInfoPanel.gameObject.SetActive(true);

                ReadyButton.interactable = true;
            } else
                ReadyButton.interactable = false;

            controller.RPC_Select(Runner.LocalPlayer, cultist.ID, force);
        }

        public void LockIn(bool isLockedIn)
        {
            CustomLobbyController controller = FindFirstObjectByType<CustomLobbyController>(FindObjectsInactive.Include);

            controller.RPC_LockIn(Runner.LocalPlayer, isLockedIn);

            _ReadyStateText.text = isLockedIn ? Ready : UnReady;
        }

        public bool IsLockdIn()
        {
            CustomLobbyController controller = FindFirstObjectByType<CustomLobbyController>(FindObjectsInactive.Include);

            for (int i = 0; i < controller.Players.Length; i++) {
                CultistSelectState playerState = controller.Players.Get(i);

                if (playerState.ClientID != Runner.LocalPlayer)
                    continue;
                return playerState.IsLockedIn;
            }

            return false;
        }

        #endregion

        #region Events

        public void OnLockInPressed()
        {
            if (Runner == null || !Runner.IsRunning || Runner.LocalPlayer == default)
                return;
            CustomLobbyController controller = FindFirstObjectByType<CustomLobbyController>(FindObjectsInactive.Include);

            if (controller == null)
                return;
            for (int i = 0; i < controller.Players.Length; i++) {
                CultistSelectState playerState = controller.Players.Get(i);

                if (Runner == null)
                    return;
                if (playerState.ClientID == default)
                    continue;
                if (playerState.ClientID != Runner.LocalPlayer)
                    continue;
                LockIn(!playerState.IsLockedIn);
            }
        }

        #endregion
    }
}
