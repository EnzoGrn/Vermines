using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.UI.Card
{

    using Vermines.CardSystem.Elements;
    using Vermines.Core.Scene;
    using Vermines.Gameplay.Phases;
    using Vermines.Player;

    public class HandManager : SceneService
    {

        #region Attributes

        [Header("References")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private HandLayout handLayout;

        [Header("Settings")]
        [SerializeField] private int maxHandSize = 50;

        private readonly List<GameObject> handCards = new();

        #endregion

        #region Getters & Setters

        public bool HasCards() => handCards.Count > 0;

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            GameEvents.OnCardDrawn.AddListener(DrawCard);
        }

        protected override void OnDeinitialize()
        {
            GameEvents.OnCardDrawn.RemoveListener(DrawCard);

            base.OnDeinitialize();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            PlayerController player = Context.NetworkGame.GetPlayer(Context.Runner.LocalPlayer);

            ResyncHand(player.Deck.Hand);
        }

        #endregion

        #region Public Methods

        public void ResyncHand(List<ICard> hand)
        {
            if (hand == null)
                return;
            List<GameObject> toRemove = new();

            foreach (GameObject cardGO in handCards)
            {
                if (cardGO.TryGetComponent<CardDisplay>(out var display))
                {
                    bool stillExists = hand.Exists(c => c.ID == display.Card.ID);

                    if (!stillExists)
                        toRemove.Add(cardGO);
                }
            }


            foreach (var cardGO in toRemove)
                RemoveCard(cardGO);
            foreach (ICard card in hand)
            {
                bool alreadyInHand = handCards.Exists(go => go.TryGetComponent<CardDisplay>(out var display) && display.Card.ID == card.ID);

                if (!alreadyInHand)
                    DrawCard(card);
            }

            RefreshHand();
        }

        public void DrawCard(ICard card)
        {
            if (handCards.Count >= maxHandSize)
                return;
            Debug.Log($"[HandManager] Drawing card: {card.Data.Name}");

            GameObject cardGO = CreateCard();

            if (cardGO.TryGetComponent<CardDisplay>(out var display))
            {
                var clickHandler = new HandClickHandler();

                display.Display(card, clickHandler);
            }
            else
                Debug.LogWarning("CardDisplay component missing on card prefab.");
            handCards.Add(cardGO);

            RefreshHand();
        }

        public void DrawCards(List<ICard> cards, int count)
        {
            for (int i = 0; i < count && i < cards.Count; i++)
                DrawCard(cards[i]);
        }

        public void RemoveCard(GameObject card)
        {
            handCards.Remove(card);

            card.transform.DOKill(true);

            RefreshHand();
        }

        public void RemoveCard(ICard card)
        {
            GameObject go = GetCardDisplayGO(card);

            if (go != null)
            {
                RemoveCard(go);

                go.transform.DOKill(true);

                Destroy(go);
            }
        }

        public void AddCard(GameObject card)
        {
            if (!handCards.Contains(card))
                handCards.Add(card);
            RefreshHand();
        }

        public void DiscardAllCards(PhaseAsset phase)
        {
            List<GameObject> cards = new(handCards);

            foreach (var card in cards)
            {
                CardDisplay display = card.GetComponent<CardDisplay>();

                if (display != null && phase is ActionPhaseAsset actionPhase)
                    actionPhase.OnDiscardNoEffect(display.Card);
            }
        }

        public GameObject GetCardDisplayGO(ICard card)
        {
            foreach (var cardGO in handCards)
            {
                if (cardGO.TryGetComponent<CardDisplay>(out var display) && display.Card.ID == card.ID)
                    return cardGO;
            }

            return null;
        }

        #endregion

        #region Private Methods

        private GameObject CreateCard()
        {
            GameObject card = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation, handLayout.transform);

            card.name = $"Card_{handCards.Count + 1}";
            card.transform.localScale = Vector3.one * 2f;

            return card;
        }

        private void RefreshHand()
        {
            handLayout.UpdateLayout(handCards);
        }

        #endregion
    }
}