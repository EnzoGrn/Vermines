using DG.Tweening;
using UnityEngine;

namespace Vermines.UI {

    public class PUI_CardAnimationController {

        public static void PlayGhostShake(PUI_CardView view)
        {
            var rt = view.GetComponent<RectTransform>();

            rt.DOShakeAnchorPos(0.6f, strength: 6f, vibrato: 10).SetEase(Ease.OutQuad);
        }

        public static void DelayedPop(PUI_CardView view, float delay)
        {
            var rt = view.GetComponent<RectTransform>();
            var cg = view.GetComponent<CanvasGroup>();

            Sequence seq = DOTween.Sequence();

            seq.AppendInterval(delay);
            seq.AppendCallback(() => view.SetGhost(false));
            seq.Append(rt.DOScale(1.15f, 0.15f));
            seq.Append(rt.DOScale(1f, 0.1f));
            seq.Join(cg.DOFade(1f, 0.2f));
        }

        public static void PlayRemove(PUI_CardView view, System.Action onComplete)
        {
            var rt = view.GetComponent<RectTransform>();
            var cg = view.GetComponent<CanvasGroup>();

            Sequence seq = DOTween.Sequence();
            seq.Append(rt.DOScale(0.85f, 0.15f));
            seq.Join(cg.DOFade(0f, 0.15f));
            seq.OnComplete(() => onComplete?.Invoke());
        }
    }
}
