using UnityEditor;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect
{

    using Vermines.Editor.Gameplay.Cards.Effect;
    using Vermines.CardSystem.Enumerations;

    [CustomEditor(typeof(SpendEffect))]
    public class SpendEffectEditor : AEffectEditor {

        protected override void DrawCustomProperties()
        {
            if (target == null || target is not SpendEffect)
                return;
            SpendEffect effect = (SpendEffect)target;

            // -- [Header("Card Properties")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);

            effect.Amount = EditorGUILayout.IntField(new GUIContent("Amount", "The amount of eloquence to spend."), effect.Amount);
            effect.DataToSpend = (DataType)EditorGUILayout.EnumPopup(new GUIContent("Data type", "The type of the data you want to spend."), effect.DataToSpend);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --
        }
    }
}
