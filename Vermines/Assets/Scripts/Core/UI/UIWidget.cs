using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Core.UI {

    using Vermines.Core.Audio;
    using Vermines.Core.Scene;

    public abstract class UIWidget : UIBehaviour {

        #region Attributes

        public bool IsVisible { get; private set; }

        protected bool IsInitialized { get; private set; }
        protected SceneUI SceneUI { get; private set; }
        protected SceneContext Context
        {
            get => SceneUI.Context;
        }
        protected UIWidget Owner { get; private set; }

        private List<UIWidget> _Children = new(16);

        #endregion

        #region MonoBehaviour Methods

        protected void OnEnable()
        {
            Visible();
        }

        protected void OnDisable()
        {
            Hidden();
        }

        #endregion

        #region Methods

        public void PlayClickSound()
        {
            if (SceneUI != null)
                SceneUI.PlayClickSound();
        }

        public void PlaySound(AudioSetup setup, ForceBehaviour force = ForceBehaviour.None)
        {
            if (setup == null) {
                Debug.LogWarning($"Missing click sound, parent {name}");

                return;
            }

            if (SceneUI != null)
                SceneUI.PlaySound(setup, force);
        }

        #endregion

        #region Internal

        internal void Initialize(SceneUI sceneUI, UIWidget owner)
        {
            if (IsInitialized)
                return;
            SceneUI = sceneUI;
            Owner   = owner;

            _Children.Clear();

            GetChildWidgets(transform, _Children);

            for (int i = 0; i < _Children.Count; i++)
                _Children[i].Initialize(sceneUI, this);
            OnInitialize();

            IsInitialized = true;

            if (gameObject.activeInHierarchy)
                Visible();
        }

        internal void Deinitialize()
        {
            if (!IsInitialized)
                return;
            Hidden();
            OnDeinitialize();

            for (int i = 0; i < _Children.Count; i++)
                _Children[i].Deinitialize();
            _Children.Clear();

            IsInitialized = false;
            SceneUI       = null;
            Owner         = null;
        }

        internal void Visible()
        {
            if (!IsInitialized || IsVisible || !gameObject.activeSelf)
                return;
            IsVisible = true;

            for (int i = 0; i < _Children.Count; i++)
                _Children[i].Visible();
            OnVisible();
        }

        internal void Hidden()
        {
            if (!IsVisible)
                return;
            IsVisible = false;

            OnHidden();

            for (int i = 0; i < _Children.Count; i++)
                _Children[i].Hidden();
        }

        internal void Tick()
        {
            if (!IsInitialized || !IsVisible)
                return;
            OnTick();

            for (int i = 0; i < _Children.Count; i++)
                _Children[i].Tick();
        }

        internal void AddChild(UIWidget widget)
        {
            if (widget == null || widget == this)
                return;
            if (_Children.Contains(widget) == true) {
                Debug.LogError($"Widget {widget.name} is already added as child of {name}");

                return;
            }
            _Children.Add(widget);

            widget.Initialize(SceneUI, this);
        }

        internal void RemoveChild(UIWidget widget)
        {
            int childIndex = _Children.IndexOf(widget);

            if (childIndex < 0) {
                Debug.LogError($"Widget {widget.name} is not child of {name} and cannot be removed");

                return;
            }
            widget.Deinitialize();

            _Children.RemoveAt(childIndex);
        }

        #endregion

        #region Interface

        public virtual bool IsActive()
        {
            return true;
        }

        protected virtual void OnInitialize() {}
        protected virtual void OnDeinitialize() {}
        protected virtual void OnVisible() {}
        protected virtual void OnHidden() {}
        protected virtual void OnTick() {}

        #endregion

        #region Utils

        private static void GetChildWidgets(Transform transform, List<UIWidget> widgets)
        {
            foreach (Transform child in transform) {
                UIWidget childWidget = child.GetComponent<UIWidget>();

                if (childWidget != null)
                    widgets.Add(childWidget);
                else
                    GetChildWidgets(child, widgets);
            }
        }

        #endregion
    }
}
