using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Vermines.Config.Utils {

    public class SettingsUtils {

        // TODO : Change GameSettings for a generic type
        public static Dictionary<string, List<ASettingBase>> GetSettingsByCategory(GameConfiguration gameSettings)
        {
            Dictionary<string, List<ASettingBase>> settingsByCategory = new();

            foreach (FieldInfo property in gameSettings.GetType().GetFields()) {
                try {
                    if (property.GetValue(gameSettings) is ASettingBase setting) {
                        if (!settingsByCategory.ContainsKey(setting.Category))
                            settingsByCategory[setting.Category] = new List<ASettingBase>();
                        settingsByCategory[setting.Category].Add(setting);
                    }
                } catch (Exception e) {
                    Debug.LogWarning($"Error processing setting {property.Name}: {e.Message}");
                }
            }

            return settingsByCategory;
        }
    }
}
