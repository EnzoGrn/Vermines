using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Player;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Sacrifice/Sacrifice cards.")]
    public class SacrificeACard : AEffect {

        #region Constants

        private static readonly string sacrificeTemplate = "Sacrifice a <b>Partisan</b>";
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

        public Sprite SacrificedIcon = null;
        public Sprite SoulIcon       = null;
        public Sprite Then           = null;

        #endregion

        public override bool CanBePlayed(PlayerRef player, out string reasonKey)
        {
            PlayerController controller = Context.NetworkGame.GetPlayer(player);
            if (controller.PlayedCards.Count == 0)
            {
                reasonKey = "skipped.remove_no_target";
                return false;
            }
            reasonKey = null;
            return true;
        }

        public override void Play(PlayerRef playerRef)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerRef);

            if (!CanBePlayed(playerRef, out string reasonKey))
            {
                if (playerRef == Context.Runner.LocalPlayer)
                    GameEvents.OnEffectSkipped.Invoke(Card, reasonKey);

                return;
            }


            if (playerRef == Context.Runner.LocalPlayer) {
                GameEvents.OnEffectPromptRequested.Invoke(new EffectPrompt(EffectPromptKind.Sacrifice, CardType.None, Card));
                GameEvents.OnCardSacrificedRequested.AddListener(OnSacrificed);
                GameEvents.OnTurnTimerExpired.AddListener(CancelPrompt);
            }
        }

        public void OnSacrificed(ICard card)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(Context.Runner.LocalPlayer);

            GameEvents.OnCardSacrificedRequested.RemoveListener(OnSacrificed);
            GameEvents.OnEffectPromptClosed.Invoke(EffectPromptKind.Sacrifice);
            GameEvents.OnTurnTimerExpired.RemoveListener(CancelPrompt);

            player.OnCardSacrificed(card.ID);
            player.NetworkEventCardEffect(Card == null ? -1 : Card.ID);
        }

        private void CancelPrompt()
        {
            GameEvents.OnCardSacrificedRequested.RemoveListener(OnSacrificed);
            GameEvents.OnTurnTimerExpired.RemoveListener(CancelPrompt);
        }

        public override void NetworkEventFunction(PlayerRef player, string data)
        {
            base.Play(player);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { (null  , SacrificedIcon) },
                { ($"+ X", null)           },
                { (null  , SoulIcon)       }
            };

            if (SubEffect != null) {
                elements.Add((null, Then));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = $"{sacrificeTemplate}";

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

            if (SacrificedIcon == null)
                SacrificedIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Sacrificed_Other_Partisan_Card");
            if (SoulIcon == null)
                SoulIcon = Resources.Load<Sprite>("Sprites/UI/Icons/Souls");
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
