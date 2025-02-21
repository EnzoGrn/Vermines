using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HandHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Transform handContainer;
    [SerializeField] private float hiddenY = -2f; // Position hors écran
    [SerializeField] private float visibleY = 0f; // Position visible
    [SerializeField] private float duration = 0.3f;

    private void Start()
    {
        // Cache la main au début
        handContainer.localPosition = new Vector3(handContainer.localPosition.x, hiddenY, handContainer.localPosition.z);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        handContainer.DOLocalMoveY(visibleY, duration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        handContainer.DOLocalMoveY(hiddenY, duration);
    }
}
