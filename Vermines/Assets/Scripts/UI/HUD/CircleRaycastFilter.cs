using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CircleRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        // Convertir la position du curseur en coordonnées locales du RectTransform
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out localPoint);

        // Rayon du cercle basé sur la taille de l'image
        float radius = rectTransform.rect.width * 0.5f;

        // Vérifier si le point est dans le cercle
        return localPoint.magnitude <= radius;
    }
}
