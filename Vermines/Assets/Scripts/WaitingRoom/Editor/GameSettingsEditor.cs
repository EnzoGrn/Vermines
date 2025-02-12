using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Vermines {

    using Vermines.Config;
    using Vermines.Config.Utils;

    [CustomEditor(typeof(GameConfiguration))]
    public class GameSettingsEditor : Editor {

        private GUIStyle warningStyle;

        private void InitializeStyles()
        {
            if (warningStyle == null) {
                warningStyle                  = new GUIStyle(EditorStyles.label);
                warningStyle.normal.textColor = Color.yellow; // Warning text color
                warningStyle.fontStyle        = FontStyle.Bold; // Make it bold for emphasis
            }
        }

        private void SetUpIntSetting(ASettingBase setting)
        {
            IntSetting intSetting = (IntSetting)setting;

            try {
                intSetting.RestrictionCheck(intSetting.Value);

                intSetting.Value = EditorGUILayout.IntField(intSetting.Name, (int)intSetting.Value);
            } catch (System.Exception e) {
                intSetting.Value = EditorGUILayout.IntField(intSetting.Name, (int)intSetting.Value);

                EditorGUILayout.HelpBox(e.Message, MessageType.Error);
            }
        }

        private void SetUpBoolSetting(ASettingBase setting)
        {
            BoolSetting boolSetting = (BoolSetting)setting;

            boolSetting.Value = EditorGUILayout.Toggle(boolSetting.Name, (bool)boolSetting.Value);
        }

        public override void OnInspectorGUI()
        {
            // Get reference to the target object
            GameConfiguration gameSettings = (GameConfiguration)target;

            Dictionary<string, List<ASettingBase>> settingsByCategory = SettingsUtils.GetSettingsByCategory(gameSettings);

            // Loop over all properties in the GameSettings object
            foreach (List<ASettingBase> settingList in settingsByCategory.Values) {
                if (settingList == null)
                    continue;
                // Label name of the category
                string category = (settingList.Count > 0 && settingList[0] != null) ? settingList[0].Category : "Other";

                // Set a category label
                GUILayout.Label(category, EditorStyles.boldLabel);

                foreach (ASettingBase setting in settingList) {
                    if (setting == null)
                        continue;
                    switch (setting.Type) {
                        case ASettingBase.SettingType.Int:
                            SetUpIntSetting(setting);
                            break;
                        case ASettingBase.SettingType.Bool:
                            SetUpBoolSetting(setting);
                            break;
                        default:
                            break;
                    }
                }

                // Save changes made in the inspector (usefull for scriptable objects)
                if (GUI.changed) {
                    EditorUtility.SetDirty(gameSettings);
                    Repaint();
                }

                // Add Space after the category
                EditorGUILayout.Space(10);
            }
        }
    }
}
