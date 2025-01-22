using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Vermines
{
    public class SettingsUtils
    {
        // TODO : Change GameSettings for a generic type
        public static Dictionary<string, List<ASetting>> GetSettingsByCategory(GameSettings gameSettings)
        {
            Dictionary<string, List<ASetting>> settingsByCategory = new Dictionary<string, List<ASetting>>();

            foreach (FieldInfo property in gameSettings.GetType().GetFields())
            {
                try
                {
                    ASetting setting = (ASetting)property.GetValue(gameSettings);

                    if (setting == null)
                    {
                        continue;
                    }

                    if (!settingsByCategory.ContainsKey(setting.Category))
                    {
                        settingsByCategory[setting.Category] = new List<ASetting>();
                    }

                    settingsByCategory[setting.Category].Add(setting);
                    Debug.Log("Setting added to category: " + setting.Category);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("WARNING -> " + e.Message);
                }
            }

            return settingsByCategory;
        }
    }
}
