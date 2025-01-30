using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vermines.HUD
{
    public class PhaseBanner : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI phaseText; // Référence au composant Text de l'UI
        [SerializeField] private PhaseType currentPhase; // Phase actuelle

        // Appelé une fois avant la première exécution d'Update
        void Start()
        {
            UpdatePhaseText();
        }

        // Méthode pour mettre à jour le texte affiché
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
