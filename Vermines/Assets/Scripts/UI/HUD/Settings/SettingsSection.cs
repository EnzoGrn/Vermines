using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines.UI {

    /// <summary>
    /// Foldable settings section: a header row (label + arrow) and a body whose height is animated.
    /// Body: LayoutElement + RectMask2D + CanvasGroup. Content: VerticalLayoutGroup + ContentSizeFitter (vertical: preferred),
    /// anchored top-stretch with pivot (0.5, 1), so the body clips it while it grows.
    /// </summary>
    public class SettingsSection : MonoBehaviour {

        #region Attributes

        [Header("Header")]
        [SerializeField] private UIButton _Header;

        [Tooltip("Optional. Pinned (highlight kept) while the section is expanded.")]
        [SerializeField] private ButtonJuice _HeaderJuice;

        [Tooltip("Optional. Rotated from 0 (\">\") to the open angle (\"v\").")]
        [SerializeField] private RectTransform _Arrow;
        [SerializeField] private float _ArrowOpenAngle = -90f;

        [Header("Body")]
        [SerializeField] private LayoutElement _Body;
        [SerializeField] private CanvasGroup _BodyGroup;
        [SerializeField] private RectTransform _Content;

        [Tooltip("Optional, on the content: rows cascade in when the section opens.")]
        [SerializeField] private StaggerReveal _Reveal;

        [Header("Feel")]
        [SerializeField] private float _Duration = 0.3f;

        [Header("Editor")]
        [Tooltip("Editor only: unfolds the body so the rows can be tuned without playing. "
               + "Ignored at runtime: Awake still folds the section.")]
        [SerializeField] private bool _PreviewExpanded;

        // Lets unticking fold the body back without touching a body that was unfolded by hand.
        [SerializeField, HideInInspector] private bool _PreviewApplied;

        #endregion

        #region Getters & Setters

        public bool IsExpanded { get; private set; }

        /// <summary>
        /// Raised before the section opens, so the owner can fold the other ones.
        /// </summary>
        public event Action<SettingsSection> Expanding;

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            _Header.onClick.AddListener(Toggle);

            SetExpanded(false, true);
        }

        private void OnDestroy()
        {
            if (_Header != null)
                _Header.onClick.RemoveListener(Toggle);
            DOTween.Kill(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;
            // Layout rebuilds and undo records are not allowed inside OnValidate.
            UnityEditor.EditorApplication.delayCall += ApplyPreview;
        }

        /// <summary>
        /// Editor only. Also available from the component's context menu after adding rows to the section.
        /// </summary>
        [ContextMenu("Refresh Preview")]
        private void ApplyPreview()
        {
            UnityEditor.EditorApplication.delayCall -= ApplyPreview;

            // Destroyed meanwhile, in play mode, or a prefab asset selected in the Project window (no scene to lay out).
            if (this == null || Application.isPlaying || !gameObject.scene.IsValid())
                return;
            if (_Body == null || _BodyGroup == null || _Content == null)
                return;
            if (!_PreviewExpanded && !_PreviewApplied)
                return;
            float height = 0f;

            if (_PreviewExpanded) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_Content);

                height = LayoutUtility.GetPreferredHeight(_Content);
            }

            float alpha = _PreviewExpanded ? 1f : 0f;
            Vector3 arrowAngle = new(0f, 0f, _PreviewExpanded ? _ArrowOpenAngle : 0f);

            UnityEditor.Undo.RecordObjects(
                _Arrow != null ? new UnityEngine.Object[] { this, _Body, _BodyGroup, _Arrow }
                               : new UnityEngine.Object[] { this, _Body, _BodyGroup },
                "Preview settings section");

            _PreviewApplied       = _PreviewExpanded;
            _Body.preferredHeight = height;
            _BodyGroup.alpha      = alpha;

            if (_Arrow != null)
                _Arrow.localEulerAngles = arrowAngle;
        }
#endif

        #endregion

        #region Methods

        public void Toggle()
        {
            SetExpanded(!IsExpanded, false);
        }

        public void SetExpanded(bool expanded, bool instant)
        {
            bool changed = IsExpanded != expanded;

            IsExpanded = expanded;

            if (expanded && changed)
                Expanding?.Invoke(this);
            DOTween.Kill(this);

            float height = 0f;

            if (expanded) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_Content);

                height = LayoutUtility.GetPreferredHeight(_Content);
            }

            _BodyGroup.interactable   = expanded;
            _BodyGroup.blocksRaycasts = expanded;

            if (_HeaderJuice != null)
                _HeaderJuice.SetPinned(expanded);
            Vector3 arrowAngle = new(0f, 0f, expanded ? _ArrowOpenAngle : 0f);

            if (instant || !isActiveAndEnabled) {
                _Body.preferredHeight = height;
                _BodyGroup.alpha      = expanded ? 1f : 0f;

                if (_Arrow != null)
                    _Arrow.localEulerAngles = arrowAngle;
                return;
            }

            DOTween.To(() => _Body.preferredHeight, h => _Body.preferredHeight = h, height, _Duration)
                .SetEase(expanded ? Ease.OutCubic : Ease.InOutCubic)
                .SetUpdate(true).SetTarget(this);
            _BodyGroup.DOFade(expanded ? 1f : 0f, expanded ? _Duration : _Duration * 0.6f)
                .SetUpdate(true).SetTarget(this);

            if (_Arrow != null)
                _Arrow.DOLocalRotate(arrowAngle, _Duration).SetEase(Ease.OutBack).SetUpdate(true).SetTarget(this);
            if (expanded && _Reveal != null)
                _Reveal.Play(_Duration * 0.3f);
        }

        #endregion
    }
}
