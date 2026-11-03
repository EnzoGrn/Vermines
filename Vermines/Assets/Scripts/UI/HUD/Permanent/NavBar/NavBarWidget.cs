using System;
using UnityEngine;

namespace Vermines.UI
{
    using Vermines.UI.Screen;

    /// <summary>
    /// Permanent HUD widget: the navigation bar (market, courtyard, table, recycle, back).
    /// Put it on an ALWAYS ACTIVE object. It does not depend on any screen lifecycle.
    /// Wire each button's onClick to one of the public methods.
    /// </summary>
    public class NavBarWidget : MonoBehaviour
    {
        [SerializeField] private GameplayUIController _Controller;

        private CamManager _CamManager;

        private void Awake()
        {
            if (_Controller == null)
                _Controller = FindFirstObjectByType<GameplayUIController>(FindObjectsInactive.Include);

            if (_Controller == null)
                Debug.LogError("[NavBarWidget] GameplayUIController not found in the scene.", this);
        }

        public void OpenMarket() => Navigate(cam => cam.GoOnMarketLocation());
        public void OpenCourtyard() => Navigate(cam => cam.GoOnCourtyardLocation());
        public void GoBack() => Navigate(cam => cam.GoOnNoneLocation());
        public void OpenTable() => OpenScreen<GameplayUITable>();
        public void OpenRecycle() => OpenScreen<GameplayUIRecycle>();

        private void Navigate(Action<CamManager> move)
        {
            if (_Controller == null || EffectPromptState.BlockIfPending())
                return;

            _Controller.Hide();

            if (TryGetCamManager(out CamManager camManager))
                move(camManager);
        }

        private void OpenScreen<T>() where T : GameplayUIScreen
        {
            if (_Controller == null)
                return;

            _Controller.GetActiveScreen(out GameplayUIScreen last);

            if (last is T)
                return;

            _Controller.Show<T>(last);
        }

        private bool TryGetCamManager(out CamManager camManager)
        {
            if (_CamManager == null)
                _CamManager = FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

            camManager = _CamManager;

            if (camManager == null)
            {
                Debug.LogWarning("[NavBarWidget] CamManager not found in scene.", this);

                return false;
            }

            return true;
        }
    }
}
