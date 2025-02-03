using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]  // Permet d'afficher les gizmos même en mode édition
public class UIButtonRaycastDebugger : MonoBehaviour
{
    public bool showAsCircle = false;  // Active l'affichage en mode cercle
    private RectTransform rectTransform;
    private Image image;

    private void OnDrawGizmos()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (image == null) image = GetComponent<Image>();

        if (rectTransform == null || image == null || !image.raycastTarget)
            return;

        // Définir la couleur des gizmos
        Gizmos.color = Color.green;

        if (showAsCircle)
        {
            DrawCircle();
        }
        else
        {
            DrawRectangle();
        }
    }

    private void DrawRectangle()
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // Dessine un cadre autour du bouton
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);
    }

    private void DrawCircle()
    {
        Vector3 center = rectTransform.position;
        float radius = rectTransform.rect.width * 0.5f * rectTransform.lossyScale.x;

        const int segments = 32;
        Vector3 previousPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * (2 * Mathf.PI / segments);
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }
}
