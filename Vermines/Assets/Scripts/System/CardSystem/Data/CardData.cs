using UnityEngine;

namespace Vermines.CardSystem.Data {

    using Vermines.CardSystem.Enumerations;

    [CreateAssetMenu(fileName = "New Card", menuName = "Vermines/Card System/Create a new card")]
    public class CardData : ScriptableObject {

        #region Information

        /// <summary>
        /// The name of the card.
        /// </summary>
        public string Name;

        /// <summary>
        /// The description of every action that the card can perform.
        /// </summary>
        public string Description;

        /// <summary>
        /// Number of exemplars of the card.
        /// (Use in Editing mode and when the cards are loading)
        /// </summary>
        /// <note>
        /// This value must be superior to 0.
        /// </note>
        public int Exemplars = 1;

        /// <summary>
        /// Is the card in the default deck of players?
        /// </summary>
        public bool IsStartingCard = false;

        #endregion

        #region Properties

        /// <summary>
        /// The type of the card.
        /// </summary>
        public CardType Type = CardType.None;

        /// <summary>
        /// Did the card belongs to a family of a player?
        /// </summary>
        public bool IsFamilyCard = false;

        /// <summary>
        /// The family of the card. Only available if the card has a partisan type. (Default: None)
        /// </summary>
        [SerializeField]
        private CardFamily _Family = CardFamily.None;

        /// <summary>
        /// Get or set the family of the card. Only available if the card has a partisan type. (Default: None)
        /// </summary>
        public CardFamily Family
        {
            get => Type == CardType.Partisan ? _Family : CardFamily.None;
            set
            {
                if (Type == CardType.Partisan)
                    _Family = value;
                else
                    _Family = CardFamily.None;
            }
        }

        /// <summary>
        /// The level of the card. Only partisan can have levels.
        /// The level is used to determine the power of the card. It can only be between 1 and 2.
        /// </summary>
        [SerializeField]
        private int _Level = 0;

        /// <summary>
        /// Get or set the level of the card. Only partisan can have levels.
        /// </summary>
        public int Level
        {
            get => Type == CardType.Partisan ? _Level : 0;
            set
            {
                if (Type == CardType.Partisan)
                    _Level = Mathf.Clamp(value, 1, 2);
                else
                    _Level = 0;
            }
        }

        #endregion

        #region Stats

        /// <summary>
        /// The cost of the card (with Eloquence as the currency).
        /// </summary>
        public int Eloquence = 0;

        /// <summary>
        /// The souls of the card (souls represent the points system of the game).
        /// </summary>
        [SerializeField]
        private int _Souls = 0;

        /// <summary>
        /// Get or set the souls of the card (souls represent the points system of the game).
        /// </summary>
        public int Souls
        {
            get => Type == CardType.Partisan ? _Souls : 0;
            set
            {
                if (Type == CardType.Partisan)
                    _Souls = value;
                else
                    _Souls = 0;
            }
        }

        #endregion

        #region UI Elements

        /// <summary>
        /// The sprite name of the visual, an item, or a character that represents the card.
        /// Given that the card is a family card.
        /// </summary>
        [SerializeField]
        private string _SpriteName = string.Empty;

        /// <summary>
        /// Get or set the sprite name of the visual
        /// </summary>
        public string SpriteName
        {
            get => IsFamilyCard ? _SpriteName : string.Empty;
            set
            {
                if (IsFamilyCard)
                    _SpriteName = value;
                else
                    _SpriteName = string.Empty;
            }
        }

        /// <summary>
        /// The sprite of the visual, an item, or a character that represents the card.
        /// </summary>
        [SerializeField]
        private Sprite _Sprite = null;

        /// <summary>
        /// Get or set the sprite of the visual
        /// </summary>
        public Sprite Sprite
        {
            get => !IsFamilyCard ? _Sprite : null;
            set
            {
                _Sprite = value;
            }
        }

        #endregion
    }
}
