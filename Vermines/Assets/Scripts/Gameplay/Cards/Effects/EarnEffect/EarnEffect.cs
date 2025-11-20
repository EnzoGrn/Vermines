using System.Collections.Generic;
using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Earn/Earn data.")]
    public class EarnEffect : AEffect {

        #region Constants

        private static readonly string eloquenceTemplate   = "<b><color=purple>{0}E</color></b>";
        private static readonly string soulTemplate        = "<b><color=red>{0}A</color></b>";
        private static readonly string descriptionTemplate = "Earn ";
        private static readonly string descriptionEveryoneTemplate = "Everyone earns ";
        private static readonly string linkerTemplate      = " then ";

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
        private bool _Everyone = false;

        public bool Everyone
        {
            get => _Everyone;
            set
            {
                _Everyone = value;

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
        public Sprite EveryoneIcon  = null;
        public Sprite ThenIcon      = null;

        #endregion

        public override void Play(PlayerRef playerRef)
        {
            PlayerController originPlayer = Context.NetworkGame.GetPlayer(playerRef);

            if (Everyone) {
                List<PlayerController> players = Context.Runner.GetAllBehaviours<PlayerController>();

                foreach (PlayerController player in players) {
                    ICommand earnCommand = new EarnCommand(player, Amount, DataToEarn);

                    CommandInvoker.ExecuteCommand(earnCommand);
                }
            } else {
                ICommand earnCommand = new EarnCommand(originPlayer, Amount, DataToEarn);

                CommandInvoker.ExecuteCommand(earnCommand);
            }

            base.Play(playerRef);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new();

            if (Everyone)
                elements.Add((null, EveryoneIcon));
            if (DataToEarn == DataType.Eloquence) {
                elements.Add(($"+{Amount}E", null));
                elements.Add((null, EloquenceIcon));
            } else if (DataToEarn == DataType.Soul) {
                elements.Add(($"+{Amount}A", null));
                elements.Add((null, SoulIcon));
            }

            if (SubEffect != null) {
                elements.Add((null, ThenIcon));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            string description = Everyone ? descriptionEveryoneTemplate : descriptionTemplate;

            if (DataToEarn == DataType.Eloquence)
                Description = $"{description}{string.Format(eloquenceTemplate, Amount)}";
            else if (DataToEarn == DataType.Soul)
                Description = $"{description}{string.Format(soulTemplate, Amount)}";
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
            if (EveryoneIcon == null)
                EveryoneIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Everyone");
            if (ThenIcon == null)
                ThenIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Then");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
