using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;
using Vermines.UI.Card;
using Vermines.UI.Plugin;

namespace Vermines.UI.Screen
{
    using Button = UnityEngine.UI.Button;

    public partial class GameplayUICopyEffect : GameplayUIScreen, IParamReceiver<CardCopyEffectContext>, ICardClickReceiver
    {
        #region Attributes

        /// <summary>
        /// Should show plugins is a flag that can be used to hide the plugin UI elements.
        /// </summary>
        protected override bool ShouldShowPlugins => false;

        protected List<ShopCardSlot> activeSlots = new();

        protected List<Vermines.UI.Screen.ShopCardEntry> currentEntries = new();

        /// <summary>
        /// The banner holder that contains the card list.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        private GameObject _cardHolder;

        /// <summary>
        /// The type of deck to be displayed.
        /// </summary>
        private CardType _deckType = CardType.Partisan;

        private ICard activeCard;

        private int currentPage = 0;
        private const int entriesPerPage = 5;
        [SerializeField]
        private Button _nextPageButton;

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
        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// Will check is the session code is compatible with the party code to toggle the session UI part.
        /// </summary>
        public override void Show()
        {
            Debug.Log($"[{nameof(GameplayUICopyEffect)}] Show called with deck type: {_deckType}");
            if (_cardHolder == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "GameplayUISacrifice Critical Error: Missing 'GameObject' reference on GameObject '{0}'. This component is required to render the card list. Please assign a valid GameObject in the Inspector.",
                    gameObject.name
                );
                return;
            }

            GetCardFromType(_deckType);
            PopulateSlots();

            base.Show();

            foreach (var plugin in Plugins)
            {
                if (plugin is not CopyPopupPlugin)
                {
                    plugin.Show(this);
                }
            }

            ShowUser();

            GameEvents.OnCardClicked.AddListener(OnCardClicked);
            _nextPageButton.onClick.AddListener(NextPage);
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
            _nextPageButton.onClick.RemoveListener(NextPage);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the callback to be called when the effect is done.
        /// </summary>
        /// <param name="onDone">The callback to be called when the effect is done.</param>
        public void SetParam(CardCopyEffectContext cardContext)
        {
            _deckType = cardContext.Type;
            activeCard = cardContext.Card;

            Debug.Log($"[{nameof(GameplayUICopyEffect)}] SetParam called with deck type: {_deckType} and card: {activeCard?.Data.Name}");

            foreach (var plugin in Plugins)
            {
                if (plugin is CopyEffectPlugin copyPlugin && copyPlugin.CardTypeTrigger == _deckType)
                {
                    copyPlugin.SetParam(activeCard);
                }
            }
        }

        protected void GetCardFromType(CardType type)
        {
            currentEntries.Clear();
            currentPage = 0;

            foreach (var plugin in Plugins)
            {
                if (plugin is CopyEffectPlugin copyPlugin && copyPlugin.CardTypeTrigger == type)
                {
                    var entries = copyPlugin.GetEntries();
                    currentEntries.AddRange(entries);
                }
            }
        }

        private void NextPage()
        {
            int maxPage = Mathf.CeilToInt((float)currentEntries.Count / entriesPerPage);
            currentPage = (currentPage + 1) % maxPage;
            PopulateSlots();
        }

        protected virtual void PopulateSlots()
        {
            foreach (var slot in activeSlots)
            {
                CardSlotPool.Instance.ReturnSlot(slot);
            }
            activeSlots.Clear();

            int startIndex = currentPage * entriesPerPage;

            for (int i = 0; i < entriesPerPage; i++)
            {
                int entryIndex = startIndex + i;

                var slot = CardSlotPool.Instance.GetSlot(_cardHolder.transform);
                if (slot == null)
                {
                    Debug.LogErrorFormat(
                        gameObject,
                        "[{0}] Critical Error: Failed to get a card slot from the pool. This component is required to render the card list. Please assign a valid GameObject in the Inspector.",
                        nameof(GameplayUISacrifice)
                    );
                    continue;
                }

                slot.transform.SetParent(_cardHolder.transform, false);
                slot.SetIndex(i);

                if (entryIndex < currentEntries.Count)
                {
                    var entry = currentEntries[entryIndex];
                    slot.Init(entry.Data, entry.IsNew, new CardClickHandler(this));
                }
                else
                {
                    slot.ResetSlot();
                }

                activeSlots.Add(slot);
            }

            _nextPageButton.gameObject.SetActive(currentEntries.Count > entriesPerPage);
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_CloseButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            Controller.Hide();
            UIContextManager.Instance.PopContext();
        }

        public void OnCardClicked(ICard card, int slodId)
        {
            if (card == null || GameManager.Instance.IsMyTurn() == false || card.Data.Type != _deckType)
                return;

            CopyPopupPlugin plugin = Get<CopyPopupPlugin>();
            if (plugin == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "[{0}] Critical Error: Missing 'CopyPopupPlugin' reference on GameObject '{1}'. This component is required to render the card list. Please assign a valid GameObject in the Inspector.",
                    nameof(GameplayUISacrifice),
                    gameObject.name
                );
                return;
            }
            plugin.SetParam(card);
            plugin.Show(this);
        }

        #endregion
    }
}