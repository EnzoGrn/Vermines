using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines.UI {

    using Vermines.Core.Audio;
    using Vermines.Core.UI;

    public class UIButton : Button {

        #region Attributes

        private static List<UIWidget> _TempWidgetList = new(16);

        [SerializeField]
        private bool _PlayClickSound = true;

        [SerializeField]
        private AudioSetup _CustomClickSound;

        private UIWidget _Parent;

        #endregion

        #region Methods

        public void PlayClickSound()
        {
            if (!_PlayClickSound)
                return;
            if (_Parent == null) {
                _TempWidgetList.Clear();

                GetComponentsInParent(true, _TempWidgetList);

                _Parent = _TempWidgetList.Count > 0 ? _TempWidgetList[0] : null;

                _TempWidgetList.Clear();
            }

            if (_CustomClickSound.Clips.Length > 0)
                _Parent.PlaySound(_CustomClickSound);
            else
                _Parent.PlayClickSound();
        }

        private void OnClick()
        {
            PlayClickSound();
        }

        #endregion

        #region Monobehaviour Methods

        protected override void Awake()
        {
            base.Awake();

            onClick.AddListener(OnClick);

            if (transition == Transition.Animation) {
                Animator buttonAnimator = animator;

                if (buttonAnimator != null)
                    buttonAnimator.keepAnimatorStateOnDisable = true;
            }
        }

        protected override void OnDestroy()
        {
            onClick.RemoveListener(OnClick);

            base.OnDestroy();
        }

        #endregion
    }
}
