using UnityEngine;
using TMPro;

namespace Vermines.Environment.Interaction.Button {

    using Vermines.UI;

    public class ButtonSignInteraction : UIButton {

        [SerializeField]
        private TextMeshPro _TextMesh;

        [SerializeField]
        private Color _NormalColor = Color.white;

        [SerializeField]
        private Color _HoverColor = Color.yellow;

        new private void Awake()
        {
            _TextMesh = GetComponentInChildren<TextMeshPro>();

            if (_TextMesh != null)
                _TextMesh.color = _NormalColor;
        }

        private void OnMouseEnter()
        {
            if (_TextMesh != null)
                _TextMesh.color = _HoverColor;
        }

        private void OnMouseExit()
        {
            if (_TextMesh != null)
                _TextMesh.color = _NormalColor;
        }

        private void OnMouseDown()
        {
            onClick?.Invoke();
        }
    }
}
