using UnityEngine.Events;
using UnityEngine;

namespace Vermines.UI {

    using Vermines.Core.UI;

    public class SliderModule : UIWidget {

        #region Fields

        [SerializeField]
        private TMPro.TMP_Text _Value;

        [SerializeField]
        private UIButton _Decrease;

        public UISlider Slider => _Slider;

        [SerializeField]
        private UISlider _Slider;

        [SerializeField]
        private UIButton _Increase;

        public bool Pourcentage;

        #endregion

        public float CurrentValue;

        #region Methods

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _Increase.onClick.AddListener(IncreaseProgress);
            _Decrease.onClick.AddListener(DecreaseProgress);
            _Slider.onValueChanged.AddListener(SetValue);

            CurrentValue = Slider.value;

            UpdateUI();
        }

        protected override void OnDeinitialize()
        {
            _Increase.onClick.RemoveListener(IncreaseProgress);
            _Decrease.onClick.RemoveListener(DecreaseProgress);
            _Slider.onValueChanged.AddListener(SetValue);

            base.OnDeinitialize();
        }

        public void SetValue(float value)
        {
            CurrentValue  = Mathf.Clamp(value, Slider.minValue, Slider.maxValue);
            _Slider.value = CurrentValue;

            OnValueChanged?.Invoke(CurrentValue);

            UpdateUI();
        }

        public void IncreaseProgress()
        {
            CurrentValue += Pourcentage ? 0.05f : 30;
            CurrentValue = Mathf.Clamp(CurrentValue, Slider.minValue, Slider.maxValue);
            _Slider.value = CurrentValue;

            OnValueChanged?.Invoke(CurrentValue);

            UpdateUI();
        }

        public void DecreaseProgress()
        {
            CurrentValue -= Pourcentage ? 0.05f : 30;
            CurrentValue  = Mathf.Clamp(CurrentValue, Slider.minValue, Slider.maxValue);
            _Slider.value = CurrentValue;

            OnValueChanged?.Invoke(CurrentValue);

            UpdateUI();
        }

        private void UpdateUI()
        {
            int value = Mathf.RoundToInt(Pourcentage ? CurrentValue * 100 : CurrentValue);

            if (CurrentValue <= Slider.minValue) {
                _Decrease.interactable = false;
                _Increase.interactable = true;
            } else if (CurrentValue >= Slider.maxValue) {
                _Decrease.interactable = true;
                _Increase.interactable = false;
            } else {
                _Decrease.interactable = true;
                _Increase.interactable = true;
            }

            if (_Value != null)
                _Value.text = value.ToString("F0") + (Pourcentage ? "%" : "");
        }

        #endregion

        #region Events

        public UnityEvent<float> OnValueChanged;

        #endregion
    }
}
