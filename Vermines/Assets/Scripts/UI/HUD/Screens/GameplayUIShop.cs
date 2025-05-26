using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.Gameplay.Phases;
using Vermines.ShopSystem.Enumerations;
using Vermines.UI.Shop;

namespace Vermines.UI.Screen
{
    [Serializable]
    public class ShopUIConfigEntry
    {
        public ShopType shopType;
        public ShopUIConfig config;
    }

    public class ShopCardEntry
    {
        public ICard Data;
        public bool IsNew;

        public ShopCardEntry(ICard data, bool isNew = false)
        {
            Data = data;
            IsNew = isNew;
        }
    }

    public partial class GameplayUIShop : GameplayUIScreen, IParamReceiver<ShopType>
    {
        #region Attributes

        [Header("Shop Configs")]

        /// <summary>
        /// The list of shop configs ScriptableObjects.
        /// </summary>
        public List<ShopUIConfigEntry> shopConfigEntries;

        protected Dictionary<ShopType, ShopUIConfig> shopConfigs = new();
        protected Dictionary<ShopType, Dictionary<int, ICard>> previousShopStates = new();

        [InlineHelp, SerializeField]
        private ShopUIController _ShopController;

        /// <summary>
        /// The type of shop to display (e.g., Market, Courtyard, etc.).
        /// </summary>
        protected ShopType _ShopType;

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

            shopConfigs.Clear();
            foreach (var entry in shopConfigEntries)
            {
                if (entry.config == null)
                {
                    Debug.LogWarningFormat(
                        gameObject,
                        "GameplayUIShop: Init called but config is null for {0}, skipped.",
                        entry.shopType
                    );
                    continue;
                }

                if (!shopConfigs.ContainsKey(entry.shopType))
                {
                    shopConfigs.Add(entry.shopType, entry.config);
                }
                else
                {
                    Debug.LogWarningFormat(
                        gameObject,
                        "GameplayUIShop: Duplicate entry for {0}, ignored.",
                        entry.shopType
                    );
                }
            }

            GameEvents.OnShopRefilled.AddListener(ReceiveFullShopList);

            GameEvents.OnCardPurchased.AddListener(OnCardPurchased);
        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// Will check is the session code is compatible with the party code to toggle the session UI part.
        /// </summary>
        public override void Show()
        {
            base.Show();
            if (shopConfigs.TryGetValue(_ShopType, out var config))
            {
                if (_ShopController != null)
                {
                    var entries = GetEntries(_ShopType);
                    _ShopController.Init(entries, config);
                }
            }
            else
                Debug.LogWarningFormat(gameObject, "[{0}] Warning: {1}", nameof(GameplayUIShop), "SetParam called but no config found for {0}.");

            ShowUser();
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();
            Debug.LogFormat(gameObject, "[{0}] {1}", nameof(GameplayUIShop), "Hide called.");
            GameEvents.OnCardPurchased.RemoveListener(OnCardPurchased);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the shop type and load corresponding data.
        /// </summary>
        /// <param name="shopType">The type of shop to load.</param>
        public void SetParam(ShopType shopType)
        {
            Debug.Log($"[GameplayUIShop] SetParam called with {shopType}.");
            _ShopType = shopType;
        }

        public List<Vermines.UI.Screen.ShopCardEntry> GetEntries(ShopType type)
        {
            if (previousShopStates.TryGetValue(type, out var shopList))
            {
                List<ShopCardEntry> entries = new();
                foreach (var kvp in shopList)
                {
                    entries.Add(new ShopCardEntry(kvp.Value));
                }
                return entries;
            }
            return new List<ShopCardEntry>();
        }

        public void ReceiveFullShopList(ShopType type, Dictionary<int, ICard> newList)
        {
            List<ShopCardEntry> entries = new();

            // Check if there's an old list for this shop type
            Dictionary<int, ICard> oldList = previousShopStates.ContainsKey(type)
                ? previousShopStates[type]
                : new Dictionary<int, ICard>();

            foreach (var kvp in newList)
            {
                int slotIndex = kvp.Key;
                ICard newCard = kvp.Value;

                // Check if the card is new or has changed in the slot
                bool isNew = !oldList.TryGetValue(slotIndex, out var oldCard) || oldCard?.ID != newCard?.ID;

                entries.Add(new ShopCardEntry(newCard, isNew));
            }

            // Update the previous shop list
            previousShopStates[type] = new Dictionary<int, ICard>(newList);

            // Notification
            GameEvents.OnShopUpdated.Invoke(type, entries);
        }

        #endregion

        #region Events

        public void OnCardPurchased(ShopType shopType, int slotIndex)
        {
            if (!previousShopStates.TryGetValue(shopType, out var shopList))
            {
                Debug.LogWarning($"[ShopManager] No shop list found for type {shopType}");
                return;
            }

            if (!shopList.ContainsKey(slotIndex))
            {
                Debug.LogWarning($"[ShopManager] Slot index {slotIndex} not found in shop {shopType}");
                return;
            }

            Debug.Log($"[ShopManager] Card purchased from {shopType} shop at slot {slotIndex}");

            shopList[slotIndex] = null;

            ReceiveFullShopList(shopType, shopList);
        }

        /// <summary>
        /// Is called when the <see cref="_CloseButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            UIContextManager.Instance.PopContextOfType<ShopConfirmPopup>();
            Controller.Hide();
            CamManager.Instance.GoOnNoneLocation();
        }

        #endregion
    }
}