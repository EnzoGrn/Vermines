using System.Collections;
using UnityEngine;
using Vermines.Gameplay.Errors;

namespace Vermines.UI
{
    using Text = TMPro.TMP_Text;

    /// <summary>
    /// Permanent HUD widget showing a brief toast whenever a game action is
    /// refused by the server (GameEvents.OnActionRefused), regardless of
    /// which action failed. Not a modal screen - always present, subscribes
    /// itself, never shown/hidden by an external Controller.
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
        }

        private void OnDestroy()
        {
            GameEvents.OnActionRefused.RemoveListener(OnActionRefused);
        }

        private void OnActionRefused(GameActionError error, string localizedMessage)
        {
            if (messageText != null)
                messageText.text = localizedMessage;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

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
