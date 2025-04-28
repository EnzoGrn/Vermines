using UnityEditor;
using UnityEngine;

namespace Vermines.Gameplay.Cards.Effect {

    using Vermines.Editor.Gameplay.Cards.Effect;
    using Vermines.ShopSystem.Enumerations;

    [CustomEditor(typeof(FreeCardEffect))]
    public class FreeCardEffectEditor : AEffectEditor {

        protected override void DrawCustomProperties()
        {
            if (target == null || target is not FreeCardEffect)
                return;
            FreeCardEffect effect = (FreeCardEffect)target;

            // -- [Header("Card Properties")]
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Effect Properties", EditorStyles.boldLabel);

            effect.Amount = EditorGUILayout.IntField(new GUIContent("Amount", "The amount of card free per turn."), effect.Amount);
            effect.ShopTarget = (ShopType)EditorGUILayout.EnumPopup(new GUIContent("Shop Target", "The shop where the reduction will be applied."), effect.ShopTarget);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            // -- EOF --
        }
    }
}
