using System;
using TMPro;
using UnityEngine;

namespace Vermines.Environment.Interaction.Button {

    public class ButtonSignInteraction : MonoBehaviour {

        [SerializeField]
        private TextMeshPro _TextMesh;

        [SerializeField]
        private Color _NormalColor = Color.white;

        [SerializeField]
        private Color _HoverColor = Color.yellow;

        [HideInInspector]
        public Action OnClicked;

        private void Awake()
        {
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
            OnClicked?.Invoke();
        }
    }
}
