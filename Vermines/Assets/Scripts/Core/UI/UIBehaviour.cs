using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEditor.Animations;

namespace Vermines.Core.UI {

    public class UIBehaviour : CoreBehaviour {

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (!_CanvasGroupChecked) {
                    _CachedCanvasGroup  = GetComponent<CanvasGroup>();
                    _CanvasGroupChecked = true;
                }

                return _CachedCanvasGroup;
            }
        }

        public RectTransform RectTransform
        {
            get
            {
                if (!_RectTransformChecked) {
                    _CachedRectTransform  = transform as RectTransform;
                    _RectTransformChecked = true;
                }

                return _CachedRectTransform;
            }
        }

        public Image Image
        {
            get
            {
                if (!_ImageChecked) {
                    _CachedImage  = GetComponent<Image>();
                    _ImageChecked = true;
                }

                return _CachedImage;
            }
        }

        public Animation Animation
        {
            get
            {
                if (!_AnimationChecked) {
                    _CachedAnimation  = GetComponent<Animation>();
                    _AnimationChecked = true;
                }

                return _CachedAnimation;
            }
        }

        public Animator Animator
        {
            get
            {
                if (!_AnimatorChecked) {
                    _CachedAnimator  = GetComponent<Animator>();
                    _AnimatorChecked = true;
                }

                return _CachedAnimator;
            }
        }

        public TextMeshProUGUI Text
        {
            get
            {
                if (!_TextChecked) {
                    _CachedText  = GetComponent<TextMeshProUGUI>();
                    _TextChecked = true;
                }

                return _CachedText;
            }
        }

        private CanvasGroup _CachedCanvasGroup;
        private bool _CanvasGroupChecked;

        private RectTransform _CachedRectTransform;
        private bool _RectTransformChecked;

        private Image _CachedImage;
        private bool _ImageChecked;

        private Animation _CachedAnimation;
        private bool _AnimationChecked;

        private Animator _CachedAnimator;
        private bool _AnimatorChecked;

        private TextMeshProUGUI _CachedText;
        private bool _TextChecked;
    }
}
