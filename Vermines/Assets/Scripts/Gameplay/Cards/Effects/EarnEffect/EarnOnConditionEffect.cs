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
    public struct EarnCondition {

        public Func<PlayerRef, ICard, bool> Condition;
        public Func<List<(string, Sprite)>> Icons;
        public string Description;
    }

    public enum EarnConditionType {
        Sacrifice3ThisCard,

        // TODO: Add more condition if needed
    }

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Earn/Earn data when a condition is completed.")]
    public class EarnOnConditionEffect : AEffect {

        #region Constants

        private static readonly string descriptionTemplate = "If {0} then earn {1}";
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
        private EarnCondition _Condition = Sacrifice3ThisCard;

        public EarnCondition Condition
        {
            get => _Condition;
            set
            {
                _Condition = value;

                UpdateDescription();
            }
        }

        [SerializeField]
        private EarnConditionType _ConditionChoice;

        public EarnConditionType ConditionChoice
        {
            get => _ConditionChoice;
            set
            {
                _ConditionChoice = value;

                ConditionFactory(_ConditionChoice);
                UpdateDescription();
            }
        }
        
        [SerializeField]
        private AEffect _SubEffect = null;

        public override AEffect SubEffect
        {
            get => _SubEffect;
            set
            {
                _SubEffect = value;

                UpdateDescription();
            }
        }

        #endregion

        #region UI Elements

        public Sprite EloquenceIcon = null;
        public Sprite SoulIcon = null;
        public Sprite ThenIcon = null;

        #endregion

        private void Run(PlayerRef player, bool play = true)
        {
            if (Condition.Condition.Invoke(player, Card) == false)
                return;
            ICommand earnCommand = new EarnCommand(player, Amount, DataToEarn);

            CommandInvoker.ExecuteCommand(earnCommand);

            if (play)
                base.Play(player);
            else if (!play)
                base.Stop(player);
        }

        public override void Play(PlayerRef player)
        {
            Run(player);
        }

        public override void Stop(PlayerRef player)
        {
            Run(player, false);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new();

            elements.AddRange(Condition.Icons.Invoke());
            elements.Add((" = ", null));
            elements.Add(($"+{Amount}", null));

            if (DataToEarn == DataType.Eloquence)
                elements.Add((null, EloquenceIcon));
            else if (DataToEarn == DataType.Soul)
                elements.Add((null, SoulIcon));

            if (SubEffect != null) {
                elements.Add((null, ThenIcon));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            string cardName = Card != null ? Card.Data.Name : "{card_name}";

            if (DataToEarn == DataType.Eloquence)
                Description = $"{string.Format(descriptionTemplate, $"{string.Format(Condition.Description, cardName)}", $"{string.Format(eloquenceTemplate, Amount)}")}";
            else if (DataToEarn == DataType.Soul)
                Description = $"{string.Format(descriptionTemplate, $"{string.Format(Condition.Description, cardName)}", $"{string.Format(soulTemplate, Amount)}")}";
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
            if (SacrificeThisIcon == null)
                SacrificeThisIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Sacrificed_This_Card");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion

        #region Conditions

        private void ConditionFactory(EarnConditionType conditionType)
        {
            switch (conditionType) {
                case EarnConditionType.Sacrifice3ThisCard:
                    Condition = Sacrifice3ThisCard;

                    break;
                default:
                    Condition = new();

                    break;
            }
        }

        #region Sacrifice 3 This card

        static Sprite SacrificeThisIcon;

        static public EarnCondition Sacrifice3ThisCard = new() {
            Condition = (player, card) => CheckSacrifice3ThisCard(player, card),
            Icons = () => DrawCheckSacrifice3ThisCard(),
            Description = "you have sacrificed 3 <b>{0}</b>"
        };

        static public bool CheckSacrifice3ThisCard(PlayerRef player, ICard card)
        {
            List<ICard> sacrificed = GameDataStorage.Instance.PlayerDeck[player].Graveyard;
            int count = 0;

            foreach (ICard sacrificeCard in sacrificed) {
                if (sacrificeCard?.Data.Name == card?.Data.Name)
                    count++;
            }

            return count >= 3;
        }

        static public List<(string, Sprite)> DrawCheckSacrifice3ThisCard()
        {
            return new() {
                ("3"  , null),
                (null , SacrificeThisIcon)
            };
        }

        #endregion

        #endregion
    }
}
