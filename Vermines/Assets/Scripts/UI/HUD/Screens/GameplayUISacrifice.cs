using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.UI.Plugin;
using Fusion;
using Vermines.CardSystem.Enumerations;
using Vermines.UI.Card;
using System.Collections.Generic;
using Vermines.Player;

namespace Vermines.UI.Screen
{
    public partial class GameplayUISacrifice : GameplayUIScreen, IParamReceiver<CardType>, ICardClickReceiver
    {
        #region Attributes

        /// <summary>
        /// Should show plugins is a flag that can be used to hide the plugin UI elements.
        /// </summary>
        protected override bool ShouldShowPlugins => false;

        /// <summary>
        /// The type of deck to be displayed.
        /// </summary>
        private CardType _deckType = CardType.Partisan;

        protected List<ShopCardSlot> activeSlots = new();

        protected List<Vermines.UI.Screen.ShopCardEntry> currentEntries = new();

        /// <summary>
        /// The banner holder that contains the card list.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        private GameObject _cardHolder;

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
        }

        /// <summary>
        /// Set the shop type and load corresponding data.
        /// </summary>
        /// <param name="cardType">The type of shop to load.</param>
        public void SetParam(CardType cardType)
        {
            Debug.Log($"[GameplayUIShop] SetParam called with {cardType}.");
            _deckType = cardType;
        }

        #endregion

        #region Methods

        protected virtual void PopulateSlots()
        {
            foreach (var slot in activeSlots)
            {
                CardSlotPool.Instance.ReturnSlot(slot);
            }
            activeSlots.Clear();

            if (currentEntries == null || currentEntries.Count == 0)
            {
                // TODO: show empty state or message
            }

            for (int i = 0; i < currentEntries.Count; i++)
            {
                Vermines.UI.Screen.ShopCardEntry entry = currentEntries[i];
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

                slot.Init(entry.Data, entry.IsNew, new CardClickHandler(this));

                activeSlots.Add(slot);
            }
        }

        protected void GetCardFromType(CardType type)
        {
            currentEntries.Clear();

            foreach (var card in GameDataStorage.Instance.PlayerDeck[PlayerController.Local.PlayerRef].Hand)
            {
                if (card.Data.Type == type)
                {
                    currentEntries.Add(new ShopCardEntry(card));
                }
            }

            foreach (var card in GameDataStorage.Instance.PlayerDeck[PlayerController.Local.PlayerRef].Equipments)
            {
                if (card.Data.Type == type)
                {
                    currentEntries.Add(new ShopCardEntry(card));
                }
            }

            foreach (var card in GameDataStorage.Instance.PlayerDeck[PlayerController.Local.PlayerRef].PlayedCards)
            {
                if (card.Data.Type == type)
                {
                    currentEntries.Add(new ShopCardEntry(card));
                }
            }

            foreach (var card in GameDataStorage.Instance.PlayerDeck[PlayerController.Local.PlayerRef].Discard)
            {
                if (card.Data.Type == type)
                {
                    currentEntries.Add(new ShopCardEntry(card));
                }
            }
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

            RemovePopupPlugin plugin = Get<RemovePopupPlugin>();
            if (plugin == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "[{0}] Critical Error: Missing 'RemovePopupPlugin' reference on GameObject '{1}'. This component is required to render the card list. Please assign a valid GameObject in the Inspector.",
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

    public interface ICardClickReceiver
    {
        void OnCardClicked(ICard card, int slodId);
    }

    public class CardClickHandler : ICardClickHandler
    {
        private readonly ICardClickReceiver _receiver;

        public CardClickHandler(ICardClickReceiver receiver)
        {
            _receiver = receiver;
        }

        public void OnCardClicked(ICard card)
        {
            _receiver.OnCardClicked(card, 0);
        }
    }
}