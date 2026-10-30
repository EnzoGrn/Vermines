using UnityEngine;

namespace Vermines.Core.UI {

    using Vermines.Core.Scene;
    using Vermines.Core.Audio;
    using Vermines.Extension;
    using System;
    using System.Collections.Generic;

    public class SceneUI : SceneService, IBackHandler {

        #region Attributes

        public Canvas Canvas { get; private set; }
        public Camera UICamera { get; private set; }

        [SerializeField]
        private AudioEffect[] _AudioEffects;

        [SerializeField]
        private AudioSetup _ClickSound;

        [SerializeField]
        private UIView[] _DefaultViews;
        protected UIView[] _Views;

        private ScreenOrientation _LastScreenOrientation;

        #endregion

        #region Interface

        int IBackHandler.Priority => -1;
        bool IBackHandler.IsActive => true;

        bool IBackHandler.OnBackAction()
        {
            return OnBackAction();
        }

        protected virtual void OnInitializeInternal() {}
        protected virtual void OnDeinitializeInternal() {}
        protected virtual void OnTickInternal() {}

        protected virtual bool OnBackAction()
        {
            return false;
        }

        protected virtual void OnViewOpened(UIView view) {}
        protected virtual void OnViewClosed(UIView view) {}

        protected override sealed void OnInitialize()
        {
            Canvas   = GetComponent<Canvas>();
            UICamera = Canvas.worldCamera;
            _Views   = GetComponentsInChildren<UIView>(true);

            for (int i = 0; i < _Views.Length; i++) {
                UIView view = _Views[i];

                view.Initialize(this, null);
                view.SetPriority(i);

                view.gameObject.SetActive(false);
            }
            OnInitializeInternal();
            UpdateScreenOrientation();
        }

        protected override sealed void OnDeinitialize()
        {
            OnDeinitializeInternal();

            if (_Views != null) {
                for (int i = 0; i < _Views.Length; i++)
                    _Views[i].Deinitialize();
                _Views = null;
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            Canvas.enabled = true;

            int count = _DefaultViews != null ? _DefaultViews.Length : 0;

            for (int i = 0; i < count; i++)
                Open(_DefaultViews[i]);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();

            int count = _Views != null ? _Views.Length : 0;

            for (int i = 0; i < count; i++)
                Close(_Views[i]);
            if (Canvas != null)
                Canvas.enabled = false;
        }

        protected override sealed void OnTick()
        {
            UpdateScreenOrientation();

            if (_Views != null) {
                for (int i = 0; i < _Views.Length; i++) {
                    UIView view = _Views[i];

                    if (view.IsOpen == true)
                        view.Tick();
                }
            }
            OnTickInternal();
        }

        #endregion

        #region Methods

        public bool PlaySound(AudioSetup setup, ForceBehaviour force = ForceBehaviour.None)
        {
            return _AudioEffects.PlaySound(setup, force);
        }

        public bool PlayClickSound()
        {
            return PlaySound(_ClickSound);
        }

        public T Get<T>() where T : UIView
        {
            if (_Views == null)
                return null;
            for (int i = 0; i < _Views.Length; i++) {
                T view = _Views[i] as T;

                if (view != null)
                    return view;
            }
            return null;
        }

        public T Open<T>() where T : UIView
        {
            if (_Views == null)
                return null;
            for (int i = 0; i < _Views.Length; i++) {
                T view = _Views[i] as T;

                if (view != null) {
                    OpenView(view);

                    return view;
                }
            }

            return null;
        }

        public void Open(UIView view)
        {
            if (_Views == null)
                return;
            int index = Array.IndexOf(_Views, view);

            if (index < 0) {
                Debug.LogError($"Cannot find view {view.name}");

                return;
            }

            OpenView(view);
        }

        public T Close<T>() where T : UIView
        {
            if (_Views == null)
                return null;
            for (int i = 0; i < _Views.Length; i++) {
                T view = _Views[i] as T;

                if (view != null) {
                    view.Close();

                    return view;
                }
            }
            return null;
        }

        public void Close(UIView view)
        {
            if (_Views == null)
                return;
            int index = Array.IndexOf(_Views, view);

            if (index < 0) {
                Debug.LogError($"Cannot find view {view.name}");

                return;
            }
            CloseView(view);
        }

        public T Toggle<T>() where T : UIView
        {
            if (_Views == null)
                return null;
            for (int i = 0; i < _Views.Length; i++) {
                T view = _Views[i] as T;

                if (view != null) {
                    if (view.IsOpen == true)
                        CloseView(view);
                    else
                        OpenView(view);
                    return view;
                }
            }
            return null;
        }

        public bool IsOpen<T>() where T : UIView
        {
            if (_Views == null)
                return false;
            for (int i = 0; i < _Views.Length; i++) {
                T view = _Views[i] as T;

                if (view != null)
                    return view.IsOpen;
            }
            return false;
        }

        public bool IsTopView(UIView view, bool interactableOnly = false)
        {
            if (!view.IsOpen || _Views == null)
                return false;
            int highestPriority = -1;

            for (int i = 0; i < _Views.Length; i++) {
                var otherView = _Views[i];

                if (otherView == view || !otherView.IsOpen || (interactableOnly && !otherView.IsInteractable))
                    continue;
                highestPriority = Math.Max(highestPriority, otherView.Priority);
            }
            return view.Priority > highestPriority;
        }

        public void CloseAll()
        {
            if (_Views == null)
                return;
            for (int i = 0; i < _Views.Length; i++)
                CloseView(_Views[i]);
        }

        public void GetAll<T>(List<T> list)
        {
            if (_Views == null)
                return;
            for (int i = 0; i < _Views.Length; i++) {
                if (_Views[i] is T element)
                    list.Add(element);
            }
        }

        private void UpdateScreenOrientation()
        {
            if (_LastScreenOrientation == Screen.orientation)
                return;
            if (_Views != null) {
                for (int i = 0; i < _Views.Length; i++)
                    _Views[i].UpdateSafeArea();
                _LastScreenOrientation = Screen.orientation;
            }
        }

        private void OpenView(UIView view)
        {
            if (view == null || view.IsOpen)
                return;
            view.Open_Internal();

            OnViewOpened(view);
        }

        private void CloseView(UIView view)
        {
            if (view == null || !view.IsOpen)
                return;
            view.Close_Internal();

            OnViewClosed(view);
        }

        #endregion
    }
}
