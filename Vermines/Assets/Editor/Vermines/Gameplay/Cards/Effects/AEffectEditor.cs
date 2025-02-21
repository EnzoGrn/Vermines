using UnityEditor;
using UnityEngine;

namespace Vermines.Editor.Gameplay.Cards.Effect {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Data;

    public abstract class AEffectEditor : UnityEditor.Editor {

        public override void OnInspectorGUI()
        {
            if (target == null || target is not AEffect)
                return;
            AEffect effect = (AEffect)target;

            if (effect == null)
                return;

            // -- Title
            GUILayout.Label("Effect Configuration", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // -- [Header("Effect Information")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Information (Can't have to be edited)", EditorStyles.boldLabel);

            // [Tooltip("The description of the effect.")]
            effect.Description = EditorGUILayout.TextField(new GUIContent("Effect Description", "The description of the effect."), effect.Description);

            GUILayout.Space(5);
            Vermines.Editor.Utils.DescriptionUtils.DrawDescriptionPreview(effect.Description);
            GUILayout.Space(5);

            effect.Type      = (EffectType)EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of the card."), effect.Type);
            effect.SubEffect = (AEffect)EditorGUILayout.ObjectField(new GUIContent("Sub Effect", "The sub effect of the card."), effect.SubEffect, typeof(AEffect), false);

            GUILayout.EndVertical();
            GUILayout.Space(10);

            // -- [Header("Effect Properties")]
            DrawCustomProperties();

            // -- [Header("Effect Preview")]
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Effect Preview :", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            GUIStyle textStyle = new(EditorStyles.boldLabel) {
                fontSize = 24
            };

            foreach (var element in effect.Draw()) {
                if (element.Item1 != null) { // -- Text
                    GUILayout.Label(element.Item1, textStyle, GUILayout.ExpandWidth(false));
                } else { // -- Icon
                    Texture2D iconTexture = AssetPreview.GetAssetPreview(element.Item2);
                    Rect      iconRect    = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32), GUILayout.Height(32));

                    if (iconTexture != null)
                        GUI.DrawTexture(iconRect, iconTexture, ScaleMode.ScaleToFit);
                }
            }
            EditorGUILayout.EndHorizontal();
            // -- EOF --

            // -- Refresh the UI, if it has changed
            if (GUI.changed)
                EditorUtility.SetDirty(effect);
        }

        protected abstract void DrawCustomProperties();
    }
}
