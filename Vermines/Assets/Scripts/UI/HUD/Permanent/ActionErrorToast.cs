using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Errors;

namespace Vermines.UI
{
    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Permanent HUD widget showing a brief toast for two kinds of events:
    /// a game action refused by the server (GameEvents.OnActionRefused) and
    /// an effect skipped for lack of a valid target (GameEvents.OnEffectSkipped).
    /// Not a modal screen - always present, subscribes itself, never
    /// shown/hidden by an external Controller.
    /// </summary>
    public class ActionErrorToast : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text messageText;
        [SerializeField] private float displayDuration = 3f;

        private Coroutine _hideCoroutine;

        private void Awake()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            GameEvents.OnActionRefused.AddListener(OnActionRefused);
            GameEvents.OnEffectSkipped.AddListener(OnEffectSkipped);
        }

        private void OnDestroy()
        {
            GameEvents.OnActionRefused.RemoveListener(OnActionRefused);
            GameEvents.OnEffectSkipped.RemoveListener(OnEffectSkipped);
        }

        private void OnActionRefused(GameActionError error, string localizedMessage)
        {
            Show(localizedMessage);
        }

        private void OnEffectSkipped(ICard source, string reasonKey)
        {
            Debug.Log($"Effect skipped: {reasonKey} (source: {(source != null ? source.Data.Name : "null")})");
            LocalizedString text = new LocalizedString("EffectTable", reasonKey)
            {
                Arguments = new object[] { source != null ? source.Data.Name : string.Empty }
            };

            Show(text.GetLocalizedString());
        }

        private void Show(string message)
        {
            if (messageText != null)
                messageText.text = message;

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);
            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            _hideCoroutine = null;
        }
    }
}
