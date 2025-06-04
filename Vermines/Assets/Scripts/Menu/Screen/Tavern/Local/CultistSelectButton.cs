using OMGG.Menu.Screen;
using UnityEngine.UI;
using UnityEngine;

namespace Vermines.Menu.Screen.Tavern {
    using TMPro;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.Characters;

    public class CultistSelectButton : MenuScreenPlugin {

        #region Attributes

        [Header("UI Elements")]

        [SerializeField]
        private Image _BackgroundImage;

        [SerializeField]
        private Image _IconImage;

        [SerializeField]
        private Image _CultistImage;

        [SerializeField]
        private TMP_Text _CultistNameText;

        [SerializeField]
        private Button _Button;

        [SerializeField]
        private GameObject _SelectedOverlay;

        [Header("Cultist Select")]

        private CultistSelectDisplay _CultistSelect;

        public Cultist Cultist { get; private set; }

        #endregion

        #region Methods

        public void SetCharacter(CultistSelectDisplay cultistSelect, Cultist cultist)
        {
            _BackgroundImage.sprite = FamilyUtils.GetSpriteByFamily(CardType.Partisan, cultist.family, "Background");
            _IconImage.sprite       = FamilyUtils.GetSpriteByFamily(CardType.Partisan, cultist.family, "Icon");
            _CultistImage.sprite    = cultist.CultistSprite;
            _CultistNameText.text   = cultist.Name;
            _CultistSelect          = cultistSelect;
            Cultist                 = cultist;
        }

        public void SelectCharacter()
        {
            _CultistSelect.Select(Cultist);

            _SelectedOverlay.SetActive(true);
        }

        public void UnSelect()
        {
            _SelectedOverlay.SetActive(false);
        }

        #endregion
    }
}
