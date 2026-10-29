using UnityEngine.Localization;
using UnityEngine;
using TMPro;

namespace Vermines.Core.Scene {

    /// <summary>
    /// Loads the title and the description of a loading screen from the "LoadingScreens" string table,
    /// and refreshes them when the language changes. Keys are "loading.KEY.title" and "loading.KEY.description".
    /// </summary>
    public class LoadingScreenLocalization : MonoBehaviour {

        #region Constants

        /// <summary>
        /// Name of the string table that holds the loading screen texts.
        /// </summary>
        public const string TableName = "LoadingScreens";

        #endregion

        #region Attributes

        /// <summary>
        /// Key of this loading screen in the table, for instance "proles_caduca".
        /// </summary>
        [SerializeField][Tooltip("Key of this loading screen in the LoadingScreens table, for instance proles_caduca.")]
        private string _Key;

        /// <summary>
        /// Text that shows the title.
        /// </summary>
        [SerializeField]
        private TMP_Text _Title;

        /// <summary>
        /// Text that shows the description.
        /// </summary>
        [SerializeField]
        private TMP_Text _Description;

        /// <summary>
        /// Localized title, null until the component is first enabled.
        /// </summary>
        private LocalizedString _TitleString;

        /// <summary>
        /// Localized description, null until the component is first enabled.
        /// </summary>
        private LocalizedString _DescriptionString;

        #endregion

        #region Getters & Setters

        public string Key => _Key;

        public static string GetTitleKey(string key) => $"loading.{key}.title";

        public static string GetDescriptionKey(string key) => $"loading.{key}.description";

        #endregion

        #region MonoBehaviour Methods

        protected void OnEnable()
        {
            if (string.IsNullOrEmpty(_Key))
                return;
            _TitleString       ??= new LocalizedString(TableName, GetTitleKey(_Key));
            _DescriptionString ??= new LocalizedString(TableName, GetDescriptionKey(_Key));

            // Subscribing loads the text, then calls back again on every language change.
            _TitleString.StringChanged       += OnTitleChanged;
            _DescriptionString.StringChanged += OnDescriptionChanged;
        }

        protected void OnDisable()
        {
            if (_TitleString != null)
                _TitleString.StringChanged -= OnTitleChanged;
            if (_DescriptionString != null)
                _DescriptionString.StringChanged -= OnDescriptionChanged;
        }

        #endregion

        #region Events

        private void OnTitleChanged(string value)
        {
            if (_Title != null)
                _Title.text = value;
        }

        private void OnDescriptionChanged(string value)
        {
            if (_Description != null)
                _Description.text = value;
        }

        #endregion
    }
}
