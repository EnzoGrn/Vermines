using System;
using Vermines.Gameplay.Cards.Effect;
using Vermines.Gameplay.Errors;
using Vermines.Player;

namespace Vermines.UI
{
    /// <summary>
    /// The effect prompt currently waiting for an answer from the local player.
    /// Written ONLY by <see cref="EffectPromptPresenter"/>; read by any widget
    /// that must adapt (table sacrifice mode) or refuse to act (navigation,
    /// next phase) while a choice is pending.
    /// </summary>
    public static class EffectPromptState
    {
        private static EffectPrompt? _Current;

        /// <summary>Raised whenever the pending prompt is set or cleared.</summary>
        public static event Action Changed;

        public static bool IsPending => _Current.HasValue;

        public static bool IsActive(EffectPromptKind kind)
            => _Current.HasValue && _Current.Value.Kind == kind;

        public static void Set(EffectPrompt prompt)
        {
            _Current = prompt;
            Changed?.Invoke();
        }

        public static void Clear()
        {
            if (!_Current.HasValue)
                return;

            _Current = null;
            Changed?.Invoke();
        }

        /// <summary>
        /// Call at the top of any action that must not run while a choice is
        /// pending. Returns true (and tells the player why) if the action must
        /// be aborted.
        /// </summary>
        public static bool BlockIfPending()
        {
            if (!_Current.HasValue)
                return false;

            if (PlayerController.Local != null)
            {
                GameActionError error = new GameActionError
                {
                    Scope = ErrorScope.Local,
                    Target = PlayerController.Local.Object.InputAuthority,
                    Severity = ErrorSeverity.Minor,
                    Location = ErrorLocation.Effect,
                    MessageKey = "Effect_PromptPending"
                };

                GameEvents.OnActionRefused.Invoke(error, GameActionError.Localize(error));
            }

            return true;
        }
    }
}
