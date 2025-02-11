using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TextureRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    private RectTransform rectTransform;
    private Image image;
    private Texture2D texture;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (image != null && image.sprite != null && image.sprite.texture != null)
        {
            texture = image.sprite.texture;
        }
        else
        {
            Debug.LogError("TextureRaycastFilter: Aucun Sprite valide trouvé sur " + gameObject.name);
        }
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (texture == null || image == null) return false;

        // Convertir la position écran en coordonnées locales du RectTransform
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out Vector2 localPoint))
            return false;

        // Convertir en coordonnées UV (0-1) puis en pixels
        Rect rect = image.sprite.rect;
        Vector2 pivot = image.rectTransform.pivot;
        float x = (localPoint.x + rect.width * pivot.x) / rect.width;
        float y = (localPoint.y + rect.height * pivot.y) / rect.height;

        int pixelX = Mathf.RoundToInt(x * texture.width);
        int pixelY = Mathf.RoundToInt(y * texture.height);

        // Vérifier si le pixel est transparent
        if (pixelX < 0 || pixelX >= texture.width || pixelY < 0 || pixelY >= texture.height)
            return false; // Hors de l'image

        Color pixelColor = texture.GetPixel(pixelX, pixelY);
        return pixelColor.a > 0.1f; // Seulement si le pixel n'est pas transparent
    }
}
