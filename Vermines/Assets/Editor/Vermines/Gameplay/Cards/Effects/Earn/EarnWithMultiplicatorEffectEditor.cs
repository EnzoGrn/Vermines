using UnityEditor;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.Editor.Gameplay.Cards.Effect;
    using Vermines.CardSystem.Enumerations;

    [CustomEditor(typeof(EarnWithMultiplicatorEffect))]
    public class EarnWithMultiplicatorEffectEditor : AEffectEditor {

        protected override void DrawCustomProperties()
        {
            if (target == null || target is not EarnWithMultiplicatorEffect)
                return;
            EarnWithMultiplicatorEffect effect = (EarnWithMultiplicatorEffect)target;

            // -- [Header("Card Properties")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);

            effect.Amount     = EditorGUILayout.IntField(new GUIContent("Amount", "The amount of data to earn."), effect.Amount);
            effect.DataToEarn = (DataType)EditorGUILayout.EnumPopup(new GUIContent("Data type", "The type of the data you want to earn."), effect.DataToEarn);

            effect.ConditionChoice = (EarnWithMultiplicatorConditionType)EditorGUILayout.EnumPopup(new GUIContent("Condition", "The condition to multiply the data that will be earned."), effect.ConditionChoice);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --
        }
    }
}
