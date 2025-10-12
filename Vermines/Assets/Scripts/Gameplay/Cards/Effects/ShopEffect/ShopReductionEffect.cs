using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.ShopSystem.Data;
    using Vermines.ShopSystem.Enumerations;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Shop/Reduction on Shop.")]
    public class ShopReductionEffect : AEffect {

        #region Constants

        private static readonly string shopTemplate = "Your purchases {0} cost <b><color=purple>{1}E</color></b> less";

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
        public List<ShopType> _ShopTarget = new() {
            ShopType.Market, ShopType.Courtyard
        };

        public List<ShopType> ShopTarget
        {
            get => _ShopTarget;
            set
            {
                _ShopTarget = value;

                UpdateDescription();
            }
        }

        #endregion

        #region UI Elements

        public Sprite CourtyardIcon = null;
        public Sprite MarketIcon    = null;
        public Sprite EloquenceIcon = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            foreach (var shopTarget in _ShopTarget) {
                Debug.Log($"[SHOP REDUCTION PLAY]: {shopTarget}, Apply reduction of {_Amount}");

                ShopSection shopSection = GameDataStorage.Instance.Shop.Sections[shopTarget];

                if (shopSection == null)
                    continue;
                foreach (var slot in shopSection.AvailableCards)
                    slot.Value.Data.EloquenceReduction(_Amount);
                foreach (var card in shopSection.Deck)
                    card.Data.EloquenceReduction(_Amount);
                foreach (var card in shopSection.DiscardDeck)
                    card.Data.EloquenceReduction(_Amount);
            }

            base.Play(player);
        }

        public override void Stop(PlayerRef player)
        {
            foreach (var shopTarget in _ShopTarget) {
                Debug.Log($"[SHOP REDUCTION STOP]: {shopTarget}, Remove reduction of {_Amount}");

                ShopSection shopSection = GameDataStorage.Instance.Shop.Sections[shopTarget];

                if (shopSection == null)
                    continue;
                foreach (var slot in shopSection.AvailableCards)
                    slot.Value.Data.RemoveReduction(_Amount);
                foreach (var card in shopSection.Deck)
                    card.Data.RemoveReduction(_Amount);
                foreach (var card in shopSection.DiscardDeck)
                    card.Data.RemoveReduction(_Amount);
            }

            base.Stop(player);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new();

            foreach (var shop in _ShopTarget) {
                if (shop == ShopType.Market)
                    elements.Add((null, MarketIcon));
                else if (shop == ShopType.Courtyard)
                    elements.Add((null, CourtyardIcon));
            }

            elements.Add(($" = -{Amount}", null));
            elements.Add((null, EloquenceIcon));

            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = string.Empty;

            if (_ShopTarget.Count == 1)
                Description = $"{string.Format(shopTemplate, $"at the {_ShopTarget[0]} ", _Amount)}";
            else
                Description = $"{string.Format(shopTemplate, "", Amount)}";
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (EloquenceIcon == null)
                EloquenceIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Eloquence_Banner");
            if (CourtyardIcon == null)
                CourtyardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Courtyard");
            if (MarketIcon == null)
                MarketIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Market");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
