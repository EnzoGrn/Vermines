using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class UISelectionOutlineDrawer
{
    static UISelectionOutlineDrawer()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += OnSelectionChanged;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (Selection.activeGameObject == null)
            return;

        GameObject selected = Selection.activeGameObject;

        if (selected.TryGetComponent<RectTransform>(out RectTransform rectTransform))
        {
            DrawUIOutline(rectTransform);
        }
    }

    private static void DrawUIOutline(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Handles.color = Color.yellow;
        Handles.DrawLine(corners[0], corners[1]); // Bas gauche → Bas droit
        Handles.DrawLine(corners[1], corners[2]); // Bas droit → Haut droit
        Handles.DrawLine(corners[2], corners[3]); // Haut droit → Haut gauche
        Handles.DrawLine(corners[3], corners[0]); // Haut gauche → Bas gauche
    }

    private static void OnSelectionChanged()
    {
        SceneView.RepaintAll(); // Force la mise à jour de la scène
    }
}
