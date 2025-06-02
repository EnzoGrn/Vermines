using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Vermines.UI.Animation {

    public class OpenCloseBookAnimation : MonoBehaviour {

        #region Sprites

        [Header("Sprites")]

        [SerializeField]
        private List<Sprite> _Frame;

        [SerializeField]
        private Image _ImageRef;

        int _FrameIndex = 0;

        #endregion

        #region Methods

        public void TriggerOpen()
        {
            if (_ImageRef == null)
                return;
            if (_Frame == null || _Frame.Count == 0)
                return;
            _ImageRef.sprite = _Frame[_FrameIndex];

            if (_FrameIndex < _Frame.Count - 1)
                _FrameIndex++;
        }

        public void TriggerClose()
        {
            if (_ImageRef == null)
                return;
            if (_Frame == null || _Frame.Count == 0)
                return;
            _ImageRef.sprite = _Frame[_FrameIndex];

            if (_FrameIndex > 0)
                _FrameIndex--;
        }

        #endregion
    }
}
