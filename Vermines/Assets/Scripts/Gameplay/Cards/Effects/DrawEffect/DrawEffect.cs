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
        public Sprite Then = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            for (int i = 0; i < Amount; i++) {
                ICommand drawCommand = new DrawCommand(player);

                CommandResponse command = CommandInvoker.ExecuteCommand(drawCommand);

                if (command.Status == CommandStatus.Success && PlayerController.Local.PlayerRef == player) {
                    PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[player];

                    GameEvents.InvokeOnDrawCard(deck.Hand.Last());

                    Debug.Log($"[DrawEffect] Player {player} drew a card from his deck. He drew {deck.Hand.Last().Data.Name}.");
                }
            }

            base.Play(player);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { (null       , DrawIcon) },
                { ($"{Amount}", null) }
            };

            if (SubEffect != null) {
                elements.Add((null, Then));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
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
