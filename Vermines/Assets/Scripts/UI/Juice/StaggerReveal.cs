using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines.UI {

    /// <summary>
    /// Fades + slides the active children in, one after the other.
    /// Call Play() when the container becomes visible.
    /// </summary>
    public class StaggerReveal : MonoBehaviour {

        #region Attributes

        [SerializeField] private float _Stagger = 0.05f;
        [SerializeField] private float _Duration = 0.25f;
        [SerializeField] private float _Slide = 40f;

        #endregion

        #region MonoBehaviour Methods

        private void OnDisable()
        {
            // Completing puts every child back to its rest pose, so nothing stays half faded.
            DOTween.Kill(this, true);
        }

        #endregion

        #region Methods

        /// <param name="delay">Wait before the first child; children are hidden right away.</param>
        public void Play(float delay = 0f)
        {
            // Complete, not just kill: a replay mid-reveal must read the rest pose, not a shifted one.
            DOTween.Kill(this, true);

            // Pending layout changes (folded sections, new labels) would snap the children back mid-slide.
            if (transform is RectTransform self)
                LayoutRebuilder.ForceRebuildLayoutImmediate(self);
            int index = 0;

            for (int i = 0; i < transform.childCount; i++) {
                if (transform.GetChild(i) is not RectTransform child || !child.gameObject.activeSelf)
                    continue;
                if (!child.TryGetComponent(out CanvasGroup group))
                    group = child.gameObject.AddComponent<CanvasGroup>();
                Vector2 rest = child.anchoredPosition;

                group.alpha            = 0f;
                child.anchoredPosition = rest + Vector2.left * _Slide;

                float at = delay + index * _Stagger;

                group.DOFade(1f, _Duration).SetDelay(at).SetUpdate(true).SetTarget(this);
                child.DOAnchorPos(rest, _Duration).SetDelay(at).SetEase(Ease.OutCubic).SetUpdate(true).SetTarget(this);

                index++;
            }
        }

        #endregion
    }
}
