using Fusion;
using UnityEngine;
using Vermines.Gameplay.Phases.Enumerations;

namespace Vermines.UI.Plugin
{
    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Manages the navigation button plugin.
    /// </summary>
    public partial class NavButtonPlugin : GameplayScreenPlugin
    {
        /// <summary>
        /// The parent screen is shown.
        /// </summary>
        /// <param name="screen">Parent screen</param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);
        }

        /// <summary>
        /// The parent screen is hidden.
        /// </summary>
        /// <param name="screen">Parent screen</param>
        public override void Hide(GameplayUIScreen screen)
        {
            base.Hide(screen);
        }
    }
}