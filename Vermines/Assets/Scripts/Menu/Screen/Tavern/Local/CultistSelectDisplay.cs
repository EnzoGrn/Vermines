using System.Collections.Generic;
using OMGG.Menu.Screen;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Vermines.Menu.Screen.Tavern {

    using Vermines.Characters;

    public class CultistSelectDisplay : MenuScreenPlugin {

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
        private TMP_Text _CultistNameText;

        [SerializeField]
        private Button _PlayButton;

        private List<CultistSelectButton> _CultistButtons = new();

        private int _PlayerCultistID;

        #endregion

        #region Overrides Methods

        public override void Init(MenuUIScreen screen)
        {
            base.Init(screen);

            Cultist[] cultists = _CultistDatabase.GetAllCultists();

            foreach (Cultist cultist in cultists) {
                CultistSelectButton selectedButtonInstance = Instantiate(_SelectButtonPrefab, _CultistHolder);

                selectedButtonInstance.SetCharacter(this, cultist);

                _CultistButtons.Add(selectedButtonInstance);
            }

            Reset();
        }

        public override void Show(MenuUIScreen screen)
        {
            base.Show(screen);
        }

        public override void Hide(MenuUIScreen screen)
        {
            base.Hide(screen);

            Reset();
        }

        #endregion

        #region Methods

        private void Reset()
        {
            _CultistInfoPanel.gameObject.SetActive(false);
            _CultistInfoPanel.SetCharacter(ScriptableObject.CreateInstance<Cultist>());

            foreach (CultistSelectButton button in _CultistButtons)
                button.UnSelect();
            _PlayerCultistID = -1;

            _PlayButton.interactable = false;
        }

        public void Select(Cultist cultist)
        {
            if (_PlayerCultistID == cultist.ID)
                return;
            if (_PlayerCultistID != -1)
                _CultistButtons.Find(button => button.Cultist.ID == _PlayerCultistID).UnSelect();
            _CultistInfoPanel.SetCharacter(cultist);
            _CultistInfoPanel.gameObject.SetActive(true);

            _PlayerCultistID = cultist.ID;

            _PlayButton.interactable = true;
        }

        public Cultist GetSelectedCultist()
        {
            return _CultistDatabase.GetCultistByID(_PlayerCultistID);
        }

        #endregion
    }
}
