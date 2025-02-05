using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FastTextureRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    private RectTransform rectTransform;
    private Image image;
    private Texture2D texture;
    private Color32[] pixels;
    private int textureWidth, textureHeight;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (image != null && image.sprite != null && image.sprite.texture != null)
        {
            texture = image.sprite.texture;
            textureWidth = texture.width;
            textureHeight = texture.height;
            pixels = texture.GetPixels32(); // Récupérer tous les pixels en une seule fois
        }
        else
        {
            Debug.LogError("FastTextureRaycastFilter: Aucun Sprite valide trouvé sur " + gameObject.name);
        }
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (texture == null || image == null || pixels == null) return false;

        // Convertir la position écran en coordonnées locales du RectTransform
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out Vector2 localPoint))
            return false;

        // Convertir en coordonnées UV (0-1) puis en pixels
        Rect rect = image.sprite.rect;
        Vector2 pivot = image.rectTransform.pivot;
        float x = (localPoint.x + rect.width * pivot.x) / rect.width;
        float y = (localPoint.y + rect.height * pivot.y) / rect.height;

        int pixelX = Mathf.RoundToInt(x * textureWidth);
        int pixelY = Mathf.RoundToInt(y * textureHeight);

        // Vérifier si on est hors des limites de la texture
        if (pixelX < 0 || pixelX >= textureWidth || pixelY < 0 || pixelY >= textureHeight)
            return false;

        // Lire la transparence directement dans le tableau
        int index = pixelY * textureWidth + pixelX;
        return pixels[index].a > 10; // Seuil de transparence (0-255)
    }
}
