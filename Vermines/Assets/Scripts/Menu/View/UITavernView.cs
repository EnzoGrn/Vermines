using System.Threading.Tasks;
using UnityEngine;
using Fusion;

namespace Vermines.Menu.View {

    using Vermines.Menu.Tavern;
    using Vermines.UI.Core;
    using Vermines.UI;
    using Vermines.Core.Network;
    using Vermines.Core;
    using Vermines.Characters;

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

        public int PlayerCultist
        {
            get => Context.PlayerData.CultistID;
            set
            {
                Global.PlayerService.PlayerData.CultistID = value;
                Context.PlayerData.CultistID              = value;
            }
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

        protected override void OnOpen()
        {
            PlayerCultist = -1;

            base.OnOpen();
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

            base.OnClose();
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
            SessionRequest session = new() {
                GameMode = GameMode.AutoHostOrClient,
                GameplayType = GameplayType.Standart,
                MaxPlayers = 4,
                ScenePath = Context.MatchmakingScenePath
            };

            Context.Matchmaking.CreateSession(session, isCustom: false);
        }

        public void OnCultistSelected(Cultist cultist)
        {
            PlayerCultist = cultist.ID;
        }

        #endregion
    }
}
