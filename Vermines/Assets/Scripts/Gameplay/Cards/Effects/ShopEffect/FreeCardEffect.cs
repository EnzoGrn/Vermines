using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.Player;
    using Vermines.ShopSystem.Enumerations;

    [CreateAssetMenu(fileName = "New Effect", menuName = "Vermines/Card System/Card/Effects/Shop/Free card in shop.")]
    public class FreeCardEffect : AEffect {

        #region Constants

        private static readonly string shopTemplate = "Once per turn, you can buy {0} card{1} for free in the {2}.";

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
        private ShopType _ShopTarget = ShopType.Market;

        public ShopType ShopTarget
        {
            get => _ShopTarget;
            set
            {
                _ShopTarget = value;

                UpdateDescription();
            }
        }

        private int _CurrentBuy = 0;

        #endregion

        #region UI Elements

        public Sprite CourtyardIcon = null;
        public Sprite MarketIcon = null;
        public Sprite EloquenceIcon = null;
        public Sprite CardIcon = null;

        #endregion

        public override void Play(PlayerRef player)
        {
            GameDataStorage.Instance.Shop.Sections[_ShopTarget].SetFree(true);

            if (player == PlayerController.Local.PlayerRef) {
                if (UIContextManager.Instance != null)
                    UIContextManager.Instance.PushContext(new FreeCardContext(_ShopTarget));
            }

            GameEvents.OnCardPurchased.AddListener(OnBuy);
        }

        public void OnBuy(ShopType shopType, int cardId)
        {
            if (_ShopTarget != shopType)
                return;
            _CurrentBuy++;

            if (_CurrentBuy == _Amount) {
                GameDataStorage.Instance.Shop.Sections[_ShopTarget].SetFree(false);

                UIContextManager.Instance.PopContext();
                GameEvents.OnCardPurchased.RemoveListener(OnBuy);
            }
        }

        public override void Stop(PlayerRef player)
        {
            GameDataStorage.Instance.Shop.Sections[_ShopTarget].SetFree(false);
        }

        public override List<(string, Sprite)> Draw()
        {
            List<(string, Sprite)> elements = new() {
                { ($"{_Amount}", null) },
                { (null    , CardIcon) }
            };

            if (_ShopTarget == ShopType.Market)
                elements.Add((null, MarketIcon));
            else if (_ShopTarget == ShopType.Courtyard)
                elements.Add((null, CourtyardIcon));
            elements.Add(($" = 0", null));
            elements.Add((null, EloquenceIcon));

            return elements;
        }

        protected override void UpdateDescription()
        {
            Description = $"{string.Format(shopTemplate, $"{_Amount}", (_Amount > 1) ? "s" : "", _ShopTarget)}";
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
            if (CardIcon == null)
                CardIcon = Resources.Load<Sprite>("Sprites/UI/Effects/Card");
        }

        #region Editor Editor

        public override void OnValidate()
        {
            UpdateDescription();
        }

        #endregion
    }
}
