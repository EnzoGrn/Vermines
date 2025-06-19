using UnityEngine;
using UnityEngine.Device;
using Vermines.UI.Book;
using Vermines.UI.Screen;

namespace Vermines.UI.Plugin
{
    using Text = TMPro.TMP_Text;
    using Button = UnityEngine.UI.Button;
    using Image = UnityEngine.UI.Image;
    using static Vermines.UI.Screen.GameplayUIBook;

    /// <summary>
    /// Represents a button in the Rule Book that navigates to a specific section.
    /// </summary>
    public class RuleBookTabPlugin : GameplayScreenPlugin
    {
        [SerializeField] private BookTabType _PageType;
        public BookTabType PageType => _PageType;

        private void Awake()
        {
            // Ensure the button is set up to call OnClick when clicked.
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        /// <summary>
        /// Show the plugin.
        /// </summary>
        /// <param name="screen">
        /// The parent screen that this plugin is attached to.
        /// </param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);
        }

        /// <summary>
        /// Hide the plugin.
        /// </summary>
        public override void Hide()
        {
            base.Hide();
        }

        public void OnClick()
        {
            GameplayUIBook bookScreen = _ParentScreen as GameplayUIBook;
            if (bookScreen != null)
            {
                bookScreen.SwitchToPage(_PageType);

                foreach (var plugin in bookScreen.Plugins)
                {
                    if (plugin is RulesBookPlugin rulesPage)
                    {
                        rulesPage.GoToSection(RuleBookSectionType.Rules);
                    }
                }
            }
            else
            {
                Debug.LogError("Parent screen is not a GameplayUIBook.");
            }
        }
    }
}