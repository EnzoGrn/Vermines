using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(LayoutPaddingVisualizer))]
public class LayoutPaddingVisualizerEditor : Editor
{
    private void OnSceneGUI()
    {
        LayoutPaddingVisualizer visualizer = (LayoutPaddingVisualizer)target;

        if (!visualizer.showInPlayMode && Application.isPlaying)
            return;

        LayoutGroup layoutGroup = visualizer.GetLayoutGroup();
        if (layoutGroup == null)
            return;

        RectTransform rectTransform = visualizer.GetRectTransform();
        if (rectTransform == null)
            return;

        DrawPadding(rectTransform, layoutGroup, visualizer.paddingColor);
    }

    private void DrawPadding(RectTransform rectTransform, LayoutGroup layoutGroup, Color paddingColor)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        RectOffset padding = layoutGroup.padding;

        // Convert padding to world coordinates
        float width = Vector3.Distance(corners[0], corners[3]);
        float height = Vector3.Distance(corners[0], corners[1]);

        Vector3 right = (corners[3] - corners[0]).normalized;
        Vector3 up = (corners[1] - corners[0]).normalized;

        // Draw padding rectangles
        Handles.color = paddingColor;

        // Left padding
        if (padding.left > 0)
        {
            Vector3[] leftPadding = new Vector3[4];
            leftPadding[0] = corners[0];
            leftPadding[1] = corners[1];
            leftPadding[2] = corners[1] + right * padding.left;
            leftPadding[3] = corners[0] + right * padding.left;
            Handles.DrawSolidRectangleWithOutline(leftPadding, paddingColor, Color.green);
        }

        // Right padding
        if (padding.right > 0)
        {
            Vector3[] rightPadding = new Vector3[4];
            rightPadding[0] = corners[3] - right * padding.right;
            rightPadding[1] = corners[2] - right * padding.right;
            rightPadding[2] = corners[2];
            rightPadding[3] = corners[3];
            Handles.DrawSolidRectangleWithOutline(rightPadding, paddingColor, Color.green);
        }

        // Top padding
        if (padding.top > 0)
        {
            Vector3[] topPadding = new Vector3[4];
            topPadding[0] = corners[1] - up * padding.top;
            topPadding[1] = corners[1];
            topPadding[2] = corners[2];
            topPadding[3] = corners[2] - up * padding.top;
            Handles.DrawSolidRectangleWithOutline(topPadding, paddingColor, Color.green);
        }

        // Bottom padding
        if (padding.bottom > 0)
        {
            Vector3[] bottomPadding = new Vector3[4];
            bottomPadding[0] = corners[0];
            bottomPadding[1] = corners[0] + up * padding.bottom;
            bottomPadding[2] = corners[3] + up * padding.bottom;
            bottomPadding[3] = corners[3];
            Handles.DrawSolidRectangleWithOutline(bottomPadding, paddingColor, Color.green);
        }

        // Display padding values
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;

        Vector3 center = (corners[0] + corners[2]) * 0.5f;

        if (padding.left > 0)
            Handles.Label(corners[0] + right * (padding.left * 0.5f) + up * (height * 0.5f), $"L: {padding.left}", style);

        if (padding.right > 0)
            Handles.Label(corners[3] - right * (padding.right * 0.5f) + up * (height * 0.5f), $"R: {padding.right}", style);

        if (padding.top > 0)
            Handles.Label(corners[1] - up * (padding.top * 0.5f) + right * (width * 0.5f), $"T: {padding.top}", style);

        if (padding.bottom > 0)
            Handles.Label(corners[0] + up * (padding.bottom * 0.5f) + right * (width * 0.5f), $"B: {padding.bottom}", style);
    }
}