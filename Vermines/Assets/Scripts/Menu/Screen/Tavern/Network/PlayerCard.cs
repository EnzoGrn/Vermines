using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Vermines.Menu.CustomLobby {

    using Vermines.Characters;
    using Vermines.Core.UI;
    using Vermines.Extension;

    public class PlayerCard : UIBehaviour {

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
        private TextMeshProUGUI _PlayerNameText;

        [SerializeField]
        private TextMeshProUGUI _CultistNameText;

        [SerializeField]
        private TextMeshProUGUI _PlayerStatusText;

        bool _IsMine = false;

        #endregion

        #region Methods

        public void UpdateDisplay(CultistSelectState state, string playerName, bool isMine = false)
        {
            if (state.CultistID > 0) {
                Cultist cultist = _CultistDatabase.GetCultistByID(state.CultistID);

                _CultistIconImage.sprite  = cultist.CultistSprite;
                _CultistIconImage.enabled = true;

                _CultistNameText.SetTextSafe(cultist.Name);
            } else {
                _CultistIconImage.enabled = false;

                _CultistNameText.SetTextSafe(string.Empty);
            }

            _PlayerStatusText.SetTextSafe(state.IsLockedIn ? "(Selected)" : "(Choosing...)");
            _PlayerNameText.SetTextSafe(playerName);

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
