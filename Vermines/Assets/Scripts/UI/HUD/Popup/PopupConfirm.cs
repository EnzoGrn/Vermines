using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Vermines.UI.Popup
{
    public class PopupConfirm : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        public event Action OnClosed;
        public void ClearOnClosed() => OnClosed = null;
        private Action _onConfirm;
        private Action _onCancel;

        public void Setup(string title, string message, Action onConfirm, Action onCancel)
        {
            // Clean previous listeners
            confirmButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();

            // Set content
            if (titleText != null)
                titleText.text = title;
            messageText.text = message;

            // Store callbacks
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            // Add current listeners
            confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Cancel);
        }

        private void Confirm()
        {
            _onConfirm?.Invoke();
            Close();
        }

        private void Cancel()
        {
            _onCancel?.Invoke();
            Close();
        }

        private void Close()
        {
            OnClosed?.Invoke();
        }

        public void ForceClose()
        {
            OnClosed?.Invoke();
        }
    }
}
