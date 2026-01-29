using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Vermines.CardSystem.Data.Effect;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;
using Vermines.Player;
using Vermines.UI.Card;
using Vermines.UI.Utils;

namespace Vermines.UI.Screen
{
    using Text = TMPro.TMP_Text;

    public class GameplayUIRecycle : GameplayUIScreen
    {
        #region Attributes

        private ICardClickHandler _previousClickHandler;
        private RecycleClickHandler _recycleHandler;

        [SerializeField] private Button validateButton;
        [SerializeField] private Button cancelButton;

        [SerializeField] private Text eloquenceText;
        [SerializeField] private Text soulsText;
        [SerializeField] private Text cardCountText;

        // Merchant Image depends on the player faction
        [SerializeField] private Image merchantImage;
        private bool _merchantImageInitialized = false;

        #endregion

        #region Override Methods

        /// <summary>
        /// The Unity awake method.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// The screen init method.
        /// </summary>
        public override void Init()
        {
            base.Init();

            _recycleHandler = new RecycleClickHandler();
        }

        /// <summary>
        /// The screen show method.
        /// </summary>
        public override void Show()
        {
            base.Show();

            _recycleHandler.OnSelectionChanged += RefreshUI;

            List<GameObject> playerCards = PlayerController.Local.Context.HandManager.HandCards;

            // Get the previous click handler to restore it later
            if (playerCards.Count > 0)
                _previousClickHandler = playerCards[0].GetComponent<CardDisplay>().GetClickHandler();

            foreach (var cardGO in playerCards)
            {
                if (!cardGO.TryGetComponent<CardDisplay>(out var card)) continue;

                if (card.Card?.Data.Type == CardType.Tools)
                    card.SetClickHandler(_recycleHandler);
                else
                    card.SetClickHandler(null);
            }

            validateButton.onClick.AddListener(OnButtonValidate);
            cancelButton.onClick.AddListener(OnButtonCancel);

            RefreshUI();
        }

        /// <summary>
        /// The screen hide method.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            validateButton.onClick.RemoveListener(OnButtonValidate);
            cancelButton.onClick.RemoveListener(OnButtonCancel);
        }

        #endregion

        #region Methods

        private void RefreshUI()
        {
            if (!_merchantImageInitialized)
            {
                CardFamily family = PlayerController.Local.Statistics.Family;
                Debug.Log("[GameplayUIRecycle] Setting merchant image for family: " + family);
                merchantImage.sprite = UISpriteLoader.GetDefaultSprite(CardType.Partisan, family, "Merchant");
                _merchantImageInitialized = true;
            }

            // Update totals
            int totalEloquence = _recycleHandler.GetTotalEloquence();
            int totalSouls = _recycleHandler.GetTotalSouls();
            int cardCount = _recycleHandler.SelectedCards.Count;

            cardCountText.text = cardCount.ToString();
            eloquenceText.text = totalEloquence.ToString();
            soulsText.text = totalSouls.ToString();
        }

        private void CleanupAndClose()
        {
            List<GameObject> playerCards = PlayerController.Local.Context.HandManager.HandCards;
            foreach (var cardGO in playerCards)
            {
                CardDisplay card = cardGO.GetComponent<CardDisplay>();
                card.SetSelected(false);
                card.SetClickHandler(_previousClickHandler);
            }
            _recycleHandler.OnSelectionChanged -= RefreshUI;

            Controller.Hide();

            cardCountText.text = "0";
            eloquenceText.text = "0";
            soulsText.text = "0";
        }

        #endregion

        #region Events

        private void OnButtonValidate()
        {
            foreach (var card in _recycleHandler.SelectedCards)
            {
                PlayerController.Local.OnRecycle(card.ID);
                PlayerController.Local.Context.HandManager.RemoveCard(card);
            }

            CleanupAndClose();
        }

        private void OnButtonCancel()
        {
            CleanupAndClose();
        }

        #endregion
    }
}