using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Events {

    public class CardEvent {

        public readonly CardWrapper Card;

        public CardEvent(CardWrapper card)
        {
            Card = card;
        }
    }

    public class CardDestroy : CardEvent {

        public CardDestroy(CardWrapper card) : base(card) {}
    }

    public class CardPlayed : CardEvent {

        // TODO: Need to detect if Equipment or not and change display on z axis.

        public CardPlayed(CardWrapper card, PlayedCardList playedCardList,
            DiscardedCardList discardedCardList, bool playedOrDiscard) : base(card) {

            if (card == null)
            {
                Debug.LogError("Card not found.");
                return;
            }
            
            GameObject cardPrefab = Resources.Load<GameObject>(Constants.CardPref);

            if (cardPrefab == null)
            {
                Debug.LogError("Card prefab not found.");
                return;
            }

            if ((playedCardList == null && playedOrDiscard) || (discardedCardList == null && !playedOrDiscard))
            {
                Debug.LogError("Played or Discarded card list not found.");
                return;
            }

            GameObject cardInstance = Object.Instantiate(cardPrefab, cardPrefab.transform.position,
                cardPrefab.transform.rotation) as GameObject;
        
            if (cardInstance == null)
            {
                Debug.LogError("Card instance not created.");
                return;
            }
            
            cardInstance.SetActive(false);
            cardInstance.transform.position = new Vector3(0, 0, 1);
            CardView cardView = cardInstance.GetComponent<CardView>();

            if (cardView == null)
            {
                Debug.LogError("CardView not found.");
                return;
            }

            CardUIView cardUIView = card.GetComponent<CardUIView>();

            if (cardUIView == null)
            {
                Debug.LogError("CardUIView not found.");
                return;
            }

            cardView.SetCard(cardUIView.GetCard());

            cardView.gameObject.transform.Find("Back").gameObject.SetActive(false);

            if (playedOrDiscard)
            {
                playedCardList.AddCard(cardView);
            }
            else
            {
                discardedCardList.AddCard(cardView);
            }
        }
    }

    public class CardHover : CardEvent
    {
        public CardHover(CardWrapper card) : base(card) {}
    }

    public class CardUnhover : CardEvent {

        public CardUnhover(CardWrapper card) : base(card) {}
    }
}
