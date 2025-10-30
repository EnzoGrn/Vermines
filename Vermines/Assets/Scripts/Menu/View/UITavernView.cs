using System.Threading.Tasks;
using UnityEngine;

namespace Vermines.Menu.View {

    using Vermines.Menu.Tavern;
    using Vermines.UI.Core;
    using Vermines.UI;
    using Vermines.Characters;
    using WebSocketSharp;

    public class UITavernView : UICloseView {

        #region Attributes

        [SerializeField]
        private UIButton _SettingsButton;

        [SerializeField]
        private UIButton _PartyButton;

        public UIButton PlayButton => _PlayButton;

        [SerializeField]
        private UIButton _PlayButton;

        [SerializeField]
        private CultistSelectDisplay _CultistSelectedDisplay;

        #endregion

        #region Getters & Setters

        public Cultist PlayerCultist
        {
            get => Context.PlayerCultist;
            set => Context.PlayerCultist = value;
        }

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _CultistSelectedDisplay = GetComponentInChildren<CultistSelectDisplay>(true);

            _CultistSelectedDisplay.Initialize(this);
            _SettingsButton.onClick.AddListener(OnSettingsButton);
            _PartyButton.onClick.AddListener(OnPartyButton);
            _PlayButton.onClick.AddListener(OnQuickPlayButton);
        }

        protected override void OnDeinitialize()
        {
            _CultistSelectedDisplay.Deinitialize();

            _SettingsButton.onClick.RemoveListener(OnSettingsButton);
            _PartyButton.onClick.RemoveListener(OnPartyButton);
            _PlayButton.onClick.RemoveListener(OnQuickPlayButton);

            base.OnDeinitialize();
        }

        protected override async void OnClose()
        {
            _CultistSelectedDisplay.OnClose();

            MainMenuCamera camera = FindFirstObjectByType<MainMenuCamera>();

            Open<UIMainMenuView>();

            if (camera)
                await OnCameraCloseAsync(camera);
        }

        private async Task OnCameraCloseAsync(MainMenuCamera camera)
        {
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

            float duration = stateInfo.length;

            await Task.Delay((int)(duration * 1000));

            camera.OnSplineReseted();
        }

        private void OnSettingsButton()
        {
            Open<UISettingsView>();
        }

        private void OnPartyButton()
        {
            Open<UIPartyMenuView>();
        }

        private void OnQuickPlayButton()
        {
            if (!Context.PlayerData.UnityID.IsNullOrEmpty()) {

            }
            /*
             CultistSelectDisplay cultistSelectDisplay = FindFirstObjectByType<CultistSelectDisplay>();

            //if (cultistSelectDisplay)
            //    SelectedCultist = cultistSelectDisplay.GetSelectedCultist();
            ConnectionArgs.Session  = null;
            ConnectionArgs.Creating = false;
            ConnectionArgs.Region   = ConnectionArgs.PreferredRegion;

            Controller.Show<VMUI_Loading>(this);

            var result = await Connection.ConnectAsync(ConnectionArgs, SceneRef);

            await Controller.HandleConnectionResult(result, Controller);
             */
        }

        #endregion
    }
}
