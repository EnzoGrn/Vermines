using System.Collections;
using UnityEngine;

namespace Vermines.UI
{
    /// <summary>
    /// Shared "play an Animator state and wait for it to finish" helper, with
    /// the mobile framerate bump this project applies during UI transitions.
    /// Used by both GameplayUIScreen and PlayerBannerUI (previously
    /// duplicated verbatim in both).
    /// </summary>
    public static class AnimatedTransition
    {
        public static IEnumerator PlayAndWait(Animator animator, int stateHash, bool adjustFramerate = true)
        {
            if (animator == null || !animator.gameObject.activeInHierarchy || !animator.HasState(0, stateHash))
                yield break;

#if UNITY_IOS || UNITY_ANDROID
            bool changedFramerate = false;
 
            if (adjustFramerate && Config.AdaptFramerateForMobilePlatform && Application.targetFrameRate < 60)
            {
                Application.targetFrameRate = 60;
                changedFramerate = true;
            }
#endif

            animator.Play(stateHash, 0, 0f);

            yield return null; // wait one frame for the animation to start

            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;

#if UNITY_IOS || UNITY_ANDROID
            if (changedFramerate)
                new FusionMenuGraphicsSettings().Apply();
#endif
        }
    }
}
