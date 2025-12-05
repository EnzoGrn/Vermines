using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Vermines.CardSystem.Data.Effect;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;
using Vermines.Player;
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

            GameEvents.OnGameInitialized.AddListener(RefreshUI);
        }

        /// <summary>
        /// The screen show method.
        /// </summary>
        public override void Show()
        {
            base.Show();

            //_previousClickHandler = GameplayController.Instance.Table.CurrentClickHandler;

            _recycleHandler = new RecycleClickHandler();
            _recycleHandler.OnSelectionChanged += RefreshUI;

            //GameplayController.Instance.Table.SetClickHandler(_recycleHandler);

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

            //validateButton.onClick.RemoveListener(OnValidate);
            //cancelButton.onClick.RemoveListener(OnCancel);
        }

        #endregion

        #region Methods

        private void RefreshUI()
        {
            if (merchantImage != null && merchantImage.sprite == null)
            {
                CardFamily family = PlayerController.Local.Statistics.Family;
                Debug.Log("[GameplayUIRecycle] Setting merchant image for family: " + family);
                merchantImage.sprite = UISpriteLoader.GetDefaultSprite(CardType.Partisan, family, "Merchant");
            }

            // Update totals
            int totalEloquence = _recycleHandler.GetTotalEloquence();
            int totalSouls = _recycleHandler.GetTotalSouls();
            int cardCount = _recycleHandler.SelectedCards.Count;

            cardCountText.text = cardCount.ToString();
            eloquenceText.text = totalEloquence.ToString();
            soulsText.text = totalSouls.ToString();

            GameEvents.OnGameInitialized.RemoveListener(RefreshUI);
        }

        private void CleanupAndClose()
        {
            //GameplayController.Instance.Table.SetClickHandler(_previousClickHandler);

            _recycleHandler.OnSelectionChanged -= RefreshUI;
            Controller.Hide();
        }

        #endregion

        #region Events

        private void OnButtonValidate()
        {
            foreach (var card in _recycleHandler.SelectedCards)
            {
                PlayerController.Local.OnRecycle(card.ID);
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