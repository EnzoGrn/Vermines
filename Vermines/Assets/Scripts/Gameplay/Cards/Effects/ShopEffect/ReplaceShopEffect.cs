using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.Player;
    using Vermines.ShopSystem.Enumerations;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Shop/Replace card in shop.")]
    public class ReplaceShopEffect : AEffect {

        #region Constants

        private static readonly string replaceTemplate = "You can replace a card in the courtyard and/or in the market.";

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

        #endregion

        #region UI Elements

        public Sprite CourtyardIcon = null;
        public Sprite MarketIcon = null;
        public Sprite CardReplace = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            if (player == PlayerController.Local.PlayerRef)
            {
                var context = new ReplaceEffectContext(dict =>
                {
                    Debug.Log("[ReplaceEffect] Replace effect completed.");
                    ReplaceCard(dict);
                });

                UIContextManager.Instance.PushContext(context);
            }
        }

        private void ReplaceCard(Dictionary<ShopType, int> dictShopSlot)
        {
            Debug.Log("[ReplaceEffect] Replacing cards in shops:");
            foreach (var shopSlot in dictShopSlot)
            {
                Debug.Log($"[ReplaceEffect] Replacing {shopSlot.Key} at slot {shopSlot.Value}");
                PlayerController.Local.OnShopReplaceCard(shopSlot.Key, shopSlot.Value);
            }
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { (null, CardReplace) },
                { (null, MarketIcon) },
                { (null, CourtyardIcon) }
            };
            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = replaceTemplate;
        }

        private void OnEnable()
        {
            UpdateDescription();

            if (CourtyardIcon == null)
                CourtyardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Courtyard");
            if (MarketIcon == null)
                MarketIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Market");
            if (CardReplace == null)
                CardReplace = Resources.Load<Sprite>("Sprites/UI/Effects/Replace_Card");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
