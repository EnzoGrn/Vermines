using UnityEditor;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.Editor.Gameplay.Cards.Effect;

    [CustomEditor(typeof(OtherDiscardEffect))]
    public class OtherDiscardEffectEditor : AEffectEditor {

        protected override void DrawCustomProperties()
        {
            if (target == null || target is not OtherDiscardEffect effect)
                return;
            // -- [Header("Card Properties")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);

            SerializedProperty cardTarget = serializedObject.FindProperty("_TargetType");

            EditorGUILayout.PropertyField(cardTarget, new GUIContent("Card Target", "The type of card that will trigger the effect when discarded."));

            serializedObject.ApplyModifiedProperties();

            effect.IsGodEffect = EditorGUILayout.Toggle(new GUIContent("Is God Effect", "If true, the effect will be considered as a god effect and will trigger the god effect of the card."), effect.IsGodEffect);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --
        }
    }
}
