using UnityEditor;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.Editor.Gameplay.Cards.Effect;

    [CustomEditor(typeof(ShopReductionEffect))]
    public class ShopReductionEffectEditor : AEffectEditor {

        protected override void DrawCustomProperties()
        {
            if (target == null || target is not ShopReductionEffect)
                return;
            ShopReductionEffect effect = (ShopReductionEffect)target;

            // -- [Header("Card Properties")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);

            effect.Amount = EditorGUILayout.IntField(new GUIContent("Amount", "The amount of reduction wanted."), effect.Amount);

            SerializedProperty shopTarget = serializedObject.FindProperty("_ShopTarget");

            EditorGUILayout.PropertyField(shopTarget, new GUIContent("Shop Target", "The shop where the reduction will be applied."));

            serializedObject.ApplyModifiedProperties();

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --
        }
    }
}
