using UnityEditor;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.Editor.Gameplay.Cards.Effect;
    using Vermines.CardSystem.Enumerations;

    [CustomEditor(typeof(RemoveToEarn))]
    public class RemoveToEarnEffectEditor : AEffectEditor {

        protected override void DrawCustomProperties()
        {
            if (target == null || target is not RemoveToEarn)
                return;
            RemoveToEarn effect = (RemoveToEarn)target;

            // -- [Header("Card Properties")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);

            effect.CardType = (CardType)EditorGUILayout.EnumPopup(new GUIContent("Card type", "The type of the card you want to remove."), effect.CardType);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --
        }
    }
}
