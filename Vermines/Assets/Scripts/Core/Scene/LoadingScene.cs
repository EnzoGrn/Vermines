using UnityEngine;
using TMPro;

namespace Vermines.Core.Scene {

    using Vermines.Core.UI;
    using Vermines.Extension;

    public class LoadingScene : MonoBehaviour {

        #region Attributes

        private UIFader _ActiveFader;

        [SerializeField]
        private UIFader _FadeInObject;

        [SerializeField]
        private UIFader _FadeOutObject;

        [SerializeField]
        private TextMeshProUGUI _Status;

        [SerializeField]
        private TextMeshProUGUI _StatusDescription;

        #endregion

        #region Getters & Setters

        public bool IsFading => _ActiveFader != null && !_ActiveFader.IsFinished;

        #endregion

        #region MonoBehaviour Methods

        protected void Awake()
        {
            _FadeInObject.SetActive(false);
            _FadeOutObject.SetActive(false);
        }

        protected void Update()
        {
            _Status.text            = Global.Networking.Status;
            _StatusDescription.text = Global.Networking.StatusDescription;
        }

        protected void OnDestroy() {}

        #endregion

        #region Methods

        public void FadeIn()
        {
            _FadeInObject.SetActive(true);
            _FadeOutObject.SetActive(false);

            _ActiveFader = _FadeInObject;
        }

        public void FadeOut()
        {
            _FadeInObject.SetActive(false);
            _FadeOutObject.SetActive(true);

            _ActiveFader = _FadeOutObject;
        }

        #endregion
    }
}
