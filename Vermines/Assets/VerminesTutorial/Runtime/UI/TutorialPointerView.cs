using UnityEngine;

namespace Vermines.Tutorial
{
    /// Main (ou flèche) animée qui montre quoi faire : tapoter une cible, ou glisser
    /// une carte de la main vers la zone partisan.
    /// L'image du prefab doit pointer vers le bas-gauche avec le pivot sur le bout du doigt.
    [RequireComponent(typeof(CanvasGroup))]
    public class TutorialPointerView : MonoBehaviour
    {
        [SerializeField] private RectTransform pointer;
        [SerializeField] private float tapAmplitude = 18f;
        [SerializeField] private float tapSpeed = 4f;
        [SerializeField] private float dragDuration = 1.2f;
        [SerializeField] private float dragPause = 0.4f;

        private CanvasGroup _group;
        private PointerMode _mode;
        private Vector2 _from;
        private Vector2 _to;
        private float _time;

        public void Configure(RectTransform pointer) => this.pointer = pointer;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;
            if (pointer == null)
                pointer = (RectTransform)transform;
            Hide();
        }

        public void Hide()
        {
            _mode = PointerMode.None;
            if (_group != null)
                _group.alpha = 0f;
        }

        /// from/to en coordonnées écran. to est ignoré en mode Tap.
        public void Show(PointerMode mode, Vector2 fromScreen, Vector2 toScreen, Camera canvasCamera)
        {
            if (mode != _mode)
                _time = 0f;
            _mode = mode;
            var parent = pointer.parent as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, fromScreen, canvasCamera, out _from);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, toScreen, canvasCamera, out _to);
        }

        private void Update()
        {
            if (_mode == PointerMode.None)
                return;
            _time += Time.unscaledDeltaTime;

            if (_mode == PointerMode.Tap)
            {
                float bob = (Mathf.Sin(_time * tapSpeed * Mathf.PI * 2f) * 0.5f + 0.5f) * tapAmplitude;
                pointer.localPosition = _from + new Vector2(bob * 0.5f, bob);
                _group.alpha = 1f;
                return;
            }

            // Glisser : aller de from à to, petite pause, disparition, recommencer.
            float cycle = dragDuration + dragPause * 2f;
            float t = _time % cycle;
            float move = Mathf.Clamp01((t - dragPause) / dragDuration);
            move = move * move * (3f - 2f * move);
            pointer.localPosition = Vector2.Lerp(_from, _to, move);
            float fadeIn = Mathf.Clamp01(t / (dragPause * 0.5f));
            float fadeOut = 1f - Mathf.Clamp01((t - dragPause - dragDuration) / (dragPause * 0.5f));
            _group.alpha = Mathf.Min(fadeIn, fadeOut);
        }
    }
}
