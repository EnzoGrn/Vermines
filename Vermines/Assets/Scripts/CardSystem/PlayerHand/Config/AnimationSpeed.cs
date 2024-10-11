using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Config {

    [Serializable]
    public class AnimationSpeed {

        [SerializeField]
        [Tooltip("Animation speed in degrees per second.")]
        public float Rotation = 60f;

        [SerializeField]
        public float Position = 500f;

        [SerializeField]
        public float ReleasePosition = 2000f;

        [SerializeField]
        public float Zoom = 0.3f;
    }
}
