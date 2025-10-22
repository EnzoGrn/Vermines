using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using UnityEngine;

namespace Vermines.Core.Scene {

    using Vermines.Core.UI;
    using Vermines.Menu;

    [Flags]
    public enum CursorStateSource {
        None,
        UI,
        Menu,
        // Add more cursor state if needed...
    }

    public class SceneInput : SceneService {

        #region Attributes

        private List<IBackHandler> _BackHandlers = new();

        private CursorStateSource _CursorVisibilitySources;

        private bool _HasInput;

        #endregion

        #region Getters & Setters

        public bool IsCursorVisible => _CursorVisibilitySources != CursorStateSource.None;

        #endregion

        #region Methods

        public void RequestCursorVisibility(bool isVisible, CursorStateSource source, bool force = true)
        {
            if (source == CursorStateSource.None)
                return;
            CursorStateSource previousSources = _CursorVisibilitySources;

            if (isVisible)
                _CursorVisibilitySources = _CursorVisibilitySources | source;
            else
                _CursorVisibilitySources = _CursorVisibilitySources & ~source;

            if (_CursorVisibilitySources != previousSources || force)
                RefreshCursor();
        }

        public void ClearCursorLock()
        {
            _CursorVisibilitySources = CursorStateSource.None;

            RefreshCursor();
        }

        public void TrigggerBackAction()
        {
            BackAction();
        }

        private void BackAction()
        {
            if (_BackHandlers.Count == 0) {
                Context.UI.GetAll(_BackHandlers);

                _BackHandlers.Add(Context.UI);
                _BackHandlers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }

            foreach (IBackHandler handler in _BackHandlers) {
                if (handler.IsActive && handler.OnBackAction())
                    break;
            }
        }

        private void RefreshCursor()
        {
            if (!IsActive || (Context != null && !Context.HasInput))
                return;
            if (IsCursorVisible) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible   = true;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible   = false;
            }
        }

        #endregion

        #region Interface

        protected override void OnTick()
        {
            base.OnTick();

            if (Context.HasInput || Scene is Menu) {
                if (Keyboard.current.escapeKey.wasPressedThisFrame == true)
                    BackAction();
            }
        }

        #endregion
    }
}
