using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config {

    [Serializable]
    public class Zoom {

        [SerializeField]
        public bool ZoomOnHover;

        [Range(1f, 5f)]
        [SerializeField]
        public float Multiplier = 1;

        [Tooltip("This is the Y position of the card when it is zoomed in. If this is -1, the card will not be moved on the Y axis.")]
        [SerializeField]
        public float OverrideYPosition = -1;

        [Header("UI Layer")]
        [Tooltip("This is the sorting order of the first card when it is not zoomed in. Subsequent cards will have a higher sorting order.")]
        [SerializeField]
        public int DefaultSortOrder;

        [SerializeField]
        public bool BringToFrontOnHover;

        [Tooltip("This is the sorting order of the card when it is zoomed in.")]
        [SerializeField]
        public int ZoomedSortOrder;

        [SerializeField]
        public bool ResetRotationOnZoom;
    }
}
