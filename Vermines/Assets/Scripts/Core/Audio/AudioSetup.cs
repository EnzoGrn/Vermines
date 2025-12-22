using System;
using UnityEngine;

namespace Vermines.Core.Audio {

    [Serializable]
    public class AudioSetup {

        public AudioClip[] Clips;

        public float Volume = 1f;
        public float PitchShift;
        public float MaxPitchChange;
        public float Delay;

        public bool Loop;

        public float FadeIn;
        public float FadeOut;

        [Space]
        public bool Repeat;
        public int RepeatPlayCount;
        public float RepeatDelay;

        public void CopyFrom(AudioSetup other)
        {
            Clips          = other.Clips;
            Volume         = other.Volume;
            PitchShift     = other.PitchShift;
            MaxPitchChange = other.MaxPitchChange;
            Delay          = other.Delay;
            Loop           = other.Loop;
            FadeIn         = other.FadeIn;
            FadeOut        = other.FadeOut;

            Repeat          = other.Repeat;
            RepeatPlayCount = other.RepeatPlayCount;
            RepeatDelay     = other.RepeatDelay;
        }
    }
}
