using System.Collections;
using UnityEngine;

namespace Vermines.Extension {

    public static class AudioSourceExtension {

        #region Constants

        private const float MIN_DURATION = 0.05f;

        #endregion

        #region Methods

        public static Coroutine FadeIn(this AudioSource source, MonoBehaviour behavior, float duration = 1f, float delay = 0f, float volume = 1f)
        {
            if (duration < MIN_DURATION && delay <= 0f) {
                source.Play();
                source.volume = volume;

                return null;
            }
            source.volume = 0f;
            return behavior.StartCoroutine(Fade_Coroutine(source, volume, duration, delay));
        }

        public static Coroutine FadeOut(this AudioSource source, MonoBehaviour behavior, float duration = 1f, float delay = 0f)
        {
            if ((duration < MIN_DURATION && delay <= 0f) || !source.isPlaying) {
                source.Stop();
                source.volume = 0f;

                return null;
            }

            return behavior.StartCoroutine(Fade_Coroutine(source, 0f, duration, delay));
        }

        private static IEnumerator Fade_Coroutine(AudioSource source, float targetVolume, float duration, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);
            float startVolume = source.volume;
            float time        = 0f;

            if (!source.isPlaying)
                source.Play();
            while (time < duration) {
                source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);

                time += Time.deltaTime;

                yield return null;
            }
            source.volume = targetVolume;

            if (targetVolume == 0f)
                source.Stop();
        }

        #endregion
    }
}
