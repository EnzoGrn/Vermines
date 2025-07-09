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
        private Color _MyColor = Color.yellow;

        [SerializeField]
        private GameObject _Visuals;

        [SerializeField]
        private Image _BackgroundImage;

        [SerializeField]
        private Image _CultistHolder;

        [SerializeField]
        private Image _CultistIconImage;

        [SerializeField]
        private TMP_Text _PlayerNameText;

        [SerializeField]
        private TMP_Text _CultistNameText;

        [SerializeField]
        private TMP_Text _PlayerStatusText;

        bool _IsMine = false;

        #endregion

        #region Methods

        public void UpdateDisplay(CultistSelectState state, bool isMine = false)
        {
            if (state.CultistID != -1) {
                Cultist cultist = _CultistDatabase.GetCultistByID(state.CultistID);

                _CultistIconImage.sprite  = cultist.CultistSprite;
                _CultistIconImage.enabled = true;
                _CultistNameText.text     = cultist.Name;
            } else {
                _CultistIconImage.enabled = false;
                _CultistNameText.text     = string.Empty;
            }

            _PlayerStatusText.text = state.IsLockedIn ? "(Selected)" : "(Choosing...)";
            _PlayerNameText.text   = $"{state.Name}";

            SetIsMine(isMine);

            _Visuals.SetActive(true);
        }

        public void DisableDisplay()
        {
            _Visuals.SetActive(false);
        }

        private void SetIsMine(bool isMine)
        {
            _IsMine = isMine;

            if (_IsMine) {
                _BackgroundImage.color = _MyColor;
                _CultistHolder.color   = _MyColor;
            } else {
                _BackgroundImage.color = Color.white;
                _CultistHolder.color   = Color.white;
            }
        }

        #endregion
    }
}
