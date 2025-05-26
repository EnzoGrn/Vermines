using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(RadialLayout))]
public class RadialLayoutEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RadialLayout layout = (RadialLayout)target;

        EditorGUILayout.Space();
        GUI.backgroundColor = new Color(0.8f, 1f, 0.8f);
        if (GUILayout.Button("Reset Distance"))
        {
            Undo.RecordObject(layout, "Reset Radial Distance");
            layout.ResetDistance();
            layout.CalculateLayoutInputHorizontal();
            EditorUtility.SetDirty(layout);
        }
        GUI.backgroundColor = Color.white;
    }
}
