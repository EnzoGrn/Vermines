using UnityEditor;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.Editor.Gameplay.Cards.Effect;

    [CustomEditor(typeof(OtherSacrificeEffect))]
    public class OtherSacrificeEffectEditor : AEffectEditor {

        protected override void DrawCustomProperties()
        {
            if (target == null || target is not OtherSacrificeEffect effect)
                return;
            // -- [Header("Card Properties")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);

            effect.IsGodEffect = EditorGUILayout.Toggle(new GUIContent("Is God Effect", "If true, the effect will be considered as a god effect and will trigger the god effect of the card."), effect.IsGodEffect);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --
        }
    }
}
