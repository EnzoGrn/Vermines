using System;
using UnityEngine;
using UnityEngine.UI;
using Vermines.CardSystem.Enumerations;

namespace Vermines.UI.Screen
{
    using Text = TMPro.TMP_Text;
    using InputField = TMPro.TMP_InputField;

    public class GameplayUISpendEffect : GameplayUIScreen, IParamReceiver<(Action<int> onDone, DataType dataToSpend, DataType dataToEarn, int multiplicator)>
    {
        #region Attributes

        [Header("UI Elements")]
        [SerializeField] private InputField amountInputField;
        [SerializeField] private Button doneButton;
        [SerializeField] private Text earnPreviewLabel;
        [SerializeField] private Text spendLabel;

        private Action<int> _onDoneCallback;
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

        public void SetParam((Action<int> onDone, DataType dataToSpend, DataType dataToEarn, int multiplicator) param)
        {
            _onDoneCallback = param.onDone;
            _dataToSpend = param.dataToSpend;
            _dataToEarn = param.dataToEarn;
            _multiplicator = param.multiplicator;

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
            _onDoneCallback?.Invoke(_currentAmount);
            Controller.Hide();
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
