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
                        phaseText.text = "Phase de Sacrifice";
                        break;
                    case PhaseType.Gain:
                        phaseText.text = "Phase de Gain";
                        break;
                    case PhaseType.Action:
                        phaseText.text = "Phase d'Action";
                        break;
                    case PhaseType.Resolution:
                        phaseText.text = "Phase de Résolution";
                        break;
                    default:
                        phaseText.text = "Phase Inconnue";
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
