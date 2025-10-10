using System.Collections.Generic;
using UnityEngine;

namespace Vermines.CardSystem.Data {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Data.Effect;

    [CreateAssetMenu(fileName = "New Card", menuName = "Vermines/Card System/Create a new card")]
    public class CardData : ScriptableObject {

        #region Information

        /// <summary>
        /// The name of the card.
        /// </summary>
        public string Name;

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

        #region Effects

        public List<AEffect> Effects;

        private List<AEffect> _OriginalEffect;

        public void CopyEffect(List<AEffect> effects)
        {
            _OriginalEffect = new List<AEffect>(Effects);

            Effects = effects;
        }

        public void RemoveEffectCopied()
        {
            _OriginalEffect ??= new List<AEffect>(Effects); // If the _Original effect not exists.

            Effects = new List<AEffect>(_OriginalEffect);
        }

        public bool ReduceInSilence = false;

        public bool HasEffectOfType(EffectType type)
        {
            foreach (AEffect effect in Effects) {
                if ((effect.Type & type) != 0)
                    return true;
            }
            return false;
        }

        #endregion

        #region Stats

        /// <summary>
        /// The cost of the card (with Eloquence as the currency).
        /// </summary>
        [SerializeField]
        private int _Eloquence = 0;

        /// <summary>
        /// Get or set the cost of the card (with Eloquence as the currency).
        /// </summary>
        public int Eloquence
        {
            get => IsStartingCard ? 0 : _Eloquence;
            set
            {
                if (IsStartingCard)
                    _Eloquence = 0;
                else
                    _Eloquence = value;
                CurrentEloquence = _Eloquence;
            }
        }

        public bool IsFree = false;

        /// <summary>
        /// The current eloquence value of the card.
        /// Use it in the UI, for know if the value has changed or not.
        /// </summary>
        public int CurrentEloquence = 0;

        /// <summary>
        /// The eloquence that are not reduced because the card is already at the minimum cost.
        /// </summary>
        private int _ExcessiveEloquence = 0;

        /// <summary>
        /// Function for reducing the eloquence of the card.
        /// </summary>
        public void EloquenceReduction(int amount)
        {
            if (CurrentEloquence - amount < 1) {
                _ExcessiveEloquence = Mathf.Abs(CurrentEloquence - 1);

                CurrentEloquence = 1;
            } else {
                CurrentEloquence -= amount;
            }
        }

        /// <summary>
        /// Function that remove the reduction of the eloquence of the card.
        /// </summary>
        public void RemoveReduction(int amount)
        {
            if (_ExcessiveEloquence > 0 && amount < _ExcessiveEloquence) {
                _ExcessiveEloquence -= amount;
            } else if (_ExcessiveEloquence > 0) {
                amount -= _ExcessiveEloquence;

                CurrentEloquence   += amount;
                _ExcessiveEloquence = 0;
            } else {
                CurrentEloquence += amount;
            }
        }

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
                CurrentSouls = _Souls;
            }
        }

        /// <summary>
        /// The current souls value of the card.
        /// Use it in the UI, for know if the value has changed or not.
        /// </summary>
        public int CurrentSouls = 0;

        private int _ExcessiveSouls = 0;

        public void SoulsReduction(int amount)
        {
            if (CurrentSouls - amount < 0) {
                _ExcessiveSouls = Mathf.Abs(CurrentSouls);

                CurrentSouls = 0;
            } else {
                CurrentSouls -= amount;
            }
        }

        public void RemoveSoulsReduction(int amount)
        {
            if (_ExcessiveSouls > 0 && amount < _ExcessiveSouls)
                _ExcessiveSouls -= amount;
            else if (_ExcessiveSouls > 0) {
                amount -= _ExcessiveSouls;

                CurrentSouls += amount;
                _ExcessiveSouls = 0;
            } else {
                CurrentSouls += amount;
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

        public List<(string, Sprite)> Draw()
        {
            if (Effects == null || Effects.Count == 0)
                return null;
            if (Effects.Count == 1 && Effects[0] != null)
                return Effects[0].Draw();
            else if (Effects.Count == 1)
                return null;
            List<(string, Sprite)> elements  = new();
            Sprite                 separator = Resources.Load<Sprite>("Sprites/UI/Effects/separator");

            for (int i = 0; i < Effects.Count; i++) {
                elements.AddRange(Effects[i].Draw());

                if (i < Effects.Count - 1)
                    elements.Add(("Separator", separator));
            }
            return elements;
        }

        #endregion
    }
}
