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

        private Action _onConfirm;
        private Action _onCancel;

        public void Setup(string title, string message, Action onConfirm, Action onCancel)
        {
            if (titleText != null)
                titleText.text = title;
            messageText.text = message;
            _onConfirm = onConfirm;
            _onCancel = onCancel;

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
