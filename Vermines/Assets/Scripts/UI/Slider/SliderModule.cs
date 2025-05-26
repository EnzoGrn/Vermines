using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Vermines.UI {

    public class SliderModule : MonoBehaviour {

        #region Fields

        [SerializeField]
        [Tooltip("Only work on runtime, not in editor")]
        private bool _Interactable = true;

        public bool Interactable
        {
            get => _Interactable;
            set
            {
                _Interactable = value;

                SetInteractable(_Interactable);
            }
        }

        [SerializeField]
        private TMPro.TMP_Text _Value;

        [SerializeField]
        private Button _Decrease;

        [SerializeField]
        private Slider _Slider;

        [SerializeField]
        private Button _Increase;

        #endregion

        public float CurrentValue;

        #region Methods

        private void Awake()
        {
            SetInteractable(_Interactable);

            UpdateUI();
        }

        private void SetInteractable(bool interactable)
        {
            _Decrease.interactable = interactable;
            _Increase.interactable = interactable;

            _Slider.interactable   = false;
        }

        public void SetValue(float value)
        {
            CurrentValue  = Mathf.Clamp(value, 0.0f, 1.0f);
            _Slider.value = CurrentValue;

            OnValueChanged?.Invoke(CurrentValue);

            UpdateUI();
        }

        public void IncreaseProgress()
        {
            CurrentValue += 0.05f;
            CurrentValue  = Mathf.Clamp(CurrentValue, 0.0f, 1.0f);
            _Slider.value = CurrentValue;

            OnValueChanged?.Invoke(CurrentValue);

            UpdateUI();
        }

        public void DecreaseProgress()
        {
            CurrentValue -= 0.05f;
            CurrentValue  = Mathf.Clamp(CurrentValue, 0.0f, 1.0f);
            _Slider.value = CurrentValue;

            OnValueChanged?.Invoke(CurrentValue);

            UpdateUI();
        }

        private void UpdateUI()
        {
            int value = Mathf.RoundToInt(CurrentValue * 100);

            if (CurrentValue <= 0.0f) {
                _Decrease.interactable = false;
                _Increase.interactable = true;
            } else if (CurrentValue >= 1.0f) {
                _Decrease.interactable = true;
                _Increase.interactable = false;
            } else {
                _Decrease.interactable = true;
                _Increase.interactable = true;
            }

            if (_Value != null)
                _Value.text = value.ToString("F0") + "%";
        }

        #endregion

        #region Events

        public UnityEvent<float> OnValueChanged;

        #endregion
    }
}
