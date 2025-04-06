using UnityEngine;
using TMPro;

namespace Vermines.UI
{
    using Vermines.Gameplay.Phases.Enumerations;

    public class PhaseBannerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI phaseText;

        private void Start()
        {
            SetPhase(PhaseType.Sacrifice);
        }

        public void SetPhase(PhaseType newPhase)
        {
            if (phaseText == null)
            {
                Debug.LogError("PhaseBannerUI: TextMeshProUGUI component is not assigned.");
                return;
            }

            string phaseKey = $"Phase.{newPhase}";
            string localizedText = LocalizePhase(phaseKey);

            phaseText.text = localizedText;
        }

        private string LocalizePhase(string key)
        {
            // TODO: Replace with your actual localization system
            // e.g., return LocalizationManager.Localize(key);

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
