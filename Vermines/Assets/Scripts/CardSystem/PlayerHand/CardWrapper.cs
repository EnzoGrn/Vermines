using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardWrapper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

    private const float EPS = 0.01f; // Epsilon value for float comparaison

    public bool    IsMyContainer;
    public float   TargetRotation;
    public Vector2 TargetPosition;
    public float   TargetVerticalDisplacement;
    public int     UILayer;
    public bool    PreventCardInteraction;

    private bool    _IsHovered;
    private bool    _IsDragged;
    private Vector2 _DragStartPos;

    // -- Components
    // - Public
    public Config.Zoom           ZoomConfig;
    public Config.AnimationSpeed AnimationSpeedConfig;
    public CardContainer         Container;
    public Config.AllEvents      EventsConfig;

    // - Private
    private RectTransform _RectTransform;
    private Canvas        _Canvas;

    public float Width
    {
        get => _RectTransform.rect.width * _RectTransform.localScale.x;
    }

    private void Awake()
    {
        _RectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        _Canvas = GetComponent<Canvas>();
    }

    private void Update()
    {
        UpdateRotation();
        UpdatePosition();
        UpdateScale();
        UpdateUILayer();
    }

    private void UpdateUILayer()
    {
        if (!_IsHovered && !_IsDragged)
            _Canvas.sortingOrder = UILayer;
    }

    private void UpdatePosition()
    {
        if (!_IsDragged) {
            var target = new Vector2(TargetPosition.x, TargetPosition.y + TargetVerticalDisplacement);

            if (_IsHovered && ZoomConfig.OverrideYPosition != -1)
                target = new Vector2(target.x, ZoomConfig.OverrideYPosition);
            var distance        = Vector2.Distance(_RectTransform.position, target);
            var repositionSpeed = _RectTransform.position.y > target.y || _RectTransform.position.y < 0 ? AnimationSpeedConfig.ReleasePosition : AnimationSpeedConfig.Position;

            _RectTransform.position = Vector2.Lerp(_RectTransform.position, target, repositionSpeed / distance * Time.deltaTime);
        } else {
            var delta = ((Vector2)Input.mousePosition + _DragStartPos);

            _RectTransform.position = new Vector2(delta.x, delta.y);
        }
    }

    private void UpdateScale()
    {
        var targetZoom = (_IsDragged || _IsHovered) && ZoomConfig.ZoomOnHover ? ZoomConfig.Multiplier : 1;
        var delta      = Mathf.Abs(_RectTransform.localScale.x - targetZoom);
        var newZoom    = Mathf.Lerp(_RectTransform.localScale.x, targetZoom, AnimationSpeedConfig.Zoom / delta * Time.deltaTime);

        _RectTransform.localScale = new Vector3(newZoom, newZoom, 1);
    }

    private void UpdateRotation()
    {
        var crtAngle = _RectTransform.rotation.eulerAngles.z;

        // If the angle is negative, add 360 to it to get the positive equivalent
        crtAngle = crtAngle < 0 ? crtAngle + 360 : crtAngle;

        // If the card is hovered and the rotation should be reset, set the target rotation to 0
        var tempTargetRotation = (_IsHovered || _IsDragged) && ZoomConfig.ResetRotationOnZoom ? 0 : TargetRotation;

        tempTargetRotation = tempTargetRotation < 0 ? tempTargetRotation + 360 : tempTargetRotation;

        var deltaAngle = Mathf.Abs(crtAngle - tempTargetRotation);

        if (!(deltaAngle > EPS))
            return;
        // Adjust the current angle and target angle so that the rotation is done in the shortest direction
        var adjustedCurrent = deltaAngle > 180 && crtAngle < tempTargetRotation ? crtAngle + 360 : crtAngle;
        var adjustedTarget  = deltaAngle > 180 && crtAngle > tempTargetRotation ? tempTargetRotation + 360 : tempTargetRotation;
        var newDelta        = Mathf.Abs(adjustedCurrent - adjustedTarget);
        var nextRotation    = Mathf.Lerp(adjustedCurrent, adjustedTarget, AnimationSpeedConfig.Rotation / newDelta * Time.deltaTime);

        _RectTransform.rotation = Quaternion.Euler(0, 0, nextRotation);
    }

    public void SetAnchor(Vector2 min, Vector2 max)
    {
        _RectTransform.anchorMin = min;
        _RectTransform.anchorMax = max;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_IsDragged || !IsMyContainer) // Avoid hover events while dragging
            return;
        if (ZoomConfig.BringToFrontOnHover)
            _Canvas.sortingOrder = ZoomConfig.ZoomedSortOrder;
        EventsConfig?.OnCardHover?.Invoke(new Events.CardHover(this));

        _IsHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_IsDragged || !IsMyContainer) // Avoid hover events while dragging
            return;
        _Canvas.sortingOrder = UILayer;
        _IsHovered           = false;

        EventsConfig?.OnCardUnhover?.Invoke(new Events.CardUnhover(this));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (PreventCardInteraction || !IsMyContainer)
            return;
        _IsDragged    = true;
        _DragStartPos = new Vector2(transform.position.x - eventData.position.x, transform.position.y - eventData.position.y);

        Container.OnCardDragStart(this);
        EventsConfig?.OnCardUnhover?.Invoke(new Events.CardUnhover(this));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _IsDragged = false;
        Container.OnCardDragEnd();
    }
}
