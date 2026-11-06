using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vermines.UI.View;

// HUD book button: opens the pause menu. Hangs from the top edge, so the juice goes INTO the screen.
[RequireComponent(typeof(Button))]
public class MenuButtonWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private UIGameplayMenu _Menu;
    [SerializeField] private RectTransform _Visual; // pivot (0.5, 1)
    [SerializeField] private float _HoverScale = 1.1f;
    [SerializeField] private float _HoverDrop = 10f;
    [SerializeField] private float _SwingAngle = 8f;

    private Button _Button;
    private Vector2 _BasePos;
    private Vector3 _BaseScale;

    private void Awake()
    {
        _Button = GetComponent<Button>();
        if (_Visual == null)
            _Visual = (RectTransform)transform;
        _BasePos = _Visual.anchoredPosition;
        _BaseScale = _Visual.localScale;
    }

    private void OnEnable() => _Button.onClick.AddListener(OnClick);

    private void OnDisable()
    {
        _Button.onClick.RemoveListener(OnClick);
        DOTween.Kill(this);
        _Visual.anchoredPosition = _BasePos;
        _Visual.localScale = _BaseScale;
        _Visual.localRotation = Quaternion.identity;
    }

    private void OnClick()
    {
        if (_Menu != null)
            _Menu.Open();
    }

    public void OnPointerEnter(PointerEventData e)
    {
        DOTween.Kill(this);
        _Visual.DOAnchorPos(_BasePos + Vector2.down * _HoverDrop, 0.2f).SetEase(Ease.OutBack).SetUpdate(true).SetTarget(this);
        _Visual.DOScale(_BaseScale * _HoverScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true).SetTarget(this);
        _Visual.localRotation = Quaternion.Euler(0f, 0f, _SwingAngle);
        _Visual.DOLocalRotate(Vector3.zero, 0.7f).SetEase(Ease.OutElastic, 1.2f, 0.4f).SetUpdate(true).SetTarget(this);
    }

    public void OnPointerExit(PointerEventData e)
    {
        DOTween.Kill(this);
        _Visual.DOAnchorPos(_BasePos, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true).SetTarget(this);
        _Visual.DOScale(_BaseScale, 0.2f).SetUpdate(true).SetTarget(this);
        _Visual.DOLocalRotate(Vector3.zero, 0.2f).SetUpdate(true).SetTarget(this);
    }

    public void OnPointerDown(PointerEventData e)
    {
        DOTween.Kill(this);
        _Visual.DOScale(new Vector3(_BaseScale.x * 1.06f, _BaseScale.y * 0.88f, _BaseScale.z), 0.07f).SetUpdate(true).SetTarget(this);
    }

    public void OnPointerUp(PointerEventData e)
    {
        DOTween.Kill(this);
        _Visual.DOScale(_BaseScale * _HoverScale, 0.18f).SetEase(Ease.OutBack).SetUpdate(true).SetTarget(this);
    }
}
