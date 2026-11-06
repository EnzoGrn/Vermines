using DG.Tweening;
using UnityEngine;

// Fades + slides children in, one after the other. Call Play() when the container becomes visible.
public class StaggerReveal : MonoBehaviour
{
    [SerializeField] private float _Stagger = 0.05f, _Duration = 0.25f, _Slide = 40f;

    public void Play()
    {
        DOTween.Kill(this);
        for (int i = 0; i < transform.childCount; i++)
        {
            var rt = transform.GetChild(i) as RectTransform;
            if (rt == null || !rt.gameObject.activeSelf) continue;

            var g = rt.GetComponent<CanvasGroup>() ?? rt.gameObject.AddComponent<CanvasGroup>();
            g.alpha = 0f;
            Vector2 rest = rt.anchoredPosition;
            rt.anchoredPosition = rest + Vector2.left * _Slide;

            float delay = i * _Stagger;
            g.DOFade(1f, _Duration).SetDelay(delay).SetUpdate(true).SetTarget(this);
            rt.DOAnchorPos(rest, _Duration).SetDelay(delay).SetEase(Ease.OutCubic).SetUpdate(true).SetTarget(this);
        }
    }
}
