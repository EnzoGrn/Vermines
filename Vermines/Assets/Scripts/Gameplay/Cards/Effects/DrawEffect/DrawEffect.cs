using System.Collections.Generic;
using System.Linq;
using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.Gameplay.Commands.Deck;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Draw/Draw cards.")]
    public class DrawEffect : AEffect {

        #region Constants

        private static readonly string drawTemplate = "Draw {0} card";
        private static readonly string everyoneDrawTemplate = "Everyone draws {0} card";
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

        public Sprite DrawIcon = null;
        public Sprite EveryoneIcon = null;
        public Sprite Then = null;

        #endregion

        private void Draw(PlayerController player, int amount)
        {
            for (int i = 0; i < amount; i++) {
                ICommand drawCommand = new DrawCommand(player);

                CommandResponse command = CommandInvoker.ExecuteCommand(drawCommand);

                if (command.Status == CommandStatus.Success && Context.Runner.LocalPlayer == player.Object.InputAuthority)
                    GameEvents.InvokeOnDrawCard(player.Deck.Hand.Last());
            }
        }

        public override void Play(PlayerRef playerRef)
        {
            PlayerController originPlayer = Context.NetworkGame.GetPlayer(playerRef);

            if (Everyone) { // Except you
                List<PlayerController> players = Context.Runner.GetAllBehaviours<PlayerController>();

                foreach (PlayerController player in players) {
                    if (playerRef == player.Object.InputAuthority)
                        continue;
                    Draw(player, Amount);
                }
            } else {
                Draw(originPlayer, Amount);
            }

            base.Play(playerRef);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new();

            if (Everyone)
                elements.Add((null, EveryoneIcon));
            elements.Add((null, DrawIcon));
            elements.Add(($"{Amount}", null));

            if (SubEffect != null) {
                elements.Add((null, Then));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            if (Everyone)
                Description = $"{string.Format(everyoneDrawTemplate, Amount)}";
            else
                Description = $"{string.Format(drawTemplate, Amount)}";

            if (Amount > 1)
                Description += "s";
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

            if (DrawIcon == null)
                DrawIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Draw");
            if (EveryoneIcon == null)
                EveryoneIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Everyone");
            if (Then == null)
                Then = Resources.Load<Sprite>("Sprites/UI/Effects/Then");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
