using OMGG.Menu.Screen;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Vermines.Menu.Tavern {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.Characters;
    using Vermines.UI;

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
        private UIButton _Button;

        [SerializeField]
        private GameObject _SelectedOverlay;

        [Header("Cultist Select")]

        private CultistSelectDisplay _CultistSelect;

        public Cultist Cultist { get; private set; }

        #endregion

        #region Methods

        public void Initialize()
        {
            _Button.onClick.AddListener(SelectCharacter);
        }

        public void Deinitialize()
        {
            _Button.onClick.RemoveListener(SelectCharacter);
        }

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
            _CultistSelect.OnSelect(Cultist);

            _SelectedOverlay.SetActive(true);
        }

        public void UnSelect()
        {
            _SelectedOverlay.SetActive(false);
        }

        #endregion
    }
}
