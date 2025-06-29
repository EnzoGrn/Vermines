using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class NarrationDialog : MonoBehaviour
{
    #region Exposed Properties
    [SerializeField] private TMPro.TextMeshProUGUI _subtitleText;
    [SerializeField] private float _charDelay = 0.03f;
    [SerializeField] private float _lineDelay = 2f;
    [SerializeField] private string _tableName;
    #endregion

    #region Private Properties
    private List<string> _dialogLines;
    private string _keyName;
    #endregion

    #region Public Properties
    public UnityEvent OnDialogComplete = new UnityEvent();
    #endregion

    void Start()
    {
        _dialogLines = new List<string>();
    }

    private void OnEnable()
    {
        // Start the dialog immediately when the object is enabled
        StartDialog();
    }

    /// <summary>
    /// Starts the dialog with the specified key name from the localization table.
    /// </summary>
    public void StartDialog()
    {
        StringTable table = LocalizationSettings.StringDatabase.GetTable(_tableName);
        _subtitleText.text = "";

        if (table == null)
        {
            Debug.LogError($"String table '{_tableName}' not found.");
            return;
        }

        StringTableEntry entry = table.GetEntry(_keyName);

        if (entry == null)
        {
            Debug.LogError($"Entry '{_keyName}' not found in table '{_tableName}'.");
            return;
        }

        _dialogLines = new List<string>(entry.LocalizedValue.Split(new[] { '\n', '.' }, StringSplitOptions.RemoveEmptyEntries));

        StartCoroutine(PlayNarration());
    }

    public void SetKeyName(string keyName)
    {
        _keyName = keyName;
    }

    public void SkipEntireDialog()
    {
        StopAllCoroutines();
        _subtitleText.text = "";
        _dialogLines.Clear();
        OnDialogComplete.Invoke();
    }

    /// <summary>
    /// Plays the narration by iterating through the dialog lines and displaying them one by one.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayNarration()
    {
        foreach (string line in _dialogLines)
        {
            yield return StartCoroutine(DisplayLine(line));

            // Wait for the line delay or until the user skips
            float timer = 0f;
            while (timer < _lineDelay)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }
        OnDialogComplete.Invoke();
        _subtitleText.text = "";
        _dialogLines.Clear();
    }

    /// <summary>
    /// Displays a single line of text character by character with a delay.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private IEnumerator DisplayLine(string line)
    {
        _subtitleText.text = "";
        foreach (char c in line)
        {
            _subtitleText.text += c;
            yield return new WaitForSeconds(_charDelay);
        }
    }
}
