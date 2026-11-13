using UnityEngine;
using UnityEngine.UI;

namespace Vermines.UI {

    using Vermines.UI.View;

    /// <summary>
    /// HUD book button: opens the pause menu.
    /// Feedback comes from NavButtonJuice, like the nav bar buttons, with its lift direction set to down
    /// since this button hangs from the top edge.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MenuButtonWidget : MonoBehaviour {

        #region Attributes

        [Tooltip("Found in the scene when empty.")]
        [SerializeField] private UIGameplayMenu _Menu;

        [Tooltip("Optional. Same component as the nav bar buttons, with _LiftDirection (0, -1).")]
        [SerializeField] private NavButtonJuice _Juice;

        private Button _Button;

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            _Button = GetComponent<Button>();

            if (_Juice == null)
                TryGetComponent(out _Juice);
            if (_Menu == null)
                _Menu = FindFirstObjectByType<UIGameplayMenu>(FindObjectsInactive.Include);
            if (_Menu == null)
                Debug.LogError("[MenuButtonWidget] UIGameplayMenu not found in the scene.", this);
        }

        private void OnEnable()
        {
            _Button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _Button.onClick.RemoveListener(OnClick);
        }

        #endregion

        #region Events

        private void OnClick()
        {
            if (_Menu == null) {
                if (_Juice != null)
                    _Juice.PlayRefused();
                return;
            }

            if (_Juice != null)
                _Juice.PlayClick();
            _Menu.Open();
        }

        #endregion
    }
}
