using UnityEngine.UI;
using UnityEngine;

namespace Vermines.UI {

    using Vermines.Core.Audio;
    using Vermines.Core.UI;

    public class UIToggle : Toggle {

        #region Attributes

        [SerializeField]
        private bool _PlayValueChangedSound = true;

        [SerializeField]
        private AudioSetup _CustomValueChangedSound;

        private UIWidget _Parent;

        #endregion

        #region Methods

        protected override void Awake()
        {
            base.Awake();

            // Toggle Awake is executed in Editor as well
            if (Application.isPlaying)
                onValueChanged.AddListener(OnValueChanged);
        }

        protected override void OnDestroy()
        {
            onValueChanged.RemoveListener(OnValueChanged);

            base.OnDestroy();
        }

        #endregion

        #region Event

        private void OnValueChanged(bool isSelected)
        {
            if (!_PlayValueChangedSound)
                return;
            if (!isSelected && group != null && !group.allowSwitchOff)
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
    }
}
