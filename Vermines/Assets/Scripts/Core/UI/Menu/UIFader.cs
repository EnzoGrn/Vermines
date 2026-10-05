using DG.Tweening;
using UnityEngine;

namespace Vermines.Core.UI {

    [RequireComponent(typeof(CanvasGroup))]
    public class UIFader : UIBehaviour {

        #region Enum

        public enum FadeDirection {
            FadeIn,
            FadeOut
        }

        public enum PlayBehaviour {
            Once     = 1,
            Restart  = 2
        }

        #endregion

        #region Attributes

        public bool IsFinished => _IsFinished;
        private bool _IsFinished;

        public float StartDelay;

        public FadeDirection Direction = FadeDirection.FadeIn;
        public float Duration = 0.5f;

        public Ease Ease = Ease.OutQuad;

        public PlayBehaviour Behaviour = PlayBehaviour.Once;

        public bool ResetOnDisable = true;

        public float FadeOutValue = 0f;

        private float _StartValue;
        private float _ResetValue;
        private float _TargetValue;

        private float _Time;

        #endregion

        #region MonoBehaviour Methods

        protected void OnEnable()
        {
            if (!_IsFinished) {
                _ResetValue  = CanvasGroup.alpha;
                _StartValue  = Direction == FadeDirection.FadeIn ? FadeOutValue : _ResetValue;
                _TargetValue = Direction == FadeDirection.FadeIn ? _ResetValue  : FadeOutValue;

                _Time = -StartDelay;

                CanvasGroup.alpha = _StartValue;
            }
        }

        protected void OnDisable()
        {
            if (ResetOnDisable)
                CanvasGroup.alpha = _ResetValue;
            _IsFinished = false;
        }

        protected void Update()
        {
            if (_IsFinished) {
                if (Behaviour == PlayBehaviour.Restart) {
                    _Time       = 0f;
                    _IsFinished = false;
                } else
                    return;
            }
            _Time += Time.deltaTime;

            if (_Time <= 0f)
                return;
            if (_Time >= Duration) {
                _Time       = Duration;
                _IsFinished = true;
            }

            CanvasGroup.alpha = DOVirtual.EasedValue(_StartValue, _TargetValue, _Time / Duration, Ease);
        }

        #endregion
    }
}
