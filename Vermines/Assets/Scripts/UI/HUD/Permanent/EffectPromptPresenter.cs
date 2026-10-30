using UnityEngine;
using Vermines.CardSystem.Enumerations;
using Vermines.Gameplay.Cards.Effect;
using Vermines.Gameplay.Phases.Enumerations;
using Vermines.ShopSystem.Enumerations;

namespace Vermines.UI
{
    using Vermines.UI.Screen;

    /// <summary>
    /// Permanent HUD component that turns effect prompts (raised by game logic)
    /// into screens, and keeps <see cref="EffectPromptState"/> up to date.
    /// Put it on the same GameObject as the GameplayUIController (it must be
    /// active at load: it subscribes in Awake).
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
            GameEvents.OnShopOpenRequested.AddListener(OnShopOpenRequested);
            GameEvents.OnPhaseChanged.AddListener(OnPhaseChanged);
            GameEvents.OnTurnTimerExpired.AddListener(OnTurnTimerExpired);
        }

        private void OnDestroy()
        {
            GameEvents.OnEffectPromptRequested.RemoveListener(OnPromptRequested);
            GameEvents.OnEffectPromptClosed.RemoveListener(OnPromptClosed);
            GameEvents.OnShopOpenRequested.RemoveListener(OnShopOpenRequested);
            GameEvents.OnPhaseChanged.RemoveListener(OnPhaseChanged);
            GameEvents.OnTurnTimerExpired.RemoveListener(OnTurnTimerExpired);

            // The state is static: never let it leak across scene reloads.
            EffectPromptState.Clear();
        }

        private void OnPromptRequested(EffectPrompt prompt)
        {
            if (_Controller == null)
                return;

            // Set the state first so screens shown below already see it.
            EffectPromptState.Set(prompt);

            // Remember the current screen so it comes back when the prompt closes.
            _Controller.GetActiveScreen(out GameplayUIScreen last);

            switch (prompt.Kind)
            {
                case EffectPromptKind.Reborn:
                    _Controller.ShowWithParams<GameplayUIRebornEffect, EffectPrompt>(prompt, last);
                    break;

                case EffectPromptKind.Copy:
                    _Controller.ShowWithParams<GameplayUICopyEffect, EffectPrompt>(prompt, last);
                    break;

                case EffectPromptKind.Remove:
                    _Controller.ShowWithParams<GameplayUISacrifice, CardType>(prompt.CardType, last);
                    break;

                case EffectPromptKind.Spend:
                    _Controller.ShowWithParams<GameplayUISpendEffect, EffectPrompt>(prompt, last);
                    break;

                // Board prompts: the player answers directly on the table.
                case EffectPromptKind.Sacrifice:
                case EffectPromptKind.Discard:
                    _Controller.Show<GameplayUITable>(last);
                    break;
            }
        }

        private void OnPromptClosed(EffectPromptKind kind)
        {
            EffectPromptState.Clear();

            if (_Controller == null)
                return;

            // Board prompts live on a screen that stays open (the table).
            if (kind == EffectPromptKind.Sacrifice || kind == EffectPromptKind.Discard)
                return;

            // Picker prompts: hide the picker and restore the previous screen.
            _Controller.Hide();
        }

        private void OnShopOpenRequested(ShopType shopType)
        {
            if (_Controller == null)
            {
                Debug.LogError("EffectPromptPresenter: GameplayUIController is null, cannot open shop.");
                return;
            }

            _Controller.GetActiveScreen(out GameplayUIScreen last);
            _Controller.ShowWithParams<GameplayUIShop, ShopType>(shopType, last);
        }

        // Safety net: a phase change always ends a pending prompt, so the lock
        // can never outlive the turn (same role as the old ClearContext()).
        private void OnPhaseChanged(PhaseType phase)
        {
            EffectPromptState.Clear();
        }

        private void OnTurnTimerExpired()
        {
            // A confirm popup (sacrifice, play effect...) must not outlive the
            // turn, and must be CANCELLED, not confirmed.
            if (_Controller != null)
                _Controller.CancelDualPopup();

            bool pickerOpen = EffectPromptState.IsPending
                && !EffectPromptState.IsActive(EffectPromptKind.Sacrifice)
                && !EffectPromptState.IsActive(EffectPromptKind.Discard);

            EffectPromptState.Clear();

            // Board prompts live on the table (it stays open); picker prompts
            // have their own screen, which must be closed.
            if (pickerOpen && _Controller != null)
                _Controller.Hide();
        }

    }
}
