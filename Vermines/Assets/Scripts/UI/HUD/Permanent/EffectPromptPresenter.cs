using UnityEngine;
using Vermines.Gameplay.Cards.Effect;
using Vermines.CardSystem.Enumerations;

namespace Vermines.UI
{
    using Vermines.UI.Screen;

    /// <summary>
    /// Permanent HUD component that turns effect prompts (raised by game logic)
    /// into screens. Put it on the same GameObject as the GameplayUIController
    /// (it must be active at load: it subscribes in Awake).
    /// </summary>
    public class EffectPromptPresenter : MonoBehaviour
    {
        [SerializeField] private GameplayUIController _Controller;

        private void Awake()
        {
            if (_Controller == null)
                _Controller = GetComponent<GameplayUIController>();

            GameEvents.OnEffectPromptRequested.AddListener(OnPromptRequested);
            GameEvents.OnEffectPromptClosed.AddListener(OnPromptClosed);
        }

        private void OnDestroy()
        {
            GameEvents.OnEffectPromptRequested.RemoveListener(OnPromptRequested);
            GameEvents.OnEffectPromptClosed.RemoveListener(OnPromptClosed);
        }

        private void OnPromptRequested(EffectPrompt prompt)
        {
            if (_Controller == null)
                return;

            // Remember the current screen so it comes back when the prompt closes.
            _Controller.GetActiveScreen(out GameplayUIScreen last);

            switch (prompt.Kind)
            {
                case EffectPromptKind.Reborn:
                    _Controller.ShowWithParams<GameplayUIRebornEffect, CardSelectedEffectContext>(
                        new CardSelectedEffectContext(prompt.CardType, prompt.Source), last);
                    break;
                case EffectPromptKind.Copy:
                    _Controller.ShowWithParams<GameplayUICopyEffect, CardSelectedEffectContext>(
                        new CardSelectedEffectContext(prompt.CardType, prompt.Source), last);
                    break;
                case EffectPromptKind.Remove:
                    _Controller.ShowWithParams<GameplayUISacrifice, CardType>(prompt.CardType, last);
                    break;
            }
        }

        private void OnPromptClosed(EffectPromptKind kind)
        {
            if (_Controller == null)
                return;

            // Same behavior as the former CardRebornContext.Exit(): hide the
            // active screen and restore the previous one.
            _Controller.Hide();
        }
    }
}
