using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using OMGG.DesignPattern;
    using Vermines.Player;
    using System.Linq;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Earn/Earn data for each ...")]
    public class EarnForEachEffect : AEffect {

        public enum ZoneType {
            PlayArea,
            Graveyard,
        }

        #region Constants

        private static readonly string template = "Earn {0} for each {1}";
        private static readonly string eloquenceTemplate = "<b><color=purple>{0}E</color></b>";
        private static readonly string soulTemplate = "<b><color=red>{0}A</color></b>";
        private static readonly string partisanTemplate = "<b>Partisan</b> card";
        private static readonly string equipmentTemplate = "<b>Equipment</b> card";
        private static readonly string playTemplate = " played";
        private static readonly string graveyardTemplate = " in the graveyard";
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
        private CardType _CardType = CardType.Partisan;

        public CardType CardType
        {
            get => _CardType;
            set
            {
                _CardType = value;

                UpdateDescription();
            }
        }

        [SerializeField]
        private ZoneType _Area = ZoneType.PlayArea;

        public ZoneType Area
        {
            get => _Area;
            set
            {
                _Area = value;

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

        public Sprite AlreadySacrifiedPartisan = null;
        public Sprite PartisanIcon = null;
        public Sprite EquipmentIcon = null;
        public Sprite EloquenceIcon = null;
        public Sprite SoulIcon = null;
        public Sprite ThenIcon = null;

        #endregion

        public override void Play(PlayerRef playerRef)
        {
            PlayerController player = Context.NetworkGame.GetPlayer(playerRef);

            if (CardType == CardType.Equipment) {
                List<ICard> equipments =player.Equipments.ToList();

                foreach (ICard _ in equipments) {
                    ICommand earnCommand = new EarnCommand(player, Amount, DataToEarn);

                    CommandInvoker.ExecuteCommand(earnCommand);
                }
            } else if (CardType == CardType.Partisan) {
                List<ICard> cards = player.Deck.PlayedCards;

                if (Area == ZoneType.Graveyard)
                    cards = player.Deck.Graveyard;
                foreach (ICard card in cards) {
                    if (card.Data.Type != CardType.Partisan)
                        continue;
                    ICommand earnCommand = new EarnCommand(player, Amount, DataToEarn);

                    CommandInvoker.ExecuteCommand(earnCommand);
                }
            } else if (CardType == CardType.None) {
                List<ICard> cards = player.Deck.PlayedCards;

                if (Area == ZoneType.Graveyard)
                    cards = player.Deck.Graveyard;
                foreach (ICard _ in cards) {
                    ICommand earnCommand = new EarnCommand(player, Amount, DataToEarn);

                    CommandInvoker.ExecuteCommand(earnCommand);
                }
            }

            base.Play(playerRef);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new();

            if (DataToEarn == DataType.Eloquence) {
                elements.Add(($"+{Amount}E", null));
                elements.Add((null, EloquenceIcon));
            } else if (DataToEarn == DataType.Soul) {
                elements.Add(($"+{Amount}A", null));
                elements.Add((null, SoulIcon));
            }

            elements.Add(("/", null));

            if (CardType == CardType.Partisan) {
                if (Area == ZoneType.Graveyard)
                    elements.Add((null, AlreadySacrifiedPartisan));
                else
                    elements.Add((null, PartisanIcon));
            } else if (CardType == CardType.Equipment) {
                elements.Add((null, EquipmentIcon));
            }

            if (SubEffect != null) {
                elements.Add((null, ThenIcon));
                elements.AddRange(SubEffect.Draw());
            }

            return elements;
        }

        protected override void UpdateDescription()
        {
            string amountFormatted = "";

            if (DataToEarn == DataType.Eloquence)
                amountFormatted = string.Format(eloquenceTemplate, Amount);
            else if (DataToEarn == DataType.Soul)
                amountFormatted = string.Format(soulTemplate, Amount);

            string target = "";

            if (CardType == CardType.Partisan)
                target = partisanTemplate;
            else if (CardType == CardType.Equipment)
                target = equipmentTemplate;

            Description = string.Format(template, amountFormatted, target);

            if (Area == ZoneType.PlayArea)
                Description += playTemplate;
            else if (Area == ZoneType.Graveyard)
                Description += graveyardTemplate;

            if (SubEffect != null) {
                string subDescription = SubEffect.Description;

                if (!string.IsNullOrEmpty(subDescription))
                    subDescription = char.ToLower(subDescription[0]) + subDescription[1..];
                Description += $"{linkerTemplate}{subDescription}";
            }
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (AlreadySacrifiedPartisan == null)
                AlreadySacrifiedPartisan = Resources.Load<Sprite>("Sprites/UI/Effects/Already_sacrified_card");
            if (PartisanIcon == null)
                PartisanIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Partisan_Card_Played");
            if (EquipmentIcon == null)
                EquipmentIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Equipment_Card");
            if (EloquenceIcon == null)
                EloquenceIcon = Resources.Load<Sprite>("Sprites/UI/Icons/Eloquence");
            if (SoulIcon == null)
                SoulIcon = Resources.Load<Sprite>("Sprites/UI/Icons/Souls");
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
