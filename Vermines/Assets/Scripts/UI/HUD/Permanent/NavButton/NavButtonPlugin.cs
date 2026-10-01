using System;
using UnityEngine;

namespace Vermines.UI.Plugin
{
    /// <summary>
    /// Manages the navigation button plugin.
    /// </summary>
    public partial class NavButtonPlugin : GameplayScreenPlugin
    {
        private static CamManager _CachedCamManager;

        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);
        }

        public override void Hide()
        {
            base.Hide();
        }

        /// <summary>
        /// Is called when the <see cref="_CloseButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            if (EffectPromptState.BlockIfPending())
                return;

            _ParentScreen.Controller.Hide();

            if (TryGetCamManager(out CamManager camManager))
                camManager.GoOnNoneLocation();
        }

        #region Shared navigation helpers

        protected void TryNavigate(Action<CamManager> navigate)
        {
            if (EffectPromptState.BlockIfPending())
                return;

            _ParentScreen.Controller?.Hide();

            if (TryGetCamManager(out CamManager camManager))
                navigate(camManager);
        }

        protected bool TryGetCamManager(out CamManager camManager)
        {
            if (_CachedCamManager == null)
                _CachedCamManager = FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

            camManager = _CachedCamManager;

            if (camManager == null)
            {
                Debug.LogWarning("[NavButtonPlugin] CamManager not found in scene.");
                return false;
            }

            return true;
        }

        #endregion
    }
}
