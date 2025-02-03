using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines
{
    public class GameSettingsUI : MonoBehaviour
    {
        [Header("Button Game Settings")]
        [SerializeField] private Button _ResetButton;
        [SerializeField] private Button _ApplyButton;

        [Header("Text Game Settings")]
        [SerializeField] private TMP_Text _InfoStateGameSettings; // Usefull to display error/success messages

        [Header("Input Field Game Settings")]
        [SerializeField] private GameObject _Content;

        [Header("Settings Prefab")]
        [SerializeField] private GameObject _IntSettingPrefab;
        [SerializeField] private GameObject _BoolSettingPrefab;
        [SerializeField] private GameObject _Space;
        [SerializeField] private GameObject _Title;

        [Header("Resources")]
        [SerializeField] private GameSettings _GameSettings;
        [SerializeField] private GameSettings _DefaultGameSettings;

        private Dictionary<string, GameObject> _InputFields;
        private Dictionary<string, List<ASetting>> _SettingsByCategory;
        private Dictionary<string, List<ASetting>> _DefaultSettingsByCategory;


        private void Awake()
        {
            if (_GameSettings == null)
            {
                Debug.LogError("GameSettings scriptable object not found");
            }

            _InputFields = new();
            _SettingsByCategory = SettingsUtils.GetSettingsByCategory(_GameSettings);
            _DefaultSettingsByCategory = SettingsUtils.GetSettingsByCategory(_DefaultGameSettings);
        }

        private void Start()
        {
            LoadUISettings();
        }

        private void SetTitle(List<ASetting> settingList)
        {
            GameObject Title = Instantiate(_Title);

            if (Title == null)
                return;

            // Find Label game object
            GameObject label = Title.transform.Find("Label").gameObject;

            if (label == null)
                return;

            // Label name of the category
            string category = (settingList.Count > 0 && settingList[0] != null)
                ? settingList[0].Category : "Other";

            label.GetComponent<TMP_Text>().text = category;
            Title.transform.SetParent(_Content.transform);
        }

        private void SetSpace()
        {
            GameObject Space = Instantiate(_Space);

            if (Space == null)
                return;

            Space.transform.SetParent(_Content.transform);
        }

        private void LoadUISettings()
        {
            foreach (List<ASetting> settingsList in _SettingsByCategory.Values)
            {
                SetTitle(settingsList);

                for (int i = 0; i < settingsList.Count; i++)
                {
                    ASetting setting = settingsList[i];
                    GameObject SettingPrefabUI = null;

                    if (setting.Type == ASetting.SettingType.Int) // Text Input Field
                    {
                        IntSetting intSetting = (IntSetting)setting;
                        SettingPrefabUI = Instantiate(_IntSettingPrefab);

                        // Find Input Field game object
                        GameObject inputField = SettingPrefabUI.transform.Find("InputSettings").gameObject;

                        if (inputField == null)
                            continue;

                        inputField.GetComponent<TMP_InputField>().text = intSetting.Value.ToString();
                        SettingPrefabUI.transform.SetParent(_Content.transform);
                    }
                    else if (setting.Type == ASetting.SettingType.Bool) // Toggle Input Field
                    {
                        BoolSetting boolSetting = (BoolSetting)setting;
                        SettingPrefabUI = Instantiate(_BoolSettingPrefab);

                        // Find Input Field game object
                        GameObject inputField = SettingPrefabUI.transform.Find("InputSettings").gameObject;

                        if (inputField == null)
                            continue;

                        inputField.GetComponent<Toggle>().isOn = (bool)boolSetting.Value;
                        SettingPrefabUI.transform.SetParent(_Content.transform);
                    }

                    if (SettingPrefabUI != null)
                    {
                        // Find Label game object
                        GameObject label = SettingPrefabUI.transform.Find("Label").gameObject;

                        if (label == null)
                            continue;

                        label.GetComponent<TMP_Text>().text = setting.Name;
                    }

                    _InputFields[setting.Name] = SettingPrefabUI;
                }
                SetSpace();
            }

            Dictionary<string, List<ASetting>> _SettingsByCategoryTest = SettingsUtils.GetSettingsByCategory(_GameSettings);
        }

        public void ResetGameSettings()
        {
            foreach (List<ASetting> settingsList in _SettingsByCategory.Values)
            {
                for (int i = 0; i < settingsList.Count; i++)
                {
                    try
                    {
                        ASetting setting = settingsList[i];

                        // Find Input Field game object
                        GameObject inputField = _InputFields[setting.Name].transform.Find("InputSettings").gameObject;

                        if (inputField == null) continue;

                        if (setting.Type == ASetting.SettingType.Int)
                        {
                            IntSetting intSetting = (IntSetting)setting;

                            intSetting.Value = ((IntSetting)_DefaultSettingsByCategory[intSetting.Category][i]).Value;
                            
                            inputField.GetComponent<TMP_InputField>().text = intSetting.Value.ToString();
                        }
                        else if (setting.Type == ASetting.SettingType.Bool)
                        {
                            BoolSetting boolSetting = (BoolSetting)setting;

                            boolSetting.Value = ((BoolSetting)_DefaultSettingsByCategory[boolSetting.Category][i]).Value;

                            inputField.GetComponent<Toggle>().isOn = (bool)boolSetting.Value;
                        }
                    }
                    catch (System.Exception e)
                    {
                        _InfoStateGameSettings.text = e.Message;
                        _InfoStateGameSettings.color = Color.red;
                    }
                }
            }

            _InfoStateGameSettings.text = "Game settings reset to default";
            _InfoStateGameSettings.color = Color.white;
        }

        public void ApplyGameSettings()
        {
            try
            {
                CheckGameSettings();
                ApplyChangesToGameSettings();
                _InfoStateGameSettings.text = "Game settings applied successfully";
                _InfoStateGameSettings.color = Color.green;
            }
            catch (System.Exception e)
            {
                _InfoStateGameSettings.text = e.Message;
                _InfoStateGameSettings.color = Color.red;
            }
        }

        private void CheckGameSettings()
        {
            try
            {
                foreach (List<ASetting> settingsList in _SettingsByCategory.Values)
                {
                    for (int i = 0; i < settingsList.Count; i++)
                    {
                        ASetting setting = settingsList[i];

                        // Find Input Field game object
                        GameObject inputField = _InputFields[setting.Name].transform.Find("InputSettings").gameObject;

                        if (inputField == null)
                            continue;

                        switch (setting.Type)
                        {
                            case ASetting.SettingType.Int:
                                int value = int.Parse(inputField.GetComponent<TMP_InputField>().text);

                                setting.RestrictionCheck(value);
                                break;
                            case ASetting.SettingType.Bool:
                                bool boolValue = inputField.GetComponent<Toggle>().isOn;

                                setting.RestrictionCheck(boolValue);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        private void ApplyChangesToGameSettings()
        {
            // To Apply Changes I need to loop over the ui elements and update the dictionary
            foreach (List<ASetting> settingsList in _SettingsByCategory.Values)
            {
                foreach (ASetting setting in settingsList)
                {
                    if (setting.Type == ASetting.SettingType.Int)
                    {
                        IntSetting intSetting = (IntSetting)setting;

                        // Find Input Field game object
                        GameObject inputField = _InputFields[intSetting.Name].transform.Find("InputSettings").gameObject;

                        if (inputField == null)
                            continue;

                        intSetting.Value = int.Parse(inputField.GetComponent<TMP_InputField>().text);
                    }
                    else if (setting.Type == ASetting.SettingType.Bool)
                    {
                        BoolSetting boolSetting = (BoolSetting)setting;

                        // Find Input Field game object
                        GameObject inputField = _InputFields[boolSetting.Name].transform.Find("InputSettings").gameObject;

                        if (inputField == null)
                            continue;

                        boolSetting.Value = inputField.GetComponent<Toggle>().isOn;
                    }
                }
            }
        }
    }
}
