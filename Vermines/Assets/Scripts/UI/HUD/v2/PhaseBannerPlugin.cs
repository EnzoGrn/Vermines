using Fusion;
using UnityEngine;
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
        /// The parent screen is shown.
        /// Cache the connection object.
        /// </summary>
        /// <param name="screen">Parent screen</param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);

            GameEvents.OnPhaseChanged.AddListener(SetPhase);
        }

        /// <summary>
        /// The parent screen is hidden. Clear the connection object.
        /// </summary>
        /// <param name="screen">Parent screen</param>
        public override void Hide(GameplayUIScreen screen)
        {
            base.Hide(screen);

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

        /// <summary>
        /// Unity update method to update text.
        /// </summary>
        public virtual void Update()
        {
        }

        public void SetPhase(PhaseType newPhase = PhaseType.Sacrifice)
        {
            string phaseKey = $"Phase.{newPhase}";
            string localizedText = LocalizePhase(phaseKey);

            _PhaseText.text = localizedText;
        }

        private string LocalizePhase(string key)
        {
            // TODO: Replace with your actual localization system
            // e.g., return LocalizationManager.Localize(key);
            /*
            // Check if the key exists in the localization system
            if (LocalizationManager.Instance.IsKeyExists(key))
            {
                // Return the localized string
                return LocalizationManager.Instance.GetLocalizedString(key);
            }
            else
            {
                // If the key doesn't exist, return a default value or an error message
                Debug.LogWarning($"Localization key '{key}' not found. Returning default value.");
                return "Unknown Phase";
            }
            */

            // Fallback version for now
            return key switch
            {
                "Phase.Sacrifice" => "Phase: Sacrifice",
                "Phase.Gain" => "Phase: Earn",
                "Phase.Action" => "Phase: Action",
                "Phase.Resolution" => "Phase: Resolution",
                _ => "Phase: Unknown"
            };
        }
    }
}