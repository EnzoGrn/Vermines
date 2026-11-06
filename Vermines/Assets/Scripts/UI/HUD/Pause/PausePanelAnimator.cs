using DG.Tweening;
using UnityEngine;

// Drives the pause scroll: dim fade, scroll drop, staggered entries, logo pop.
// Pure presentation. Owned by UIGameplayMenu, which decides when to call it.
public class PausePanelAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup _Root;      // whole view (alpha + raycasts)
    [SerializeField] private CanvasGroup _Dim;       // BlurBackground group
    [SerializeField] private RectTransform _Panel;   // the scroll
    [SerializeField] private RectTransform[] _Items; // Resume / Settings / Quit, top to bottom
    [SerializeField] private RectTransform _Logo;

    [SerializeField] private float _DropDistance = 900f;
    [SerializeField] private float _DimAlpha = 1f;
    [SerializeField] private float _ItemStagger = 0.07f;
    [SerializeField] private float _ItemSlide = 70f;

    private Vector2 _PanelRest;
    private Vector2[] _ItemRest;
    private CanvasGroup[] _ItemGroups;
    private Vector3 _LogoScale;

    private void Awake()
    {
        _PanelRest = _Panel.anchoredPosition;
        _ItemRest = new Vector2[_Items.Length];
        _ItemGroups = new CanvasGroup[_Items.Length];

        for (int i = 0; i < _Items.Length; i++)
        {
            _ItemRest[i] = _Items[i].anchoredPosition;
            _ItemGroups[i] = _Items[i].GetComponent<CanvasGroup>() ?? _Items[i].gameObject.AddComponent<CanvasGroup>();
        }
        if (_Logo != null) _LogoScale = _Logo.localScale;

        Snap(false);
    }

    private void OnDestroy() => DOTween.Kill(this);

    // Instant state, no animation.
    public void Snap(bool visible)
    {
        DOTween.Kill(this);
        _Root.alpha = visible ? 1f : 0f;
        _Root.blocksRaycasts = visible;
        _Dim.alpha = visible ? _DimAlpha : 0f;
        _Panel.anchoredPosition = visible ? _PanelRest : _PanelRest + Vector2.up * _DropDistance;

        for (int i = 0; i < _Items.Length; i++)
        {
            _ItemGroups[i].alpha = visible ? 1f : 0f;
            _Items[i].anchoredPosition = visible ? _ItemRest[i] : _ItemRest[i] + Vector2.left * _ItemSlide;
        }
        if (_Logo != null) _Logo.localScale = visible ? _LogoScale : Vector3.zero;
    }

    public Tween Show()
    {
        DOTween.Kill(this);
        _Root.alpha = 1f;
        _Root.blocksRaycasts = true;

        Sequence s = DOTween.Sequence().SetTarget(this).SetUpdate(true);
        s.Join(_Dim.DOFade(_DimAlpha, 0.25f));
        s.Join(_Panel.DOAnchorPos(_PanelRest, 0.5f).SetEase(Ease.OutBack, 1.1f));

        for (int i = 0; i < _Items.Length; i++)
        {
            float at = 0.3f + i * _ItemStagger;
            s.Insert(at, _ItemGroups[i].DOFade(1f, 0.2f));
            s.Insert(at, _Items[i].DOAnchorPos(_ItemRest[i], 0.3f).SetEase(Ease.OutCubic));
        }
        if (_Logo != null)
            s.Insert(0.3f + _Items.Length * _ItemStagger, _Logo.DOScale(_LogoScale, 0.35f).SetEase(Ease.OutBack));

        return s;
    }

    // aside = true when settings open on top: lighter, faster, keeps the dim.
    public Tween Hide(bool aside = false)
    {
        DOTween.Kill(this);
        _Root.blocksRaycasts = false;

        Sequence s = DOTween.Sequence().SetTarget(this).SetUpdate(true);

        for (int i = 0; i < _Items.Length; i++)
        {
            s.Join(_ItemGroups[i].DOFade(0f, 0.12f));
            s.Join(_Items[i].DOAnchorPos(_ItemRest[i] + Vector2.left * (_ItemSlide * 0.5f), 0.15f));
        }
        if (_Logo != null) s.Join(_Logo.DOScale(0f, 0.15f).SetEase(Ease.InBack));

        s.Insert(0.05f, _Panel.DOAnchorPos(_PanelRest + Vector2.up * _DropDistance, aside ? 0.25f : 0.35f).SetEase(Ease.InBack));

        if (!aside)
        {
            s.Insert(0.15f, _Dim.DOFade(0f, 0.25f));
            s.OnComplete(() => _Root.alpha = 0f);
        }
        return s;
    }
}
