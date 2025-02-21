using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 initialLocalPosition;
    private Vector3 initialScale;
    private bool isLocked = false;

    private void Start()
    {
        initialScale = transform.localScale;
    }

    public void SetInitialPosition()
    {
        initialLocalPosition = transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked) return;

        transform.DOKill();
        transform.DOLocalMove(initialLocalPosition + new Vector3(0, 0.5f, 0), 0.2f);
        transform.DOScale(initialScale * 1.1f, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked) return;

        transform.DOKill();
        transform.DOLocalMove(initialLocalPosition, 0.2f);
        transform.DOScale(initialScale, 0.2f);
    }

    public void SetLocked(bool value)
    {
        isLocked = value;
    }
}
