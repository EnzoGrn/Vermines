#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Vermines.UI.Card;
using Vermines.CardSystem.Elements;
using UnityEngine.UI;

public class DraggableResizableCard : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    private Vector2 offset;
    private bool resizing = false;

    [SerializeField] private float resizeBorderThickness = 15f;
    [SerializeField] private Vector2 minSize = new Vector2(100, 100);
    [SerializeField] private Vector2 maxSize = new Vector2(500, 500);

    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    [SerializeField] private CardDisplay _cardDisplay;

    void Awake()
    {
        var layoutElement = gameObject.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        GameEvents.OnCardClicked.AddListener(Show);
        gameObject.SetActive(false);
        gameObject.GetComponentInChildren<CardDisplay>().gameObject.SetActive(true);
    }

    public void Show(ICard card, int i)
    {
        Debug.Log($"[DraggableResizableCard] Show card: {card?.Data.Name}, index: {i}");
        if (card == null || i > 0) return;

        if (_cardDisplay != null)
        {
            _cardDisplay.Display(card);
            gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIElement(gameObject))
            {
                CloseCard();
            }
        }
        if (Input.GetKeyDown(closeKey))
        {
            CloseCard();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsOnResizeBorder(eventData))
        {
            resizing = true;
        }
        else
        {
            resizing = false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, eventData.position, eventData.pressEventCamera, out offset);
            offset = rectTransform.anchoredPosition - offset;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (resizing)
        {
            Resize(eventData);
        }
        else
        {
            Drag(eventData);
        }
    }

    private void Drag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            rectTransform.anchoredPosition = localPoint + offset;
        }
    }

    private void Resize(PointerEventData eventData)
    {
        float delta = eventData.delta.x;

        Vector2 sizeDelta = rectTransform.sizeDelta;
        sizeDelta += new Vector2(delta, delta);

        sizeDelta.x = Mathf.Clamp(sizeDelta.x, minSize.x, maxSize.x);
        sizeDelta.y = Mathf.Clamp(sizeDelta.y, minSize.y, maxSize.y);

        float finalSize = Mathf.Min(sizeDelta.x, sizeDelta.y);
        rectTransform.sizeDelta = new Vector2(finalSize, finalSize);
    }

    private bool IsOnResizeBorder(PointerEventData eventData)
    {
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out localMousePos);

        Rect rect = rectTransform.rect;

        return (localMousePos.x > rect.width / 2 - resizeBorderThickness &&
                localMousePos.y < -rect.height / 2 + resizeBorderThickness);
    }

    private bool IsPointerOverUIElement(GameObject target)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (var res in results)
        {
            if (res.gameObject == target || res.gameObject.transform.IsChildOf(target.transform))
                return true;
        }
        return false;
    }

    private void CloseCard()
    {
        gameObject.SetActive(false); // ou Destroy(gameObject); si tu veux vraiment la supprimer
    }

    // --- DEBUG ZONE DE RESIZE ---
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        Rect rect = rectTransform.rect;
        Vector3 topLeft = rectTransform.TransformPoint(new Vector3(rect.width / 2 - resizeBorderThickness, -rect.height / 2 + resizeBorderThickness));
        Vector3 topRight = rectTransform.TransformPoint(new Vector3(rect.width / 2, -rect.height / 2 + resizeBorderThickness));
        Vector3 bottomRight = rectTransform.TransformPoint(new Vector3(rect.width / 2, -rect.height / 2));
        Vector3 bottomLeft = rectTransform.TransformPoint(new Vector3(rect.width / 2 - resizeBorderThickness, -rect.height / 2));

        Handles.color = Color.green;
        Handles.DrawLine(topLeft, topRight);
        Handles.DrawLine(topRight, bottomRight);
        Handles.DrawLine(bottomRight, bottomLeft);
        Handles.DrawLine(bottomLeft, topLeft);
    }
#endif
}
