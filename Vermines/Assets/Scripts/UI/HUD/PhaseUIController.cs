using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vermines.Gameplay.Phases;
using Vermines.Gameplay.Phases.Enumerations;

namespace Vermines.UI
{
    public class PhaseUIController : MonoBehaviour
    {
        [SerializeField] private PhaseBannerUI phaseBanner;
        [SerializeField] private Button nextPhaseButton;
        [SerializeField] private TextMeshProUGUI buttonText;

        private void OnEnable()
        {
            GameEvents.OnPhaseChanged.AddListener(UpdateUI);
        }

        private void OnDisable()
        {
            GameEvents.OnPhaseChanged.RemoveListener(UpdateUI);
        }

        private void UpdateUI(PhaseType currentPhase = PhaseType.Sacrifice)
        {
            bool isMyTurn = GameManager.Instance.IsMyTurn();

            phaseBanner.SetPhase(currentPhase);

            nextPhaseButton.interactable = isMyTurn;

            var labelKey = isMyTurn ? GetButtonTranslationKey(currentPhase) : "ui.button.wait_your_turn";

            buttonText.text = Translate(labelKey);
        }

        private string GetButtonTranslationKey(PhaseType phase)
        {
            return phase switch
            {
                PhaseType.Sacrifice => "ui.button.next_gain",
                PhaseType.Gain => "ui.button.next_action",
                PhaseType.Action => "ui.button.next_resolution",
                PhaseType.Resolution => "ui.button.next_player",
                _ => "ui.button.unknown_phase"
            };
        }

        private string Translate(string key)
        {
            // TODO: Integrate with the localization system
            // Futur example: return LocalizationManager.Instance.GetLocalizedString(key);

            // For now, just return the key
            return key;
        }
    }
}
