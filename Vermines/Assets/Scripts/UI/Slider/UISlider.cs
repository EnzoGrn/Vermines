using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Vermines.UI {

    using Vermines.Core.Audio;
    using Vermines.Core.UI;
    using Vermines.Extension;

    public class UISlider : Slider {

        #region Attributes

        [SerializeField]
        private TextMeshProUGUI _ValueText;

        [SerializeField]
        private string _ValueFormat = "f1";

        [SerializeField]
        private bool _PlayValueChangedSound = true;

        [SerializeField]
        private AudioSetup _CustomValueChangedSound;

        private UIWidget _Parent;

        #endregion

        #region Getters & Setters

        public void SetValue(float value)
        {
            SetValueWithoutNotify(value);
            UpdateValueText();
        }

        #endregion

        #region MonoBehaviour Methods

        protected override void Awake()
        {
            base.Awake();

            onValueChanged.AddListener(OnValueChanged);
        }

        protected override void OnDestroy()
        {
            onValueChanged.RemoveListener(OnValueChanged);

            base.OnDestroy();
        }

        #endregion

        #region Methods

        private void UpdateValueText()
        {
            _ValueText.SetTextSafe(value.ToString(_ValueFormat));
        }

        private void PlayValueChangedSound()
        {
            if (!_PlayValueChangedSound)
                return;
            if (_Parent == null)
                _Parent = GetComponentInParent<UIWidget>();

            if (_Parent == null)
                return;
            if (_CustomValueChangedSound.Clips.Length > 0)
                _Parent.PlaySound(_CustomValueChangedSound);
            else
                _Parent.PlayClickSound();
        }

        #endregion

        #region Event

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left)
                PlayValueChangedSound();
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                PlayValueChangedSound();
            base.OnPointerUp(eventData);
        }

        private void OnValueChanged(float value)
        {
            UpdateValueText();
        }

        #endregion
    }
}
