using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Vermines.Menu.CustomLobby {

    using Vermines.Characters;
    using Vermines.Menu.Tavern;
    using Vermines.UI;
    using Vermines.Core.UI;
    using Vermines.Core.Scene;
    using Vermines.Extension;
    using Vermines.Core;

    public class NetworkCultistSelectDisplay : UIBehaviour {

        const string Ready   = "Unready";
        const string UnReady = "Ready";

        #region Attributes

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
        private TextMeshProUGUI _CultistNameText;

        private List<NetworkCultistSelectButton> _CultistButtons = new();

        [SerializeField]
        private TextMeshProUGUI _ReadyStateText;

        public UIButton ReadyButton;

        private SceneContext _Context;

        #endregion

        #region Methods

        public void Initialize(SceneContext context, bool hasInputAuthority)
        {
            _Context = context;

            ReadyButton.onClick.AddListener(OnReadyButton);

            if (hasInputAuthority) {
                Cultist[] allCultists = Global.Settings.Cultists.GetAllCultists();

                foreach (Cultist cultist in allCultists) {
                    NetworkCultistSelectButton selectedButtonInstance = Instantiate(_SelectButtonPrefab, _CultistHolder);

                    selectedButtonInstance.Initialize();
                    selectedButtonInstance.SetCharacter(this, cultist);

                    _CultistButtons.Add(selectedButtonInstance);
                }

                Clear();
            } else {
                _SelectPanel.SetActive(false);
                _CultistInfoFullPanel.SetActive(false);
            }
        }

        public void Deinitialize()
        {
            ReadyButton.onClick.RemoveListener(OnReadyButton);

            foreach (NetworkCultistSelectButton button in _CultistButtons)
                button.Deinitialize();
            Clear();
        }

        private void Clear()
        {
            _CultistInfoPanel.gameObject.SetActive(false);
            _CultistInfoPanel.SetCharacter(ScriptableObject.CreateInstance<Cultist>());

            foreach (NetworkCultistSelectButton button in _CultistButtons) {
                button.SetEnabled();
                button.UnSelect();
            }

            _ReadyStateText.SetTextSafe(UnReady);

            ReadyButton.interactable = false;
        }

        #endregion

        #region Methods

        public void HandleStatesChanged()
        {
            // Cultist Buttons are only filled if it's yours.
            // This line check if you are the local player.
            if (_CultistButtons.Count == 0)
                return;
            NetworkLobby   lobby = _Context.NetworkLobby;
            LobbyManager manager = _Context.Lobby;

            foreach (NetworkCultistSelectButton button in _CultistButtons) {
                if (manager.IsCultistTaken(button.Cultist.ID, false))
                    button.SetDisabled();
                else
                    button.SetEnabled();
            }

            List<LobbyPlayerController> players = _Context.Runner.GetAllBehaviours<LobbyPlayerController>();

            foreach (LobbyPlayerController player in players) {
                CultistSelectState state = player.State;

                if (state.ClientID != _Context.LocalPlayerRef)
                    continue;
                _ReadyStateText.SetTextSafe(state.IsLockedIn ? Ready : UnReady);

                if (state.CultistID > 0 && !state.IsLockedIn && manager.IsCultistTaken(state.CultistID, false)) {
                    _CultistButtons.ForEach(button => {
                        if (button.Cultist.ID == state.CultistID)
                            button.UnSelect();
                    });

                    Select(-1, true);

                    _CultistInfoPanel.SetCharacter(ScriptableObject.CreateInstance<Cultist>());
                    _CultistInfoPanel.gameObject.SetActive(false);
                } else if (IsLockdIn()) {
                    _CultistButtons.ForEach(button => {
                        if (button.Cultist.ID == state.CultistID)
                            button.SetDisabled();
                    });
                }
            }
        }

        public void Select(int cultistID, bool force = false)
        {
            List<LobbyPlayerController> players = _Context.Runner.GetAllBehaviours<LobbyPlayerController>();
            LobbyManager manager = _Context.Lobby;

            foreach (LobbyPlayerController player in players) {
                CultistSelectState state = player.State;

                if (state.ClientID != _Context.LocalPlayerRef)
                    continue;
                if (state.IsLockedIn && !force)
                    return;
                if (state.CultistID == cultistID)
                    return;
                if (manager.IsCultistTaken(cultistID, false) && !force)
                    return;
                if (state.CultistID > 0)
                    _CultistButtons.Find(button => button.Cultist.ID == state.CultistID).UnSelect();
            }

            if (Global.Settings.Cultists.IsValidCultistID(cultistID)) {
                Cultist cultist = Global.Settings.Cultists.GetCultistByID(cultistID);

                _CultistNameText.SetTextSafe(cultist.Name);
                _CultistInfoPanel.SetCharacter(cultist);
                _CultistInfoPanel.gameObject.SetActive(true);

                ReadyButton.interactable = true;
            } else
                ReadyButton.interactable = false;

            manager.RPC_Select(_Context.LocalPlayerRef, cultistID, force);
        }

        public void Select(Cultist cultist, bool force = false)
        {
            List<LobbyPlayerController> players = _Context.Runner.GetAllBehaviours<LobbyPlayerController>();
            LobbyManager manager = _Context.Lobby;

            foreach (LobbyPlayerController player in players) {
                CultistSelectState state = player.State;

                if (state.ClientID != _Context.LocalPlayerRef)
                    continue;
                if (state.IsLockedIn && !force)
                    return;
                if (state.CultistID == cultist.ID)
                    return;
                if (manager.IsCultistTaken(cultist.ID, false) && !force)
                    return;
                if (state.CultistID > 0)
                    _CultistButtons.Find(button => button.Cultist.ID == state.CultistID).UnSelect();
            }

            if (Global.Settings.Cultists.IsValidCultistID(cultist.ID)) {
                _CultistNameText.SetTextSafe(cultist.Name);
                _CultistInfoPanel.SetCharacter(cultist);
                _CultistInfoPanel.gameObject.SetActive(true);

                ReadyButton.interactable = true;
            } else
                ReadyButton.interactable = false;
            manager.RPC_Select(_Context.LocalPlayerRef, cultist.ID, force);
        }

        public void LockIn(bool isLockedIn)
        {
            LobbyManager lobby = _Context.Lobby;

            lobby.RPC_LockIn(_Context.LocalPlayerRef, isLockedIn);

            _ReadyStateText.text = isLockedIn ? Ready : UnReady;
        }

        public bool IsLockdIn()
        {
            List<LobbyPlayerController> players = _Context.Runner.GetAllBehaviours<LobbyPlayerController>();

            foreach (LobbyPlayerController player in players) {
                CultistSelectState state = player.State;

                if (state.ClientID != _Context.LocalPlayerRef)
                    continue;
                return state.IsLockedIn;
            }

            return false;
        }

        #endregion

        #region Events

        private void OnReadyButton()
        {
            List<LobbyPlayerController> players = _Context.Runner.GetAllBehaviours<LobbyPlayerController>();

            foreach (LobbyPlayerController player in players) {
                CultistSelectState state = player.State;

                if (state.ClientID == default || state.ClientID != _Context.LocalPlayerRef)
                    continue;
                LockIn(!state.IsLockedIn);
            }
        }

        #endregion
    }
}
