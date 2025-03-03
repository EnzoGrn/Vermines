using UnityEngine;
using TMPro;

namespace Vermines.HUD {

    using Vermines.Gameplay.Phases.Enumerations;

    public class PhaseBanner : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private PhaseType currentPhase;

        void Start()
        {
            UpdatePhaseText();
        }

        public void SetPhase(PhaseType newPhase)
        {
            currentPhase = newPhase;
            UpdatePhaseText();
        }

        private void UpdatePhaseText()
        {
            if (phaseText != null)
            {
                switch (currentPhase)
                {
                    case PhaseType.Sacrifice:
                        phaseText.text = "Phase: Sacrifice";
                        break;
                    case PhaseType.Gain:
                        phaseText.text = "Phase: Earn";
                        break;
                    case PhaseType.Action:
                        phaseText.text = "Phase: Action";
                        break;
                    case PhaseType.Resolution:
                        phaseText.text = "Phase: Resolution";
                        break;
                    default:
                        phaseText.text = "Phase: Unknown";
                        break;
                }
            }
            else
            {
                Debug.LogWarning("PhaseBanner: Le composant Text n'est pas assigné.");
            }
        }
    }
}
