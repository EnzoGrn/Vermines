using UnityEngine;
using UnityEngine.Localization;
using Vermines.Core.Scene;
using Vermines.Gameplay.Phases;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.Player;

namespace Vermines.UI
{
    using Button = UnityEngine.UI.Button;
    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Permanent HUD widget: the "next phase" button. Subscribes in Awake, so it
    /// stays up to date whatever screen is open.
    /// </summary>
    public class PhaseButtonWidget : MonoBehaviour
    {
        [SerializeField] private Button _Button;
        [SerializeField] private GameplayUIController _Controller;

        private Text _Label;

        private void Awake()
        {
            if (_Controller == null)
                _Controller = FindFirstObjectByType<GameplayUIController>(FindObjectsInactive.Include);

            if (_Button == null)
                Debug.LogError("[PhaseButtonWidget] Missing Button reference.", this);
            else
                _Label = _Button.GetComponentInChildren<Text>();

            GameEvents.OnPhaseChanged.AddListener(HandlePhaseChanged);
            GameEvents.OnTurnChanged.AddListener(HandleTurnChanged);
            GameEvents.OnPlayerInitialized.AddListener(Refresh);
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            GameEvents.OnPhaseChanged.RemoveListener(HandlePhaseChanged);
            GameEvents.OnTurnChanged.RemoveListener(HandleTurnChanged);
            GameEvents.OnPlayerInitialized.RemoveListener(Refresh);
        }

        private void HandlePhaseChanged(PhaseType _) => Refresh();
        private void HandleTurnChanged(int _) => Refresh();

        private void Refresh()
        {
            if (_Button == null || !PlayerController.Local)
                return;

            SceneContext context = PlayerController.Local.Context;
            bool isMyTurn = context.GameplayMode.IsMyTurn;

            _Button.interactable = isMyTurn;

            if (_Label == null)
                return;

            string key = isMyTurn
                ? GetTranslationKey(context.GameplayMode.PhaseManager.CurrentPhase)
                : "ui.button.wait_your_turn";

            _Label.text = new LocalizedString("UIUtils", key).GetLocalizedString();
        }

        private static string GetTranslationKey(PhaseType phase)
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

        /// <summary>Wire the button's onClick to this method.</summary>
        public void OnClick()
        {
            if (!PlayerController.Local || _Controller == null)
                return;
            if (EffectPromptState.BlockIfPending())
                return;

            SceneContext context = PlayerController.Local.Context;
            PhaseManager phaseManager = context.GameplayMode.PhaseManager;

            if (!Vermines.Tutorial.Integration.TutorialGameHooks.AllowNextPhase(phaseManager.CurrentPhase))
                return;

            if (context.HandManager.HasCards(true) && phaseManager.CurrentPhase == PhaseType.Action)
            {
                _Controller.ShowDualPopup(new DefaultDiscardStrategy());

                return;
            }

            phaseManager.Phases[phaseManager.CurrentPhase].OnPhaseEnding(context.Runner.LocalPlayer, false);
        }
    }
}
