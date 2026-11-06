using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Slide + scale on hover, squash on press, punch on click.
[RequireComponent(typeof(Button))]
public class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform _Visual;
    [SerializeField] private Vector2 _HoverSlide = new Vector2(14f, 0f);
    [SerializeField] private float _HoverScale = 1.05f, _PressScale = 0.96f;

    private Button _Button;
    private Vector2 _BasePos;
    private Vector3 _BaseScale;
    private bool _Ready;

    private void Awake()
    {
        _Button = GetComponent<Button>();
        if (_Visual == null) _Visual = (RectTransform)transform;
    }

    // Captured on enable: layouts and entrance tweens have settled their rest pose by then.
    private void OnEnable()
    {
        _BasePos = _Visual.anchoredPosition;
        _BaseScale = _Visual.localScale;
        _Ready = true;
        _Button.onClick.AddListener(Punch);
    }

    private void OnDisable()
    {
        _Button.onClick.RemoveListener(Punch);
        DOTween.Kill(this);
        if (_Ready) { _Visual.anchoredPosition = _BasePos; _Visual.localScale = _BaseScale; }
    }

    public void OnPointerEnter(PointerEventData e) { if (_Button.interactable) To(_BasePos + _HoverSlide, _BaseScale * _HoverScale, 0.15f); }
    public void OnPointerExit(PointerEventData e) => To(_BasePos, _BaseScale, 0.2f);
    public void OnPointerDown(PointerEventData e) { if (_Button.interactable) To(_BasePos + _HoverSlide, _BaseScale * _PressScale, 0.07f); }
    public void OnPointerUp(PointerEventData e) => To(_BasePos + _HoverSlide, _BaseScale * _HoverScale, 0.1f);

    private void Punch()
    {
        DOTween.Kill(this);
        _Visual.localScale = _BaseScale;
        _Visual.DOPunchScale(_BaseScale * 0.08f, 0.25f, 6, 0.6f).SetUpdate(true).SetTarget(this);
    }

    private void To(Vector2 pos, Vector3 scale, float d)
    {
        DOTween.Kill(this);
        _Visual.DOAnchorPos(pos, d).SetEase(Ease.OutQuad).SetUpdate(true).SetTarget(this);
        _Visual.DOScale(scale, d).SetEase(Ease.OutBack).SetUpdate(true).SetTarget(this);
    }
}
