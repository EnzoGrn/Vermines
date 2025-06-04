using OMGG.Menu.Screen;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Vermines.Menu.Screen.Tavern.Network {

    using Vermines.Characters;

    public class PlayerCard : MenuScreenPlugin {

        #region Attributes

        [Header("Database")]

        [SerializeField]
        private CultistDatabase _CultistDatabase;

        [Header("UI Elements")]

        [SerializeField]
        private GameObject _Visuals;
        
        [SerializeField]
        private Image _CultistIconImage;
        
        [SerializeField]
        private TMP_Text _PlayerNameText;

        [SerializeField]
        private TMP_Text _CultistNameText;

        #endregion

        #region Methods

        public void UpdateDisplay(CultistSelectState state)
        {
            if (state.CultistID != -1) {
                Cultist cultist = _CultistDatabase.GetCultistByID(state.CultistID);

                _CultistIconImage.sprite  = cultist.CultistSprite;
                _CultistIconImage.enabled = true;
                _CultistNameText.text     = cultist.Name;
            } else
                _CultistIconImage.enabled = false;
            _PlayerNameText.text = state.IsLockedIn ? $"{state.Name}" : $"{state.Name} (Choosing...)";

            _Visuals.SetActive(true);
        }

        public void DisableDisplay()
        {
            _Visuals.SetActive(false);
        }

        #endregion
    }
}
