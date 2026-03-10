using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/On Game Start Effects/Change slot on table.")]
    public class SlotOnTableEffect : AEffect {

        #region Constants

        private static readonly string template = "You can play up to {0} partisans in your partisans zone";

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
        private int _Amount = 3;

        public int Amount
        {
            get => _Amount;
            set
            {
                _Amount = value;

                UpdateDescription();
            }
        }

        #endregion

        #region UI Elements

        public Sprite PartisanArea = null;

        #endregion

        public override void Play(PlayerRef playerRef)
        {
            PlayerController originPlayer = Context.NetworkGame.GetPlayer(playerRef);

            originPlayer.SetNumberOfSlotOnTable(Amount);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                (Amount.ToString(), null),
                (null, PartisanArea)
            };

            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = string.Format(template, Amount);
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (PartisanArea == null)
                PartisanArea = Resources.Load<Sprite>("Sprites/UI/Effects/Partisan_Card_Played");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
