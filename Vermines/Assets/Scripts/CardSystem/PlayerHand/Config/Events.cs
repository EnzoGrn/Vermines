using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Config {

    [Serializable]
    public class AllEvents {

        [SerializeField]
        public UnityEvent<Events.CardPlayed> OnCardPlayed;

        [SerializeField]
        public UnityEvent<Events.CardDestroy> OnCardDestroy;

        [SerializeField]
        public UnityEvent<Events.CardHover> OnCardHover;

        [SerializeField]
        public UnityEvent<Events.CardUnhover> OnCardUnhover;
    }
}
