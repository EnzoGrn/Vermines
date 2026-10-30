using UnityEditor;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.Editor.Gameplay.Cards.Effect;

    [CustomEditor(typeof(SlotOnTableEffect))]
    public class SlotOnTableEffectEditor : AEffectEditor {

        protected override void DrawCustomProperties()
        {
            if (target == null || target is not SlotOnTableEffect effect)
                return;
            // -- [Header("Card Properties")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);

            effect.Amount = EditorGUILayout.IntField(new GUIContent("Amount", "The amount of slot we want."), effect.Amount);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --
        }
    }
}
