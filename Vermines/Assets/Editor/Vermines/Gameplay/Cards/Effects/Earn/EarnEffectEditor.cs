using UnityEditor;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.Editor.Gameplay.Cards.Effect;
    using Vermines.CardSystem.Enumerations;

    [CustomEditor(typeof(EarnEffect))]
    public class EarnEffectEditor : AEffectEditor {

        protected override void DrawCustomProperties()
        {
            if (target == null || target is not EarnEffect)
                return;
            EarnEffect effect = (EarnEffect)target;

            // -- [Header("Card Properties")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);

            effect.Amount     = EditorGUILayout.IntField(new GUIContent("Amount", "The amount of data to earn."), effect.Amount);
            effect.DataToEarn = (DataType)EditorGUILayout.EnumPopup(new GUIContent("Data type", "The type of the data you want to earn."), effect.DataToEarn);
            effect.Everyone   = EditorGUILayout.Toggle(new GUIContent("Everyone", "If true, everyone will earn the data."), effect.Everyone);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --
        }
    }
}
