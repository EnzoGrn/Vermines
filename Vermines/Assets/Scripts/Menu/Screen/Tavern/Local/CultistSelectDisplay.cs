using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Vermines.Menu.Tavern {

    using Vermines.Characters;
    using Vermines.Core.UI;
    using Vermines.Menu.View;

    public class CultistSelectDisplay : UIBehaviour {

        #region Attributes

        [Header("Database")]

        [SerializeField]
        private CultistDatabase _CultistDatabase;

        [Header("UI Elements")]

        [SerializeField]
        private Transform _CultistHolder;
        
        [SerializeField]
        private CultistSelectButton _SelectButtonPrefab;

        [SerializeField]
        private CultistInfoPanel _CultistInfoPanel;

        [SerializeField]
        private TextMeshProUGUI _CultistNameText;

        private readonly List<CultistSelectButton> _CultistButtons = new();

        private UITavernView _View;

        #endregion

        #region Methods

        public void Initialize(UITavernView view)
        {
            _View = view;

            Cultist[] cultists = _CultistDatabase.GetAllCultists();

            foreach (Cultist cultist in cultists) {
                CultistSelectButton selectedButtonInstance = Instantiate(_SelectButtonPrefab, _CultistHolder);

                selectedButtonInstance.Initialize();
                selectedButtonInstance.SetCharacter(this, cultist);

                _CultistButtons.Add(selectedButtonInstance);
            }

            Clear();
        }

        public void Deinitialize()
        {
            foreach (CultistSelectButton button in _CultistButtons)
                button.Deinitialize();
        }

        #endregion

        #region Methods

        private void Clear()
        {
            _CultistInfoPanel.gameObject.SetActive(false);
            _CultistInfoPanel.SetCharacter(ScriptableObject.CreateInstance<Cultist>());

            foreach (CultistSelectButton button in _CultistButtons)
                button.UnSelect();
            _View.PlayerCultist           = default;
            _View.PlayButton.interactable = false;
        }

        #endregion

        #region Events

        public void OnSelect(int cultistID)
        {
            if (_View.PlayerCultist == cultistID)
                return;
            if (_View.PlayerCultist > 0)
                _CultistButtons.Find(button => button.Cultist.ID == _View.PlayerCultist).UnSelect();
            Cultist cultist = _CultistDatabase.GetCultistByID(cultistID);

            _CultistInfoPanel.SetCharacter(cultist);
            _CultistInfoPanel.gameObject.SetActive(true);

            _View.OnCultistSelected(cultist.ID);
            _View.PlayButton.interactable = true;
        }

        public void OnSelect(Cultist cultist)
        {
            if (_View.PlayerCultist == cultist.ID)
                return;
            if (_View.PlayerCultist > 0)
                _CultistButtons.Find(button => button.Cultist.ID == _View.PlayerCultist).UnSelect();
            _CultistInfoPanel.SetCharacter(cultist);
            _CultistInfoPanel.gameObject.SetActive(true);

            _View.OnCultistSelected(cultist.ID);
            _View.PlayButton.interactable = true;
        }

        public void OnClose()
        {
            Clear();
        }

        #endregion
    }
}
