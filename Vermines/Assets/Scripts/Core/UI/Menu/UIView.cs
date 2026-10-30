using UnityEngine;
using System;

namespace Vermines.Core.UI {

    [RequireComponent(typeof(CanvasGroup))]
    public class UIView : UIWidget, IBackHandler {

        #region Attributes

        public event Action HasOpened;
        public event Action HasClosed;

        public bool IsOpen { get; private set; }

        public bool IsInteractable
        {
            get => CanvasGroup.interactable;
            set => CanvasGroup.interactable = value;
        }

        public int Priority => _Priority;
        private int _Priority;

        public virtual bool NeedsCursor => _NeedsCursor;

        [SerializeField]
        private bool _NeedsCursor;

        [SerializeField]
        private bool _CanHandleBackAction;

        [SerializeField]
        private bool _UseSafeArea = true;

        private Rect _LastSafeArea;

        #endregion

        #region Methods

        public void Open()
        {
            SceneUI.Open(this);
        }

        public void Close()
        {
            if (SceneUI == null) {
                Debug.Log($"Closing view {gameObject.name} without SceneUI");

                Close_Internal();
            } else {
                SceneUI.Close(this);
            }
        }

        public void UpdateSafeArea()
        {
            if (!_UseSafeArea)
                return;
            Rect safeArea = Screen.safeArea;

            if (safeArea == _LastSafeArea)
                return;
            ApplySafeArea(safeArea);
        }

        public void Tick(float deltaTime) {}

        protected T Switch<T>() where T : UIView
        {
            Close();

            return SceneUI.Open<T>();
        }

        protected T Open<T>() where T : UIView
        {
            return SceneUI.Open<T>();
        }

        protected void Open(UIView view)
        {
            SceneUI.Open(view);
        }

        private void ApplySafeArea(Rect safeArea)
        {
            RectTransform rectTransform = transform as RectTransform;

            // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            int screenHeight = Screen.height;
            int screenWidth  = Screen.width;

            anchorMin.x /= screenWidth;
            anchorMax.x /= screenWidth;
            anchorMin.y /= screenHeight;
            anchorMax.y /= screenHeight;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;

            _LastSafeArea = safeArea;
        }

        #endregion

        #region Getters & Setters

        public void SetPriority(int priority)
        {
            _Priority = priority;
        }

        public void SetState(bool isOpen)
        {
            if (isOpen == true)
                Open();
            else
                Close();
        }

        public bool IsTopView(bool interactableOnly = false)
        {
            return SceneUI.IsTopView(this, interactableOnly);
        }

        #endregion

        #region Interface

        protected override void OnInitialize() {}

        protected override void OnDeinitialize()
        {
            Close_Internal();

            HasOpened = null;
            HasClosed = null;
        }

        int IBackHandler.Priority => _Priority;
        bool IBackHandler.IsActive => IsOpen == true && _CanHandleBackAction;

        bool IBackHandler.OnBackAction()
        {
            return OnBackAction();
        }

        protected virtual void OnOpen() {}
        protected virtual void OnClose() {}

        protected virtual bool OnBackAction()
        {
            if (IsInteractable)
                Close();
            return true;
        }

        #endregion

        #region Internal

        internal void Open_Internal()
        {
            if (IsOpen)
                return;
            IsOpen = true;

            gameObject.SetActive(true);

            OnOpen();

            if (HasOpened != null) {
                HasOpened.Invoke();

                HasOpened = null;
            }
        }

        internal void Close_Internal()
        {
            if (!IsOpen)
                return;
            IsOpen = false;

            OnClose();

            if (gameObject != null)
                gameObject.SetActive(false);
            if (HasClosed != null) {
                HasClosed.Invoke();

                HasClosed = null;
            }
        }

        #endregion
    }
}
