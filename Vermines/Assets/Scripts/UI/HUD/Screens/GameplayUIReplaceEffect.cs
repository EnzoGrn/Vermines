using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vermines.ShopSystem.Enumerations;

namespace Vermines.UI.Screen
{
    [Serializable]
    public class ShopButtonEntry
    {
        public ShopType shopType;
        public Button button;
    }

    public partial class GameplayUIReplaceEffect : GameplayUIScreen, IParamReceiver<Action<Dictionary<ShopType, int>>>
    {
        #region Attributes

        [Header("Buttons")]

        /// <summary>
        /// The button to validate the choice and apply the effect.
        /// This end the replace effect.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button doneButton;

        [SerializeField] private List<ShopButtonEntry> shopButtons;
        private Dictionary<ShopType, Button> shopTypeToButton;
        private Action<Dictionary<ShopType, int>> onDoneCallback;
        public Dictionary<ShopType, int> shopReplacements = new();

        #endregion

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #region Override Methods

        /// <summary>
        /// The Unity awake method.
        /// Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            AwakeUser();
        }

        /// <summary>
        /// The screen init method.
        /// Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Init()
        {
            base.Init();

            InitUser();

            shopTypeToButton = new Dictionary<ShopType, Button>();
            foreach (var entry in shopButtons)
            {
                Debug.Log($"[ReplaceEffect] Adding button for {entry.shopType} to dictionary");
                shopTypeToButton[entry.shopType] = entry.button;
            }
        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// Will check is the session code is compatible with the party code to toggle the session UI part.
        /// </summary>
        public override void Show()
        {
            base.Show();

            ShowUser();

            GameEvents.OnCardClickedInShopWithSlotIndex.AddListener(OnShopCardClicked);
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();

        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the callback to be called when the effect is done.
        /// </summary>
        /// <param name="onDone">The callback to be called when the effect is done.</param>
        public void SetParam(Action<Dictionary<ShopType, int>> onDone)
        {
            onDoneCallback = onDone;
            shopReplacements.Clear();

            foreach (var kvp in shopTypeToButton)
            {
                var shopType = kvp.Key;
                var button = kvp.Value;

                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OpenReplace(shopType));
            }

            doneButton.onClick.RemoveAllListeners();
            doneButton.onClick.AddListener(OnDoneButtonPressed);
        }

        private void OpenReplace(ShopType shopType)
        {
            Debug.Log($"[ReplaceEffect] Opening shop for {shopType}");

            Controller.ShowWithParams<GameplayUIShop, ShopType>(shopType, this);
        }

        private void SetShopDone(ShopType shopType)
        {
            if (shopTypeToButton.TryGetValue(shopType, out var button))
                button.interactable = false;
        }

        #endregion

        #region Events

        public void OnDoneButtonPressed()
        {
            onDoneCallback?.Invoke(shopReplacements);
            UIContextManager.Instance.PopContext();
            GameEvents.OnCardClickedInShopWithSlotIndex.RemoveListener(OnShopCardClicked);
            Controller.Hide();
        }

        public void OnShopCardClicked(ShopType shopType, int slotId)
        {
            Debug.Log($"[ReplaceEffect] Card clicked in shop: {shopType} at slot {slotId}");
            if (shopReplacements == null)
                shopReplacements = new Dictionary<ShopType, int>();
            if (shopReplacements.ContainsKey(shopType))
            {
                Debug.Log($"[ReplaceEffect] Already have a replacement stored for {shopType} at slot {shopReplacements[shopType]}");
                return;
            }
            Debug.Log($"[ReplaceEffect] Replacing {shopType} at slot {slotId}");
            shopReplacements.Add(shopType, slotId);
            SetShopDone(shopType);
        }

        #endregion
    }
}