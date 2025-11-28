using System.Collections.Generic;
using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Spend/Spend data.")]
    public class SpendEffect : AEffect {

        #region Constants

        private static readonly string eloquenceTemplate   = "<b><color=purple>{0}E</color></b>";
        private static readonly string soulTemplate        = "<b><color=red>{0}A</color></b>";
        private static readonly string descriptionTemplate = "Spend ";
        private static readonly string linkerTemplate      = " to ";

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
        private DataType _DataToSpend = DataType.Eloquence;

        public DataType DataToSpend
        {
            get => _DataToSpend;
            set
            {
                _DataToSpend = value;

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
        public Sprite SoulIcon      = null;

        #endregion

        public override void Play(PlayerRef playerRef)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerRef);

            if (Context.Runner.LocalPlayer == playerRef)
                player.NetworkEventCardEffect(Card.ID);
        }

        public override void NetworkEventFunction(PlayerRef playerRef, string data)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerRef);

            ICommand spendCommand = new SpendCommand(player, Amount, DataToSpend);

            CommandInvoker.ExecuteCommand(spendCommand);

            base.Play(playerRef);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new();

            if (DataToSpend == DataType.Eloquence) {
                elements.Add(($"{Amount}E", null));
                elements.Add((null, EloquenceIcon));
            } else if (DataToSpend == DataType.Soul) {
                elements.Add(($"{Amount}A", null));
                elements.Add((null, SoulIcon));
            }

            if (SubEffect != null) {
                elements.Add((" : ", null));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            if (DataToSpend == DataType.Eloquence)
                Description = $"{descriptionTemplate}{string.Format(eloquenceTemplate, Amount)}";
            else if (DataToSpend == DataType.Soul)
                Description = $"{descriptionTemplate}{string.Format(soulTemplate, Amount)}";
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
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
