using System;
using UnityEngine;

namespace Vermines.UI
{
    using Vermines.UI.Screen;

    public class NavBarWidget : MonoBehaviour
    {
        [SerializeField] private GameplayUIController _Controller;

        [Header("Juice (optional)")]
        [SerializeField] private NavButtonJuice _MarketJuice;
        [SerializeField] private NavButtonJuice _CourtyardJuice;
        [SerializeField] private NavButtonJuice _CityJuice;
        [SerializeField] private NavButtonJuice _TableJuice;
        [SerializeField] private NavButtonJuice _RecycleJuice;

        private CamManager _CamManager;
        private CamSplineType _Location = CamSplineType.None;
        private GameplayUIScreen _ActiveScreen;
        private float _NextCamLookup;

        private void Awake()
        {
            if (_Controller == null)
                _Controller = FindFirstObjectByType<GameplayUIController>(FindObjectsInactive.Include);

            if (_Controller == null)
                Debug.LogError("[NavBarWidget] GameplayUIController not found in the scene.", this);
        }

        private void Start()
        {
            RefreshSelection();
        }

        private void OnDestroy()
        {
            if (_CamManager != null)
                _CamManager.OnCamLocationIsChanging.RemoveListener(OnCamLocation);
        }

        private void Update()
        {
            // The camera may live in a scene loaded after this one: look for it quietly.
            if (_CamManager == null && Time.unscaledTime >= _NextCamLookup)
            {
                _NextCamLookup = Time.unscaledTime + 1f;
                _CamManager = FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

                if (_CamManager != null)
                    _CamManager.OnCamLocationIsChanging.AddListener(OnCamLocation);
            }

            if (_Controller == null)
                return;

            _Controller.GetActiveScreen(out GameplayUIScreen active);

            if (active == _ActiveScreen)
                return;

            _ActiveScreen = active;

            RefreshSelection();
        }

        public void OpenMarket() => Navigate(cam => cam.GoOnMarketLocation(), _MarketJuice);
        public void OpenCourtyard() => Navigate(cam => cam.GoOnCourtyardLocation(), _CourtyardJuice);
        public void GoBack() => Navigate(cam => cam.GoOnNoneLocation(), _CityJuice);
        public void OpenTable() => OpenScreen<GameplayUITable>(_TableJuice);
        public void OpenRecycle() => OpenScreen<GameplayUIRecycle>(_RecycleJuice);

        private void Navigate(Action<CamManager> move, NavButtonJuice juice)
        {
            if (_Controller == null)
                return;

            if (EffectPromptState.BlockIfPending())
            {
                if (juice != null)
                    juice.PlayRefused();

                return;
            }

            if (juice != null)
                juice.PlayClick();

            _Controller.Hide();

            if (TryGetCamManager(out CamManager camManager))
                move(camManager);
        }

        private void OpenScreen<T>(NavButtonJuice juice) where T : GameplayUIScreen
        {
            if (_Controller == null)
                return;

            _Controller.GetActiveScreen(out GameplayUIScreen last);

            if (last is T)
                return;

            if (juice != null)
                juice.PlayClick();

            _Controller.Show<T>(last);
        }

        private void OnCamLocation(CamSplineType location)
        {
            _Location = location;

            RefreshSelection();
        }

        private void RefreshSelection()
        {
            bool table = _ActiveScreen is GameplayUITable;
            bool recycle = _ActiveScreen is GameplayUIRecycle;
            bool screen = table || recycle;

            Select(_TableJuice, table);
            Select(_RecycleJuice, recycle);
            Select(_MarketJuice, !screen && (_Location == CamSplineType.MainViewToMarket || _Location == CamSplineType.CourtyardToMarket));
            Select(_CourtyardJuice, !screen && (_Location == CamSplineType.MainViewToCourtyard || _Location == CamSplineType.MarketToCourtyard));
            Select(_CityJuice, !screen && _Location == CamSplineType.None);
        }

        private static void Select(NavButtonJuice juice, bool selected)
        {
            if (juice != null)
                juice.SetSelected(selected);
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
