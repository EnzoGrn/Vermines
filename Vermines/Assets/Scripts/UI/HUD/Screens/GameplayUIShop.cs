using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vermines.CardSystem.Data;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;
using Vermines.Core.Scene;
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

        protected override bool ShouldShowPlugins => false;
        protected override bool ShouldHidePlugins => false;

        [Header("Shop Configs")]
        public List<ShopUIConfigEntry> shopConfigEntries;

        protected Dictionary<ShopType, ShopUIConfig> shopConfigs = new();
        protected Dictionary<ShopType, List<ICard>> previousShopStates = new();


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

        private SceneContext _cachedContext;
        private SceneContext Context
        {
            get
            {
                if (_cachedContext == null)
                    _cachedContext = PlayerController.Local.Context;
                return _cachedContext;
            }
        }

        private GameplayUIController _cachedUIController;
        private GameplayUIController UIController
        {
            get
            {
                if (_cachedUIController == null)
                    _cachedUIController = FindAnyObjectByType<GameplayUIController>();
                return _cachedUIController;
            }
        }

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

            LoadShopConfigs();

            GameEvents.OnShopRefilled.AddListener(ReceiveFullShopList);
            GameEvents.OnCardPurchased.AddListener(OnCardPurchased);

            Resync();
        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// Will check is the session code is compatible with the party code to toggle the session UI part.
        /// </summary>
        public override void Show()
        {
            base.Show();

            Resync();

            ShopUIController shopUIController = Get<ShopUIController>();
            if (shopUIController == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "[{0}] Error: ShopUIController not found in plugins.",
                    nameof(GameplayUIShop)
                );
                return;
            }
            shopUIController.Init(GetEntries(_shopType), shopConfigs[_shopType]);

            ShowUser();

            ShowPluginsExcept<ShopPopupPlugin>();

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

        private void Resync()
        {
            foreach (var section in Context.GameplayMode.Shop.Sections)
            {
                var displayCards = Context.GameplayMode.Shop.GetDisplayCards(section.Key);
                ReceiveFullShopList(section.Key, displayCards);
            }
        }

        private void LoadShopConfigs()
        {
            shopConfigs.Clear();

            foreach (var entry in shopConfigEntries)
            {
                if (entry.config == null)
                {
                    Debug.LogWarningFormat(
                        gameObject,
                        "[{0}] Config is null for {1}, skipped.",
                        nameof(GameplayUIShop),
                        entry.shopType
                    );
                    continue;
                }

                if (shopConfigs.ContainsKey(entry.shopType))
                {
                    Debug.LogWarningFormat(
                        gameObject,
                        "[{0}] Duplicate entry for {1}, ignored.",
                        nameof(GameplayUIShop),
                        entry.shopType
                    );
                    continue;
                }

                shopConfigs.Add(entry.shopType, entry.config);
            }
        }

        /// <summary>
        /// Set the shop type and load corresponding data.
        /// </summary>
        /// <param name="shopType">The type of shop to load.</param>
        public void SetParam(ShopType shopType)
        {
            Debug.Log($"[{nameof(GameplayUIShop)}] SetParam called with {shopType}.");
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
            var oldList = previousShopStates.ContainsKey(type)
                ? previousShopStates[type]
                : new List<ICard>();

            var sortedNewList = newList
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => kvp.Value)
                .ToList();

            var entries = BuildShopEntries(oldList, sortedNewList);

            previousShopStates[type] = sortedNewList;

            GameEvents.OnShopUpdated.Invoke(type, entries);
        }

        private List<ShopCardEntry> BuildShopEntries(List<ICard> oldList, List<ICard> newList)
        {
            var entries = new List<ShopCardEntry>();

            for (int i = 0; i < newList.Count; i++)
            {
                ICard newCard = newList[i];
                bool isNew = i >= oldList.Count || oldList[i]?.ID != newCard?.ID;
                entries.Add(new ShopCardEntry(newCard, isNew));
            }

            return entries;
        }

        private void ShowPluginsExcept<T>() where T : class
        {
            foreach (var plugin in Plugins)
            {
                if (plugin is not T)
                    plugin.Show(this);
            }
        }

        #endregion

        #region Events

        public void OnCardPurchased(ShopType shopType, int cardId)
        {
            if (!previousShopStates.TryGetValue(shopType, out var shopList))
            {
                Debug.LogWarningFormat(
                    gameObject,
                    "[{0}] No shop list found for type {1}.",
                    nameof(GameplayUIShop),
                    shopType
                );
                return;
            }

            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);
            if (card == null || !shopList.Contains(card))
            {
                Debug.LogWarningFormat(
                    gameObject,
                    "[{0}] Card {1} not found in shop {2}.",
                    nameof(GameplayUIShop),
                    cardId,
                    shopType
                );
                return;
            }

            if (Context.GameplayMode.IsMyTurn)
                HandleEquipmentPurchase(card);

            RemoveCardFromShop(shopType, shopList, card);
        }

        private void HandleEquipmentPurchase(ICard card)
        {
            if (card.Data.Type == CardType.Equipment)
                GameEvents.OnEquipmentCardPurchased.Invoke(card);
        }

        private void RemoveCardFromShop(ShopType shopType, List<ICard> shopList, ICard card)
        {
            int index = shopList.FindIndex(c => c != null && c.ID == card.ID);
            if (index >= 0)
                shopList[index] = null;

            var displayCards = Context.GameplayMode.Shop.GetDisplayCards(shopType);
            ReceiveFullShopList(shopType, displayCards);
        }


        public void OnCardClicked(ICard card, int slodId)
        {
            if (card == null) return;

            ShopPopupPlugin plugin = Get<ShopPopupPlugin>();
            if (plugin == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "[{0}] Critical Error: Missing 'ShopPopupPlugin' on '{1}'.",
                    nameof(GameplayUIShop),
                    gameObject.name
                );
                return;
            }

            plugin.SetParam(card);

            if (UIContextManager.Instance.IsInContext<ReplaceEffectContext>())
                SetupReplaceMode(plugin, card);
            else
                SetupPurchaseMode(plugin, card);

            plugin.Show(this);
        }

        private void SetupReplaceMode(ShopPopupPlugin plugin, ICard card)
        {
            plugin.Setup(_ =>
            {
                GameEvents.OnCardClickedInShopWithSlotIndex.Invoke(_shopType, card.ID);
                if (UIController) UIController.ShowLast();
            }, isReplace: true, _shopType);
        }

        private void SetupPurchaseMode(ShopPopupPlugin plugin, ICard card)
        {
            plugin.Setup(_ =>
            {
                GameEvents.InvokeOnCardPurchaseRequested(_shopType, card.ID);
            }, isReplace: false, _shopType);
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