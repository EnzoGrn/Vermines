using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Vermines.Menu.Tavern {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.Characters;
    using Vermines.Core.UI;

    public class CultistInfoPanel : UIBehaviour {

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

        #endregion

        #region Methods

        public void SetCharacter(Cultist cultist)
        {
            if (cultist.ID == -1) {
                _BackgroundImage.sprite = null;
                _IconImage.sprite       = null;
                _CultistImage.sprite    = null;
                _CultistNameText.text   = string.Empty;
            } else {
                _BackgroundImage.sprite = FamilyUtils.GetSpriteByFamily(CardType.Partisan, cultist.family, "Background");
                _IconImage.sprite       = FamilyUtils.GetSpriteByFamily(CardType.Partisan, cultist.family, "Icon");
                _CultistImage.sprite    = cultist.CultistSprite;
                _CultistNameText.text   = cultist.Name;
            }
        }

        #endregion
    }
}
