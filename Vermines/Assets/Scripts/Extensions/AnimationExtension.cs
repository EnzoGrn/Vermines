using UnityEngine;

namespace Vermines.Extension {

    public static class AnimationExtension {

        #region Animation

        public static void PlayForward(this Animation animation, bool reset = false)
        {
            animation.PlayForward(animation.clip, reset);
        }

        public static void PlayForward(this Animation animation, AnimationClip clip, bool reset = false)
        {
            animation.Play(clip.name, 1f, reset);
        }

        public static void PlayForward(this Animation animation, string clipName, bool reset = false)
        {
            animation.Play(clipName, 1f, reset);
        }

        public static void PlayBackward(this Animation animation, bool reset = false)
        {
            animation.PlayBackward(animation.clip, reset);
        }

        public static void PlayBackward(this Animation animation, AnimationClip clip, bool reset = false)
        {
            animation.Play(clip.name, -1f, reset);
        }

        public static void PlayBackward(this Animation animation, string clipName, bool reset = false)
        {
            animation.Play(clipName, -1f, reset);
        }

        public static void Play(this Animation animation, string clipName, float speed, bool reset = false)
        {
            var      state = animation[clipName];
            bool isPlaying = state.enabled && state.weight > 0f;

            if (!isPlaying || reset)
                state.time = speed >= 0f ? 0f : state.length;
            state.speed = speed;

            if (speed != 0f) {
                state.enabled = true;
                state.weight  = 1f;
            }
        }

        public static void SampleStart(this Animation animation)
        {
            animation.SampleStart(animation.clip.name);
        }

        public static void SampleStart(this Animation animation, string clipName)
        {
            animation.Sample(clipName, 0f);
        }

        public static void Sample(this Animation animation, string clipName, float normalizedTime)
        {
            animation.Stop();

            AnimationState state = animation[clipName];

            state.normalizedTime = normalizedTime;
            state.weight         = 1f;
            state.enabled        = true;

            animation.Sample();

            state.enabled = false;
        }

        #endregion

        #region Animator

        public static void PlayForward(this Animator animator, string stateName, bool reset = false, int layer = 0)
        {
            animator.Play(stateName, layer, reset ? 0f : animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
        }

        public static void PlayForward(this Animator animator, bool reset = false, int layer = 0)
        {
            var clipName = GetDefaultStateName(animator, layer);

            if (clipName != null)
                animator.PlayForward(clipName, reset, layer);
        }

        public static void PlayBackward(this Animator animator, string stateName, bool reset = false, int layer = 0)
        {
            animator.Play(stateName, layer, reset ? 1f : animator.GetCurrentAnimatorStateInfo(layer).normalizedTime);
        }

        public static void PlayBackward(this Animator animator, bool reset = false, int layer = 0)
        {
            var clipName = GetDefaultStateName(animator, layer);

            if (clipName != null)
                animator.PlayBackward(clipName, reset, layer);
        }

        public static void SampleStart(this Animator animator, string stateName, int layer = 0)
        {
            animator.Sample(stateName, 0f, layer);
        }

        public static void SampleStart(this Animator animator, int layer = 0)
        {
            var clipName = GetDefaultStateName(animator, layer);

            if (clipName != null)
                animator.SampleStart(clipName, layer);
        }

        public static void Sample(this Animator animator, string stateName, float normalizedTime, int layer = 0)
        {
            if (animator == null)
                return;
            int hash = Animator.StringToHash(stateName);

            if (!animator.HasState(layer, hash)) {
                Debug.LogWarning($"Animator does not contain state '{stateName}' on layer {layer}.");

                return;
            }

            animator.Play(hash, layer, normalizedTime);
            animator.Update(0f);
        }

        private static string GetDefaultStateName(Animator animator, int layer = 0)
        {
            if (animator == null)
                return null;
            var info = animator.GetCurrentAnimatorStateInfo(layer);

            return info.IsName("") ? null : info.shortNameHash.ToString();
        }

        #endregion
    }
}
