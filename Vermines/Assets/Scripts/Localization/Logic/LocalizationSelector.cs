using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

// https://www.youtube.com/watch?v=qcXuvd7qSxg - To set up the build of the project
public class LocalizationSelector : MonoBehaviour
{
    private bool _IsActive = false;

    private void Start()
    {
        SetLocale(PlayerPrefs.GetInt("LocalID"));
    }

    // Coroutine to set the locale
    public IEnumerator SetLocale(int localeID)
    {
        _IsActive = true;
        // Wait for the localization system to initialize
        yield return LocalizationSettings.InitializationOperation;

        // Change the active locale using the ID
        if (localeID >= 0 && localeID < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            // 0 - English
            // 1 - French
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
            PlayerPrefs.SetInt("LocaleID", localeID);
            Debug.Log($"Locale changed to: {LocalizationSettings.SelectedLocale}");
        }
        else
        {
            Debug.LogWarning("Invalid locale ID");
        }
        _IsActive = false;
    }

    public void ChangeLanguage(int localeID)
    {
        if (_IsActive)
            return;

        StartCoroutine(SetLocale(localeID));
    }
}
