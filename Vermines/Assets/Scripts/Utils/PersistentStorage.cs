using UnityEngine;
using WebSocketSharp;

namespace Vermines.Utils {

    public static class PersistentStorage {

        #region Const

        private const int VALUE_TRUE  = 1;
        private const int VALUE_FALSE = 0;

        #endregion

        #region Setters & Getters

        public static bool GetBool(string key, bool defaultValue = false)
        {
            int defaultValueInt = defaultValue ? VALUE_TRUE : VALUE_FALSE;

            return PlayerPrefs.GetInt(key, defaultValueInt) == VALUE_TRUE;
        }

        public static void SetBool(string key, bool value, bool saveImmediately = true)
        {
            PlayerPrefs.SetInt(key, value == true ? VALUE_TRUE : VALUE_FALSE);

            if (saveImmediately)
                PlayerPrefs.Save();
        }

        public static float GetFloat(string key, float defaultValue = 0.0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static void SetFloat(string key, float value, bool saveImmediately = true)
        {
            PlayerPrefs.SetFloat(key, value);

            if (saveImmediately)
                PlayerPrefs.Save();
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static void SetInt(string key, int value, bool saveImmediately = true)
        {
            PlayerPrefs.SetInt(key, value);

            if (saveImmediately)
                PlayerPrefs.Save();
        }

        public static string GetString(string key, string defaultValue = null)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public static void SetString(string key, string value, bool saveImmediately = true)
        {
            PlayerPrefs.SetString(key, value);

            if (saveImmediately)
                PlayerPrefs.Save();
        }

        public static T GetObject<T>(string key, T defaultValue = default)
        {
            var objectJson = GetString(key);

            if (objectJson.IsNullOrEmpty())
                return defaultValue;
            return JsonUtility.FromJson<T>(objectJson);
        }

        public static void SetObject(string key, object value, bool saveImmediately = true)
        {
            var objectJson = JsonUtility.ToJson(value);

            SetString(key, objectJson, saveImmediately);
        }

        #endregion

        #region Methods

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static void Delete(string key, bool saveImmediately = true)
        {
            PlayerPrefs.DeleteKey(key);

            if (saveImmediately)
                PlayerPrefs.Save();
        }

        #endregion
    }
}
