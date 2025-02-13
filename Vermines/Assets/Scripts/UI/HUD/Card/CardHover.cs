using UnityEngine;
using DG.Tweening;

public class CardHover : MonoBehaviour
{
    private Vector3 initialLocalPosition;
    private Vector3 initialScale;

    private void Start()
    {
        initialScale = transform.localScale;
    }

    public void SetInitialPosition()
    {
        initialLocalPosition = transform.localPosition;
    }

    public void OnMouseEnter()
    {
        transform.DOKill();
        transform.DOLocalMove(initialLocalPosition + new Vector3(0, 0.5f, 0), 0.2f);
        transform.DOScale(initialScale * 1.1f, 0.2f);
    }

    public void OnMouseExit()
    {
        transform.DOKill();
        transform.DOLocalMove(initialLocalPosition, 0.2f);
        transform.DOScale(initialScale, 0.2f);
    }
}
