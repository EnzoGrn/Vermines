using UnityEngine;
using TMPro;

namespace Vermines.Core.Scene {

    using Vermines.Core.UI;
    using Vermines.Extension;

    public class LoadingScene : MonoBehaviour {

        #region Attributes

        /// <summary>
        /// Learned loading times, shared by every loading of the session.
        /// </summary>
        private static LoadingTimeEstimator _Estimator;

        private UIFader _ActiveFader;

        [SerializeField]
        private UIFader _FadeInObject;

        [SerializeField]
        private UIFader _FadeOutObject;

        [SerializeField]
        private TextMeshProUGUI _Status;

        [SerializeField]
        private TextMeshProUGUI _StatusDescription;

        /// <summary>
        /// Rotating loading screens, moved into the active fader so they fade with it.
        /// </summary>
        [SerializeField]
        private LoadingScreenCarousel _Carousel;

        /// <summary>
        /// True when the fade out was asked but waits for the current loading screen to be seen long enough.
        /// </summary>
        private bool _IsFadeOutPending;

        #endregion

        #region Getters & Setters

        public bool IsFading => _IsFadeOutPending || (_ActiveFader != null && !_ActiveFader.IsFinished);

        private static LoadingTimeEstimator Estimator => _Estimator ??= new LoadingTimeEstimator();

        #endregion

        #region MonoBehaviour Methods

        protected void Awake()
        {
            _FadeInObject.SetActive(false);
            _FadeOutObject.SetActive(false);

            if (_Carousel != null)
                _Carousel.RemainingTimeProvider = Estimator.GetRemainingTime;
        }

        protected void Update()
        {
            if (_Status != null && _Status.text != null)
                _Status.text = Global.Networking.Status;
            if (_StatusDescription != null && _StatusDescription.text != null)
                _StatusDescription.text = Global.Networking.StatusDescription;
            Estimator.SetStage($"{Global.Networking.Status}/{Global.Networking.StatusDescription}");

            if (_IsFadeOutPending && (_Carousel == null || _Carousel.HasSettled)) {
                _IsFadeOutPending = false;

                StartFadeOut();
            }
        }

        #endregion

        #region Methods

        public void FadeIn()
        {
            _IsFadeOutPending = false;

            Estimator.Begin();
            AttachCarousel(_FadeInObject, true);

            _FadeInObject.SetActive(true);
            _FadeOutObject.SetActive(false);

            _ActiveFader = _FadeInObject;
        }

        /// <summary>
        /// Fades the loading screen out, once the screen on display has been seen long enough.
        /// </summary>
        public void FadeOut()
        {
            Estimator.End();

            if (_Carousel != null) {
                _Carousel.SetRotating(false);

                if (!_Carousel.HasSettled) {
                    _IsFadeOutPending = true;

                    return;
                }
            }
            StartFadeOut();
        }

        private void StartFadeOut()
        {
            // Keep the screen on display while it fades out.
            AttachCarousel(_FadeOutObject, false);

            _FadeInObject.SetActive(false);
            _FadeOutObject.SetActive(true);

            _ActiveFader = _FadeOutObject;
        }

        private void AttachCarousel(UIFader fader, bool isRotating)
        {
            if (_Carousel == null)
                return;
            _Carousel.SetRotating(isRotating);
            _Carousel.transform.SetParent(fader.transform, false);
            _Carousel.transform.SetAsFirstSibling();
        }

        #endregion
    }
}
