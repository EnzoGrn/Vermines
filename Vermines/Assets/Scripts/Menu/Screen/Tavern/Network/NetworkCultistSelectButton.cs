using OMGG.Menu.Screen;
using UnityEngine.UI;
using UnityEngine;

namespace Vermines.Menu.Screen.Tavern.Network {
    using TMPro;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.Characters;

    public class NetworkCultistSelectButton : MenuScreenPlugin {

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
        private GameObject _DisabledOverlay;
        
        [SerializeField]
        private Button _Button;

        [Header("Cultist Select")]

        private NetworkCultistSelectDisplay _CultistSelect;

        public Cultist Cultist { get; private set; }

        public bool IsDisabled { get; private set; }

        #endregion

        #region Methods

        public void SetCharacter(NetworkCultistSelectDisplay cultistSelect, Cultist cultist)
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
        }

        public void SetDisabled()
        {
            IsDisabled = true;
            
            _DisabledOverlay.SetActive(true);

            _Button.interactable = false;
        }

        #endregion
    }
}
