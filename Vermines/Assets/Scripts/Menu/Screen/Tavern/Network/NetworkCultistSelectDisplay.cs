using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

namespace Vermines.Menu.Screen.Tavern.Network {

    using Vermines.Characters;

    public class NetworkCultistSelectDisplay : NetworkBehaviour, IPlayerJoined, IPlayerLeft {

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
        private PlayerCard[] _PlayerCards;

        [SerializeField]
        private GameObject _CultistInfoPanel;

        [SerializeField]
        private TMP_Text _CultistNameText;

        private List<NetworkCultistSelectButton> _CultistButtons = new();

        [Networked, Capacity(4), OnChangedRender(nameof(HandlePlayerStatesChanged))]
        private NetworkArray<CultistSelectState> _Players { get; }

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
            }

            if (HasStateAuthority) {
                foreach (PlayerRef client in Runner.ActivePlayers)
                    HandleClientConnected(client);
            }
        }

        #endregion

        #region Methods

        private void HandlePlayerStatesChanged()
        {
            for (int i = 0; i < _PlayerCards.Length; i++) {
                if (_Players.Length > i)
                    _PlayerCards[i].UpdateDisplay(_Players.Get(i));
                else
                    _PlayerCards[i].DisableDisplay();
            }

            foreach (NetworkCultistSelectButton button in _CultistButtons) {
                if (button.IsDisabled)
                    continue;
                if (IsCultistTaken(button.Cultist.ID, false))
                    button.SetDisabled();
            }
        }

        private void HandleClientConnected(PlayerRef client)
        {
            for (int i = 0; i < _Players.Length; i++) {
                if (_Players.Get(i).Equals(default(CultistSelectState)))
                    _Players.Set(i, new CultistSelectState(client));
            }
        }

        private void HandleClientDisconnected(PlayerRef client)
        {
            for (int i = 0; i < _Players.Length; i++) {
                if (_Players.Get(i).ClientID != client)
                    continue;
                for (int j = i; j < _Players.Length - 1; j++)
                    _Players.Set(j, _Players.Get(j + 1));
                _Players.Set(_Players.Length - 1, default(CultistSelectState));
            }
        }

        public void Select(Cultist cultist)
        {
            for (int i = 0; i < _Players.Length; i++) {
                CultistSelectState playerState = _Players.Get(i);

                if (playerState.ClientID != Runner.LocalPlayer)
                    continue;
                if (playerState.IsLockedIn)
                    return;
                if (playerState.CultistID == cultist.ID)
                    return;
                if (IsCultistTaken(cultist.ID, false))
                    return;
            }

            _CultistNameText.text = cultist.Name;

            _CultistInfoPanel.SetActive(true);

            RPC_Select(Runner.LocalPlayer, cultist.ID);
        }

        public void LockIn(bool isLockedIn)
        {
            RPC_LockIn(Runner.LocalPlayer, isLockedIn);
        }

        private bool IsCultistTaken(int cultistID, bool checkAll)
        {
            for (int i = 0; i < _Players.Length; i++) {
                CultistSelectState state = _Players.Get(i);

                if (!checkAll) {
                    if (state.ClientID == Runner.LocalPlayer)
                        continue;
                }

                if (state.IsLockedIn && state.CultistID == cultistID)
                    return true;
            }

            return false;
        }

        #endregion

        #region Events

        public void PlayerJoined(PlayerRef player)
        {
            HandleClientConnected(player);
        }

        public void PlayerLeft(PlayerRef player)
        {
            HandleClientDisconnected(player);
        }

        #endregion

        #region RPCs

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_Select(PlayerRef player, int cultistID)
        {
            for (int i = 0; i < _Players.Length; i++) {
                if (_Players.Get(i).ClientID != player)
                    continue;
                if (!_CultistDatabase.IsValidCultistID(cultistID))
                    return;
                if (IsCultistTaken(cultistID, true))
                    return;
                _Players.Set(i, new CultistSelectState(player, cultistID, _Players.Get(i).IsLockedIn));
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_LockIn(PlayerRef player, bool isLockedIn = true)
        {
            for (int i = 0; i < _Players.Length; i++) {
                if (_Players.Get(i).ClientID != player)
                    continue;
                if (isLockedIn) {
                    if (!_CultistDatabase.IsValidCultistID(_Players.Get(i).CultistID))
                        return;
                    if (IsCultistTaken(_Players.Get(i).CultistID, true))
                        return;
                }

                _Players.Set(i, new CultistSelectState(player, _Players.Get(i).CultistID, isLockedIn));
            }

            if (isLockedIn) {
                foreach (CultistSelectState state in _Players) {
                    if (!state.IsLockedIn)
                        return;
                }

                // TODO: Call Match play SetCharacter / StartGame
            }
        }

        #endregion
    }
}
