namespace Vermines.Extension {

    using Vermines.Core.Audio;

    public static class AudioEffectExtension {

        public static bool PlaySound(this AudioEffect[] effects, AudioSetup setup, ForceBehaviour force = ForceBehaviour.None) {
            if (effects == null)
                return false;
            AudioEffect bestPlayingEffect = null;
            float bestTime = 0.5f;

            for (int i = 0; i < effects.Length; i++) {
                AudioEffect audioEffect = effects[i];

                if (audioEffect.IsPlaying == false) {
                    audioEffect.Play(setup);

                    return true;
                }
                bool chooseAudioEffect = false;

                switch (force) {
                    case ForceBehaviour.ForceDifferentSetup:
                        chooseAudioEffect = audioEffect.AudioSource.time > bestTime && audioEffect.CurrentSetup != setup;

                        break;
                    case ForceBehaviour.ForceSameSetup:
                        chooseAudioEffect = audioEffect.AudioSource.time > bestTime && audioEffect.CurrentSetup == setup;

                        break;
                    case ForceBehaviour.ForceAny:
                        chooseAudioEffect = audioEffect.AudioSource.time > bestTime;

                        break;
                }

                if (chooseAudioEffect) {
                    bestPlayingEffect = audioEffect;
                    bestTime          = audioEffect.AudioSource.time;
                }
            }

            if (force == ForceBehaviour.None)
                return false;
            if (bestPlayingEffect != null) {
                bestPlayingEffect.Play(setup, force);

                return true;
            }

            return false;
        }
    }
}
