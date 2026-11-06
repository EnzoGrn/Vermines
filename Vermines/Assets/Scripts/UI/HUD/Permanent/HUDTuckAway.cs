using DG.Tweening;
using UnityEngine;
using Vermines.UI.View;

// Slides a HUD group off-screen while the pause flow is engaged, and back when it ends.
public class HUDTuckAway : MonoBehaviour
{
    [SerializeField] private RectTransform _Target;
    [SerializeField] private Vector2 _HiddenOffset = new Vector2(600f, 0f);
    [SerializeField] private float _Duration = 0.35f;
    [SerializeField] private float _Delay = 0f;

    private Vector2 _ShownPos;
    private CanvasGroup _Group;

    private void Awake()
    {
        if (_Target == null)
            _Target = (RectTransform)transform;
        _ShownPos = _Target.anchoredPosition;
        _Group = _Target.GetComponent<CanvasGroup>();
        if (_Group == null)
            _Group = _Target.gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        UIGameplayMenu.EngagedChanged += OnEngagedChanged;
        Apply(UIGameplayMenu.IsEngaged, true);
    }

    private void OnDisable()
    {
        UIGameplayMenu.EngagedChanged -= OnEngagedChanged;
        DOTween.Kill(this);
    }

    private void OnEngagedChanged(bool engaged) => Apply(engaged, false);

    private void Apply(bool engaged, bool instant)
    {
        DOTween.Kill(this);
        Vector2 to = engaged ? _ShownPos + _HiddenOffset : _ShownPos;
        _Group.blocksRaycasts = !engaged; // no clicks on a hidden HUD

        if (instant)
        {
            _Target.anchoredPosition = to;
            return;
        }

        _Target.DOAnchorPos(to, _Duration)
               .SetDelay(_Delay)
               .SetEase(engaged ? Ease.InBack : Ease.OutBack)
               .SetUpdate(true)
               .SetTarget(this);
    }
}
