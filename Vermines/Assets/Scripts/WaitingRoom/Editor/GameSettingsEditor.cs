using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Vermines
{
    [CustomEditor(typeof(GameSettings))]
    public class GameSettingsEditor : Editor
    {
        private GUIStyle warningStyle;
        private void InitializeStyles()
        {
            if (warningStyle == null)
            {
                warningStyle = new GUIStyle(EditorStyles.label);
                warningStyle.normal.textColor = Color.yellow; // Warning text color
                warningStyle.fontStyle = FontStyle.Bold; // Make it bold for emphasis
            }
        }

        private void SetUpIntSetting(ASetting setting)
        {
            IntSetting intSetting = (IntSetting)setting;

            try
            {
                intSetting.RestrictionCheck(intSetting.Value);
                intSetting.Value = EditorGUILayout.IntField(intSetting.Name, (int)intSetting.Value);
            }
            catch (System.Exception e)
            {
                // Debug.LogError(e.Message);
                intSetting.Value = EditorGUILayout.IntField(intSetting.Name, (int)intSetting.Value);
                EditorGUILayout.HelpBox(e.Message, MessageType.Error);
            }
        }

        private void SetUpBoolSetting(ASetting setting)
        {
            BoolSetting boolSetting = (BoolSetting)setting;

            boolSetting.Value = EditorGUILayout.Toggle(boolSetting.Name, (bool)boolSetting.Value);
        }

        public override void OnInspectorGUI()
        {
            // Get reference to the target object
            GameSettings gameSettings = (GameSettings)target;

            Dictionary<string, List<ASetting>> settingsByCategory = SettingsUtils.GetSettingsByCategory(gameSettings);

            // Loop over all properties in the GameSettings object
            foreach (List<ASetting> settingList in settingsByCategory.Values)
            {
                if (settingList == null)
                {
                    continue;
                }

                // Label name of the category
                string category = (settingList.Count > 0 && settingList[0] != null)
                    ? settingList[0].Category : "Other";

                // Set a category label
                GUILayout.Label(category, EditorStyles.boldLabel);

                foreach (ASetting setting in settingList)
                {
                    if (setting == null)
                    {
                        continue;
                    }

                    switch (setting.Type)
                    {
                        case ASetting.SettingType.Int:
                            SetUpIntSetting(setting);
                            break;
                        case ASetting.SettingType.Bool:
                            SetUpBoolSetting(setting);
                            break;
                        default:
                            break;
                    }
                }

                // Save changes made in the inspector (usefull for scriptable objects)
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(gameSettings);
                    Repaint();
                }

                // Add Space after the category
                EditorGUILayout.Space(10);
            }
        }
    }
}
