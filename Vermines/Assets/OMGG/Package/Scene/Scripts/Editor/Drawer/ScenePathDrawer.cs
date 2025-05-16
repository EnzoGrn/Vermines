#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace OMGG.Scene.Editor.Drawer {

    using OMGG.Scene.Attribute;

    [CustomPropertyDrawer(typeof(ScenePathAttribute))]
    public class ScenePathDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SceneAsset sceneAsset = null;

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            if (!string.IsNullOrEmpty(property.stringValue))
                sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);
            SceneAsset newScene = EditorGUI.ObjectField(position, sceneAsset, typeof(SceneAsset), false) as SceneAsset;

            if (newScene != sceneAsset) {
                if (newScene != null) {
                    string path = AssetDatabase.GetAssetPath(newScene);

                    if (path.EndsWith(".unity"))
                        property.stringValue = path;
                    else
                        Debug.LogError("The selected asset is not a Unity scene.");
                } else
                    property.stringValue = string.Empty;
            }

            EditorGUI.EndProperty();
        }
    }
};

#endif
