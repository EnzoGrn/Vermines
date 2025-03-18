using System.Collections.Generic;
using System;
using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Cards.Effects;

    [System.Serializable]
    public struct EarnWithMultiplicatorCondition {

        public Func<PlayerRef, int> Condition;
        public Func<List<(string, Sprite)>> Icons;
        public string Description;
    }

    public enum EarnWithMultiplicatorConditionType {
        ScaleOnBeeSacrificed,

        // TODO: Add more condition if needed
    }

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Earn/Earn data with a multiplicator function.")]
    public class EarnWithMultiplicatorEffect : AEffect {

        #region Constants

        private static readonly string descriptionTemplate = "Earn {0} {1}";

        private static readonly string eloquenceTemplate = "<b><color=purple>{0}E</color></b>";
        private static readonly string soulTemplate = "<b><color=red>{0}A</color></b>";

        private static readonly string linkerTemplate = " then ";

        #endregion

        #region Properties

        [SerializeField]
        private string _Description;

        public override string Description
        {
            get => _Description;
            set
            {
                _Description = value;
            }
        }

        [SerializeField]
        private int _Amount = 1;

        public int Amount
        {
            get => _Amount;
            set
            {
                _Amount = value;

                UpdateDescription();
            }
        }

        [SerializeField]
        private DataType _DataToEarn = DataType.Eloquence;

        public DataType DataToEarn
        {
            get => _DataToEarn;
            set
            {
                _DataToEarn = value;

                UpdateDescription();
            }
        }

        [SerializeField]
        private EarnWithMultiplicatorCondition _Condition = ScaleOnBeeSacrificed;

        public EarnWithMultiplicatorCondition Condition
        {
            get => _Condition;
            set
            {
                _Condition = value;

                UpdateDescription();
            }
        }

        [SerializeField]
        private EarnWithMultiplicatorConditionType _ConditionChoice;

        public EarnWithMultiplicatorConditionType ConditionChoice
        {
            get => _ConditionChoice;
            set
            {
                _ConditionChoice = value;

                ConditionFactory(_ConditionChoice);
                UpdateDescription();
            }
        }

        #endregion

        #region UI Elements

        public Sprite EloquenceIcon = null;
        public Sprite SoulIcon = null;
        public Sprite ThenIcon = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            int amount = Amount * Condition.Condition.Invoke(player);

            ICommand earnCommand = new EarnCommand(player, amount, DataToEarn);

            CommandInvoker.ExecuteCommand(earnCommand);

            base.Play(player);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new();

            if (DataToEarn == DataType.Eloquence) {
                elements.Add(($"+{Amount}", null));
                elements.Add((null, EloquenceIcon));
            } else if (DataToEarn == DataType.Soul) {
                elements.Add(($"+{Amount}", null));
                elements.Add((null, SoulIcon));
            }

            elements.AddRange(Condition.Icons.Invoke());

            if (SubEffect != null) {
                elements.Add((null, ThenIcon));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            if (DataToEarn == DataType.Eloquence)
                Description = $"{string.Format(descriptionTemplate, $"{string.Format(eloquenceTemplate, Amount)}", Condition.Description)}";
            else if (DataToEarn == DataType.Soul)
                Description = $"{string.Format(descriptionTemplate, $"{string.Format(soulTemplate, Amount)}", Condition.Description)}";
            if (SubEffect != null) {
                string subDescription = SubEffect.Description;

                if (subDescription.Length > 0)
                    subDescription = char.ToLower(subDescription[0]) + subDescription[1..];
                Description += $"{linkerTemplate}{subDescription}";
            }
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (EloquenceIcon == null)
                EloquenceIcon = Resources.Load<Sprite>("Sprites/UI/Icons/Eloquence");
            if (SoulIcon == null)
                SoulIcon = Resources.Load<Sprite>("Sprites/UI/Icons/Souls");
            if (ThenIcon == null)
                ThenIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Then");
            if (PartisanSacrificedIcon == null)
                PartisanSacrificedIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Sacrificied_Other_Partisan_Card");
            if (BeeIcon == null)
                BeeIcon = Resources.Load<Sprite>("Sprites/Card/Bee/Icon");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion

        #region Conditions

        private void ConditionFactory(EarnWithMultiplicatorConditionType conditionType)
        {
            switch (conditionType) {
                case EarnWithMultiplicatorConditionType.ScaleOnBeeSacrificed:
                    Condition = ScaleOnBeeSacrificed;

                    break;
                default:
                    Condition = new();

                    break;
            }
        }

        #region Scale on bee sacrificed

        static Sprite PartisanSacrificedIcon;
        static Sprite BeeIcon;

        static public EarnWithMultiplicatorCondition ScaleOnBeeSacrificed = new() {
            Condition = (player) => ScaleOnBeeSacrificedCondition(player),
            Icons = () => DrawScaleOnBeeSacrificedCondition(),
            Description = "for each additional bee sacrificed",
        };

        static public int ScaleOnBeeSacrificedCondition(PlayerRef player)
        {
            List<ICard> sacrificed = GameDataStorage.Instance.PlayerDeck[player].Graveyard;
            int count = 0;

            foreach (ICard card in sacrificed) {
                if (card.Data.Family == CardFamily.Bee)
                    count++;
            }

            return count;
        }

        static public List<(string, Sprite)> DrawScaleOnBeeSacrificedCondition()
        {
            return new() {
                ("/" , null),
                (null, PartisanSacrificedIcon),
                (null, BeeIcon)
            };
        }

        #endregion

        #endregion
    }
}
