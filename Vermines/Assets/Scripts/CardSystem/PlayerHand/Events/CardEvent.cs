using Config;
using System.Collections;
using System.Collections.Generic;
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

        public bool PlayedOrDiscard = true; // True if played, false if discarded

        public CardPlayed(CardWrapper card) : base(card) {}
    }

    public class CardHover : CardPlayed {

        public CardHover(CardWrapper card) : base(card) {}
    }

    public class CardUnhover : CardEvent {

        public CardUnhover(CardWrapper card) : base(card) {}
    }
}
