using UnityEngine;

namespace Vermines.UI {

    /// <summary>
    /// Scrollbar drawn with the ornament: the track is the ornament at low opacity, the handle is a window
    /// (RectMask2D) showing the same ornament at full opacity. The fill copy is kept exactly on top of the track
    /// whatever the handle position, so the handle looks like the ornament lighting up.
    /// Fill and track share the same anchors / pivot setup (centre) and the same size.
    /// </summary>
    [ExecuteAlways]
    public class OrnamentScrollbar : MonoBehaviour {

        #region Attributes

        [Tooltip("The faint ornament (Image, may be rotated).")]
        [SerializeField] private RectTransform _Track;

        [Tooltip("Full opacity copy of the ornament, child of the handle (which carries a RectMask2D).")]
        [SerializeField] private RectTransform _Fill;

        #endregion

        #region MonoBehaviour Methods

        private void LateUpdate()
        {
            if (_Track == null || _Fill == null)
                return;
            _Fill.sizeDelta = _Track.rect.size;

            _Fill.SetPositionAndRotation(_Track.position, _Track.rotation);

            Vector3 trackScale  = _Track.lossyScale;
            Vector3 parentScale = _Fill.parent != null ? _Fill.parent.lossyScale : Vector3.one;

            _Fill.localScale = new Vector3(
                parentScale.x != 0f ? trackScale.x / parentScale.x : 1f,
                parentScale.y != 0f ? trackScale.y / parentScale.y : 1f,
                1f);
        }

        #endregion
    }
}
