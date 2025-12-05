using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LayoutPaddingVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    public Color paddingColor = new Color(0f, 1f, 0f, 0.3f);
    public bool showInPlayMode = false;

    public LayoutGroup GetLayoutGroup()
    {
        return GetComponent<LayoutGroup>();
    }

    public RectTransform GetRectTransform()
    {
        return GetComponent<RectTransform>();
    }
}