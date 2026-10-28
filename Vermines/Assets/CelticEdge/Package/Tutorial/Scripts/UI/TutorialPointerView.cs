using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// Animated hand (or arrow) that shows what to do:
    /// tap a target, or drag an object from one target to another (a card from your hand to the board...).
    /// 
    /// The prefab's image should point to the lower-left corner, with the pivot at the tip of the finger.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class TutorialPointerView : MonoBehaviour, ITutorialPointer {

        [SerializeField]
        private RectTransform _Pointer;

        [SerializeField]
        private float _TapAmplitude = 18f;

        [SerializeField]
        private float _TapSpeed = 4f;

        [SerializeField]
        private float _DragDuration = 1.2f;

        [SerializeField]
        private float _DragPause = 0.4f;

        private CanvasGroup _Group;
        private PointerMode _Mode;
        private Vector2     _From;
        private Vector2     _To;
        private float       _Time;

        public void Configure(RectTransform pointer) => _Pointer = pointer;

        private void Awake()
        {
            _Group = GetComponent<CanvasGroup>();

            _Group.blocksRaycasts = false;
            _Group.interactable   = false;

            if (_Pointer == null)
                _Pointer = (RectTransform)transform;
            Hide();
        }

        public void Hide()
        {
            _Mode = PointerMode.None;

            if (_Group != null)
                _Group.alpha = 0f;
        }

        /// <summary>
        /// from/to in screen coordinates.
        /// to is ignored in Tap mode.
        /// </summary>
        public void Show(PointerMode mode, Vector2 fromScreen, Vector2 toScreen, Camera canvasCamera)
        {
            if (mode != _Mode)
                _Time = 0f;
            _Mode = mode;

            var parent = _Pointer.parent as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, fromScreen, canvasCamera, out _From);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, toScreen  , canvasCamera, out _To);
        }

        private void Update()
        {
            if (_Mode == PointerMode.None)
                return;
            _Time += Time.unscaledDeltaTime;

            if (_Mode == PointerMode.Tap) {
                float bob = (Mathf.Sin(_Time * _TapSpeed * Mathf.PI * 2f) * 0.5f + 0.5f) * _TapAmplitude;

                _Pointer.localPosition = _From + new Vector2(bob * 0.5f, bob);
                _Group.alpha           = 1f;

                return;
            }

            // Slide: move from "from" to "to", pause briefly, disappear, then repeat.
            float cycle = _DragDuration + _DragPause * 2f;
            float t     = _Time % cycle;
            float move  = Mathf.Clamp01((t - _DragPause) / _DragDuration);

            move                   = move * move * (3f - 2f * move);
            _Pointer.localPosition = Vector2.Lerp(_From, _To, move);

            float fadeIn  = Mathf.Clamp01(t / (_DragPause * 0.5f));
            float fadeOut = 1f - Mathf.Clamp01((t - _DragPause - _DragDuration) / (_DragPause * 0.5f));

            _Group.alpha = Mathf.Min(fadeIn, fadeOut);
        }
    }
}
