using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vermines.CardSystem.Data;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;
using Vermines.Player;
using Vermines.ShopSystem.Enumerations;
using Vermines.UI.Plugin;
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

        /// <summary>
        /// Should show plugins is a flag that can be used to hide the plugin UI elements.
        /// </summary>
        protected override bool ShouldShowPlugins => false;

        [Header("Shop Configs")]

        /// <summary>
        /// The list of shop configs ScriptableObjects.
        /// </summary>
        public List<ShopUIConfigEntry> shopConfigEntries;

        protected Dictionary<ShopType, ShopUIConfig> shopConfigs = new();
        protected Dictionary<ShopType, List<ICard>> previousShopStates = new();

        protected override bool ShouldHidePlugins => false;

        /// <summary>
        /// The button to close the shop UI.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        private GameObject _CloseButton;

        /// <summary>
        /// The type of shop to display (e.g., Market, Courtyard, etc.).
        /// </summary>
        protected ShopType _shopType;

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

            // Get the ShopUIController in the plugin list.
            ShopUIController shopUIController = Get<ShopUIController>();
            if (shopUIController == null)
            {
                Debug.LogErrorFormat(gameObject, "[{0}] Error: {1}", nameof(GameplayUIShop), "ShopUIController not found in plugins.");
                return;
            }
            var entries = GetEntries(_shopType);
            shopUIController.Init(entries, shopConfigs[_shopType]);
            ShowUser();

            foreach (var plugin in Plugins)
            {
                if (plugin is not ShopPopupPlugin)
                {
                    plugin.Show(this);
                }
            }

            GameEvents.OnCardClicked.AddListener(OnCardClicked);
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();

            GameEvents.OnCardClicked.RemoveListener(OnCardClicked);
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
            _shopType = shopType;
        }

        public List<ShopCardEntry> GetEntries(ShopType type)
        {
            if (previousShopStates.TryGetValue(type, out var shopList))
            {
                List<ShopCardEntry> entries = new();
                foreach (var card in shopList)
                {
                    entries.Add(new ShopCardEntry(card));
                }
                return entries;
            }
            return new List<ShopCardEntry>();
        }

        public void ReceiveFullShopList(ShopType type, Dictionary<int, ICard> newList)
        {
            List<ShopCardEntry> entries = new();

            // Get old list as List<ICard>
            List<ICard> oldList = previousShopStates.ContainsKey(type)
                ? previousShopStates[type]
                : new List<ICard>();

            // Sort new list by slot index for consistency
            var sortedNewList = newList.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();

            for (int i = 0; i < sortedNewList.Count; i++)
            {
                ICard newCard = sortedNewList[i];
                bool isNew = i >= oldList.Count || oldList[i]?.ID != newCard?.ID;
                entries.Add(new ShopCardEntry(newCard, isNew));
            }

            previousShopStates[type] = sortedNewList;

            GameEvents.OnShopUpdated.Invoke(type, entries);
        }

        #endregion

        #region Events

        public void OnCardPurchased(ShopType shopType, int cardId)
        {
            if (!previousShopStates.TryGetValue(shopType, out var shopList))
            {
                Debug.LogWarning($"[ShopManager] No shop list found for type {shopType}.");
                return;
            }

            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);
            if (!shopList.Contains(card))
            {
                Debug.LogWarning($"[ShopManager] Card {cardId} not found in shop {shopType}.");
                return;
            }

            // If the card bought is an equipment card, invoke the specific event
            if (PlayerController.Local.PlayerRef == GameManager.Instance.GetCurrentPlayer())
            {
                if (card.Data.Type == CardType.Equipment)
                    GameEvents.OnEquipmentCardPurchased.Invoke(card);
            }

            // Remove the card from the list
            int index = shopList.FindIndex(c => c != null && c.ID == card.ID);
            if (index >= 0)
                shopList[index] = null;

            // Update the shop UI
            ReceiveFullShopList(shopType, GameDataStorage.Instance.Shop.GetDisplayCards(shopType));
        }


        public void OnCardClicked(ICard card, int slodId)
        {
            if (card == null)
                return;
            ShopPopupPlugin plugin = Get<ShopPopupPlugin>();

            if (plugin == null) {
                Debug.LogErrorFormat(gameObject, "[{0}] Critical Error: Missing 'ShopPopupPlugin' reference on GameObject '{1}'. This component is required to render the card list. Please assign a valid GameObject in the Inspector.", nameof(GameplayUIShop), gameObject.name);

                return;
            }

            plugin.SetParam(card);

            if (UIContextManager.Instance.IsInContext<ReplaceEffectContext>()) {
                plugin.Setup((c) => {
                    GameEvents.OnCardClickedInShopWithSlotIndex.Invoke(_shopType, card.ID);
                    GameplayUIController controller = GameObject.FindAnyObjectByType<GameplayUIController>();

                    if (controller != null)
                        controller.ShowLast();
                }, isReplace: true, _shopType);
            } else {
                plugin.Setup((c) => {
                    GameEvents.InvokeOnCardPurchaseRequested(_shopType, card.ID);
                }, isReplace: false, _shopType);
            }

            plugin.Show(this);
        }

        /// <summary>
        /// Is called when the <see cref="_CloseButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            Controller.Hide();
        }

            #endregion
        }
}