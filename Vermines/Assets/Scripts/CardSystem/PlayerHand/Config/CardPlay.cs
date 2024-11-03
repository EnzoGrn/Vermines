using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config {

    [Serializable]
    public class CardPlay {

        [SerializeField]
        public RectTransform PlayArea;

        [SerializeField]
        public RectTransform DiscardArea;

        [SerializeField]
        public bool DestroyOnPlay;

        [SerializeField]
        public bool DestroyOnDiscard;
    }
}