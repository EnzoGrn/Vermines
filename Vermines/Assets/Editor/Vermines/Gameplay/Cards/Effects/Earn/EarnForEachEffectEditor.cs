using UnityEditor;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.Editor.Gameplay.Cards.Effect;
    using Vermines.CardSystem.Enumerations;

    [CustomEditor(typeof(EarnForEachEffect))]
    public class EarnForEachEffectEditor : AEffectEditor {

        protected override void DrawCustomProperties()
        {
            if (target == null || target is not EarnForEachEffect)
                return;
            EarnForEachEffect effect = (EarnForEachEffect)target;

            // -- [Header("Card Properties")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);

            effect.Amount = EditorGUILayout.IntField(new GUIContent("Amount", "The amount of data to earn."), effect.Amount);
            effect.DataToEarn = (DataType)EditorGUILayout.EnumPopup(new GUIContent("Data type", "The type of the data you want to earn."), effect.DataToEarn);

            EditorGUILayout.EndVertical();

            EditorGUILayout.HelpBox("Only Partisan and Equipment card types are supported.", MessageType.Warning);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            effect.CardType = (CardType)EditorGUILayout.EnumPopup(new GUIContent("Card type", "The type of card to count."), effect.CardType);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --
        }
    }
}
