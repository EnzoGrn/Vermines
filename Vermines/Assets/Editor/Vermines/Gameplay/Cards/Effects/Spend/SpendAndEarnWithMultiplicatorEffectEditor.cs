using UnityEditor;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.Editor.Gameplay.Cards.Effect;
    using Vermines.CardSystem.Enumerations;

    [CustomEditor(typeof(SpendAndEarnMultiplicatorEffect))]
    public class SpendAndEarnMultiplicatorEffectEditor : AEffectEditor {

        protected override void DrawCustomProperties()
        {
            if (target == null || target is not SpendAndEarnMultiplicatorEffect)
                return;
            SpendAndEarnMultiplicatorEffect effect = (SpendAndEarnMultiplicatorEffect)target;

            // -- [Header("Card Properties")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);

            effect.DataToSpend   = (DataType)EditorGUILayout.EnumPopup(new GUIContent("Data type", "The type of the data you want to spend."), effect.DataToSpend);
            effect.DataToEarn    = (DataType)EditorGUILayout.EnumPopup(new GUIContent("Data type", "The type of the data you want to earn."), effect.DataToEarn);
            effect.Multiplicator = EditorGUILayout.IntField(new GUIContent("Multiplicator", "The multiplicator to apply on the earn data."), effect.Multiplicator);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --
        }
    }
}
