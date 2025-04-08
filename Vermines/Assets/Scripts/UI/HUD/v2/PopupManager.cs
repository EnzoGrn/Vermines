using DG.Tweening;
using System;
using UnityEngine;

namespace Vermines.UI.Popup
{
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager Instance;

        [Header("Prefabs")]
        [SerializeField] private GameObject overlayBackgroundPrefab;
        [SerializeField] private GameObject popupConfirmPrefab;

        private GameObject _currentOverlay;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void ShowConfirm(string message, Action onConfirm, Action onCancel = null)
        {
            ShowConfirm("Confirmation", message, onConfirm, onCancel);
        }

        public void ShowConfirm(string title, string message, Action onConfirm, Action onCancel = null)
        {
            Debug.Log("[PopupManager] ShowConfirm: " + message);
            if (_currentOverlay == null)
            {
                _currentOverlay = Instantiate(overlayBackgroundPrefab, transform);
                var canvasGroup = _currentOverlay.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                    canvasGroup.alpha = 0f;
                canvasGroup?.DOFade(1f, 0.2f);
            }

            var popup = Instantiate(popupConfirmPrefab, transform);
            var popupScript = popup.GetComponent<PopupConfirm>();
            popupScript.Setup(title, message, onConfirm, onCancel);

            popupScript.OnClosed += () =>
            {
                Destroy(popup);
                if (_currentOverlay != null)
                {
                    var canvasGroup = _currentOverlay.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
                        {
                            Destroy(_currentOverlay);
                            _currentOverlay = null;
                        });
                    }
                    else
                    {
                        Destroy(_currentOverlay);
                        _currentOverlay = null;
                    }
                }
            };
        }
    }
}
