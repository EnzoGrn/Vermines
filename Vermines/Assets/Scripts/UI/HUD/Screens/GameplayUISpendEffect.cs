using System;
using UnityEngine;
using UnityEngine.UI;
using Vermines.CardSystem.Enumerations;
using Vermines.Gameplay.Cards.Effect;

namespace Vermines.UI.Screen
{
    using InputField = TMPro.TMP_InputField;
    using Text = TMPro.TMP_Text;

    public class GameplayUISpendEffect : GameplayUIScreen, IParamReceiver<EffectPrompt>
    {
        #region Attributes

        [Header("UI Elements")]
        [SerializeField] private InputField amountInputField;
        [SerializeField] private Button doneButton;
        [SerializeField] private Text earnPreviewLabel;
        [SerializeField] private Text spendLabel;

        private DataType _dataToSpend;
        private DataType _dataToEarn;
        private int _multiplicator;
        private int _currentAmount = 0;

        #endregion

        #region Overrides

        public override void Show()
        {
            base.Show();
            amountInputField.onValueChanged.AddListener(OnAmountChanged);
            doneButton.onClick.AddListener(OnDoneButtonPressed);
            UpdateEarnPreview();
        }

        public override void Hide()
        {
            base.Hide();
            amountInputField.onValueChanged.RemoveListener(OnAmountChanged);
            doneButton.onClick.RemoveListener(OnDoneButtonPressed);
        }

        #endregion

        #region Param Receiver

        public void SetParam(EffectPrompt prompt)
        {
            _dataToSpend = prompt.DataToSpend;
            _dataToEarn = prompt.DataToEarn;
            _multiplicator = prompt.Multiplicator;

            _currentAmount = 0;

            if (amountInputField != null)
                amountInputField.text = string.Empty;

            UpdateEarnPreview();
        }

        #endregion

        #region Events

        private void OnAmountChanged(string value)
        {
            if (int.TryParse(value, out int result))
                _currentAmount = Mathf.Max(0, result);
            else
                _currentAmount = 0;

            UpdateEarnPreview();
        }

        public void OnDoneButtonPressed()
        {
            GameEvents.OnEffectSpendSubmitted.Invoke(_currentAmount);
        }

        #endregion

        #region Private Methods

        private void UpdateEarnPreview()
        {
            int earnAmount = _currentAmount * _multiplicator;

            if (spendLabel != null)
                spendLabel.text = $"{_currentAmount}";

            if (earnPreviewLabel != null)
                earnPreviewLabel.text = $"{earnAmount}";
        }

        #endregion
    }
}
