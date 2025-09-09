using UnityEngine;
using UnityEngine.EventSystems;

public class ResizeHandle : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform targetRect; // UI element to resize
    private Vector2 originalMousePos;
    private Vector3 originalScale;
    private float aspectRatio;
    private float originalWidth;

    private void Awake()
    {
        if (targetRect == null)
            targetRect = transform.parent.GetComponent<RectTransform>(); // assume parent is target

        aspectRatio = targetRect.rect.width / targetRect.rect.height;
        originalWidth = targetRect.rect.width;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetRect, eventData.position, eventData.pressEventCamera, out originalMousePos);

        originalScale = targetRect.localScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetRect, eventData.position, eventData.pressEventCamera, out localMousePos);

        Vector2 delta = localMousePos - originalMousePos;

        // Scale factor based on horizontal drag
        float scaleDelta = 1f + (delta.x / originalWidth);

        // Apply uniform scale (preserve aspect ratio)
        Vector3 newScale = originalScale * scaleDelta;

        // Clamp so it doesn’t get too small
        newScale = new Vector3(
            Mathf.Max(0.2f, newScale.x),
            Mathf.Max(0.2f, newScale.y),
            1f
        );

        targetRect.localScale = newScale;
    }
}
