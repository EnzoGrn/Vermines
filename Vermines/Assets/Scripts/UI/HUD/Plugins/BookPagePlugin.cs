using Fusion;
using UnityEngine;
using UnityEngine.Localization;
using Vermines.Gameplay.Phases.Enumerations;

namespace Vermines.UI.Plugin
{
    using static Vermines.UI.Screen.GameplayUIBook;
    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Manages the display of the phase banner in the gameplay screen.
    /// </summary>
    public class BookPagePlugin : GameplayScreenPlugin
    {
        [SerializeField] private BookTabType _PageType;
        public BookTabType PageType => _PageType;

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
    }
}