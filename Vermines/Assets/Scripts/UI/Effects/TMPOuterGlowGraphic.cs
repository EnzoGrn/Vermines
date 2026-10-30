using UnityEngine.UI;
using UnityEngine;

namespace Vermines.UI.Effects {

    /// <summary>
    /// Hidden graphic drawn right behind a text that has a <see cref="TMPOuterGlow"/>.
    /// It shows the blurred text mask over the area covered by the glow, the shading is done by its material.
    /// </summary>
    [AddComponentMenu("")]
    public class TMPOuterGlowGraphic : MaskableGraphic {

        #region Attributes

        /// <summary>
        /// Blurred text mask rendered by the owning <see cref="TMPOuterGlow"/>.
        /// </summary>
        private Texture _MaskTexture;

        /// <summary>
        /// Area covered by the glow, in the local space of the text.
        /// </summary>
        private Rect _GlowRect;

        #endregion

        #region Properties

        public override Texture mainTexture => _MaskTexture != null ? _MaskTexture : s_WhiteTexture;

        #endregion

        #region Methods

        /// <summary>
        /// Updates the mask texture and the area it covers.
        /// </summary>
        /// <param name="maskTexture">Blurred text mask, or null to draw nothing.</param>
        /// <param name="glowRect">Area covered by the mask, in the local space of the text.</param>
        public void SetGlow(Texture maskTexture, Rect glowRect)
        {
            if (_MaskTexture != maskTexture) {
                _MaskTexture = maskTexture;

                SetMaterialDirty();
                SetVerticesDirty();
            }

            if (_GlowRect != glowRect) {
                _GlowRect = glowRect;

                SetVerticesDirty();
            }
        }

        #endregion

        #region Graphic Methods

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (_MaskTexture == null || _GlowRect.width <= 0f || _GlowRect.height <= 0f)
                return;
            Color32 vertexColor = color;

            vh.AddVert(new Vector3(_GlowRect.xMin, _GlowRect.yMin), vertexColor, new Vector4(0f, 0f));
            vh.AddVert(new Vector3(_GlowRect.xMin, _GlowRect.yMax), vertexColor, new Vector4(0f, 1f));
            vh.AddVert(new Vector3(_GlowRect.xMax, _GlowRect.yMax), vertexColor, new Vector4(1f, 1f));
            vh.AddVert(new Vector3(_GlowRect.xMax, _GlowRect.yMin), vertexColor, new Vector4(1f, 0f));
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        #endregion
    }
}
