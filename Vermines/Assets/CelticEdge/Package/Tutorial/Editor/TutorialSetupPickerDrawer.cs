using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

namespace CelticEdge.Tutorial.EditorTools {

    /// <summary>
    /// Picks the setup class of a lesson (TutorialLessonData.Setup field),
    /// then edits its fields.
    /// </summary>
    [CustomPropertyDrawer(typeof(TutorialSetupPickerAttribute))]
    public class TutorialSetupPickerDrawer : PropertyDrawer {

        private static List<Type> _Types;

        private static List<Type> Types => _Types ??= TypeCache.GetTypesDerivedFrom<ITutorialLessonSetup>()
            .Where(t => !t.IsAbstract && !t.IsGenericType && !typeof(UnityEngine.Object).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.Name)
            .ToList();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return height;
            foreach (var child in Children(property))
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(child, true);
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            if (property.propertyType != SerializedPropertyType.ManagedReference) {
                EditorGUI.LabelField(line, label.text, "[TutorialSetupPicker] requires a [SerializeReference] field.");

                return;
            }

            var options = new List<string> { "(none)" };

            options.AddRange(Types.Select(t => t.Name));

            string current = property.managedReferenceFullTypename;
            int    index   = 0;

            for (int i = 0; i < Types.Count; i++) {
                if (current == $"{Types[i].Assembly.GetName().Name} {Types[i].FullName}")
                    index = i + 1;
            }

            EditorGUI.BeginProperty(position, label, property);

            int chosen = EditorGUI.Popup(line, label.text, index, options.ToArray());

            if (chosen != index) {
                property.managedReferenceValue = chosen == 0 ? null : Activator.CreateInstance(Types[chosen - 1]);
                property.serializedObject.ApplyModifiedProperties();
                EditorGUI.EndProperty();

                return;
            }

            EditorGUI.indentLevel++;

            float y = line.yMax;

            foreach (var child in Children(property)) {
                float h = EditorGUI.GetPropertyHeight(child, true);

                y += EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), child, true);

                y += h;
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        private static IEnumerable<SerializedProperty> Children(SerializedProperty property)
        {
            if (string.IsNullOrEmpty(property.managedReferenceFullTypename))
                yield break;
            var  it    = property.Copy();
            var  end   = property.GetEndProperty();
            bool enter = true;

            while (it.NextVisible(enter) && !SerializedProperty.EqualContents(it, end)) {
                enter = false;

                yield return it.Copy();
            }
        }
    }
}
