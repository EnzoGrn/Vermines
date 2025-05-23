using Fusion;
using UnityEngine;
using UnityEngine.Localization;
using Vermines.Gameplay.Phases.Enumerations;

namespace Vermines.UI.Plugin
{
    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Manages the display of the phase banner in the gameplay screen.
    /// </summary>
    public class PhaseBannerPlugin : GameplayScreenPlugin
    {
        /// <summary>
        /// The phase text.
        /// Can't be null.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _PhaseText;

        /// <summary>
        /// Show the plugin.
        /// </summary>
        /// <param name="screen">
        /// The parent screen that this plugin is attached to.
        /// </param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);

            GameEvents.OnPhaseChanged.AddListener(SetPhase);
        }

        /// <summary>
        /// Hide the plugin.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            GameEvents.OnPhaseChanged.RemoveListener(SetPhase);
        }

        public virtual void Start()
        {
            if (_PhaseText == null)
            {
                Debug.LogErrorFormat(
                    gameObject,
                    "PhaseBannerPlugin Critical Error: Missing 'TextMeshProUGUI' reference on GameObject '{0}'. This component is required to render phase banners. Please assign a valid TextMeshProUGUI in the Inspector.",
                    gameObject.name
                );
                return;
            }
            SetPhase(PhaseType.Sacrifice);
        }

        public void SetPhase(PhaseType newPhase = PhaseType.Sacrifice)
        {
            string phaseKey = $"Phase.{newPhase}";
            string localizedText = LocalizePhase(phaseKey);

            _PhaseText.text = localizedText;
        }

        private string LocalizePhase(string key)
        {
            // Check if the key exists in the localization system
            LocalizedString localized = new LocalizedString("PhaseTable", key);
            return localized.GetLocalizedString();
        }
    }
}