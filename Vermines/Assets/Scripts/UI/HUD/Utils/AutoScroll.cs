using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class AutoScrollPage : MonoBehaviour
{
    public ScrollRect scrollRect;
    public TextMeshProUGUI contentText;

    void Update()
    {
        UpdateScroll();
    }

    private void UpdateScroll()
    {
        var contentHeight = contentText.preferredHeight;
        var viewportHeight = scrollRect.viewport.rect.height;

        scrollRect.vertical = contentHeight > viewportHeight;
    }
}
