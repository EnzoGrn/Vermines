using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Raycast filter that ignores fully or mostly transparent pixels in a UI Image.
/// Useful for precise hit detection on non-rectangular sprites.
/// </summary>
[RequireComponent(typeof(Image))]
public class FastTextureRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    private RectTransform rectTransform;
    private Image image;
    private Texture2D texture;
    private Color32[] pixels;
    private int textureWidth, textureHeight;

    /// <summary>
    /// Initializes the required components and caches pixel data from the image texture.
    /// </summary>
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (image != null && image.sprite != null && image.sprite.texture != null)
        {
            texture = image.sprite.texture;
            textureWidth = texture.width;
            textureHeight = texture.height;
            pixels = texture.GetPixels32();
        }
        else
        {
            Debug.LogWarning("FastTextureRaycastFilter: No valid Sprite found on " + gameObject.name);
        }
    }

    /// <summary>
    /// Determines whether the given screen point is valid for raycasting.
    /// Transparent pixels are ignored.
    /// </summary>
    /// <param name="sp">Screen position of the pointer.</param>
    /// <param name="eventCamera">Camera used to convert screen point to world space.</param>
    /// <returns>True if the raycast hit a non-transparent pixel; otherwise, false.</returns>
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (texture == null || image == null || pixels == null) return false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out Vector2 localPoint))
            return false;

        Rect rect = image.sprite.rect;
        Vector2 pivot = image.rectTransform.pivot;
        float x = (localPoint.x + rect.width * pivot.x) / rect.width;
        float y = (localPoint.y + rect.height * pivot.y) / rect.height;

        int pixelX = Mathf.RoundToInt(x * textureWidth);
        int pixelY = Mathf.RoundToInt(y * textureHeight);

        if (pixelX < 0 || pixelX >= textureWidth || pixelY < 0 || pixelY >= textureHeight)
            return false;

        int index = pixelY * textureWidth + pixelX;
        return pixels[index].a > 10;
    }
}
