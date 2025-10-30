using UnityEngine;

namespace Vermines.Menu.View {

    using Vermines.Environment.Interaction.Button;
    using Vermines.Core.UI;
    using Vermines.UI.Dialog;
    using Vermines.Extension;
    using Vermines.UI;

    public class UIMainMenuView : UIView {

        #region Attributes

        [SerializeField]
        private ButtonSignInteraction _PlayButton;

        [SerializeField]
        private ButtonSignInteraction _SettingsButton;

        [SerializeField]
        private ButtonSignInteraction _QuitButton;

        [SerializeField]
        private GameObject SignGameObject;

        #endregion

        #region Methods

        public void ActiveButton()
        {
            _PlayButton.onClick.AddListener(OnPlayButton);
            _SettingsButton.onClick.AddListener(OnSettingsButton);
            _QuitButton.onClick.AddListener(OnQuitButton);
        }

        public void DeactiveButton()
        {
            _PlayButton.onClick.RemoveListener(OnPlayButton);
            _SettingsButton.onClick.RemoveListener(OnSettingsButton);
            _QuitButton.onClick.RemoveListener(OnQuitButton);
        }

        private void InTavern()
        {
            MainMenuCamera camera = FindFirstObjectByType<MainMenuCamera>();

            if (camera) {
                camera.OnSplineEnd.RemoveListener(() => InTavern());
                camera.OnKnotPassed.RemoveListener((knotIndex) => OnKnotPassed(knotIndex));
            }

            Open<UITavernView>();
        }

        private void OnKnotPassed(int knotIndex)
        {
            MainMenuCamera camera = FindFirstObjectByType<MainMenuCamera>();

            if (camera) {
                if (camera.GetKnotsCount() / 2 == knotIndex)
                    camera.SetEndLookAt();
            }
        }

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            ActiveButton();
        }

        protected override void OnDeinitialize()
        {
            DeactiveButton();
            
            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            SignGameObject.SetActive(true);
        }

        protected override void OnClose()
        {
            SignGameObject.SetActive(false);

            base.OnClose();
        }

        protected override bool OnBackAction()
        {
            if (!IsInteractable)
                return false;
            OnQuitButton();

            return true;
        }

        private void OnPlayButton()
        {
            MainMenuCamera camera = FindFirstObjectByType<MainMenuCamera>();

            if (camera) {
                camera.OnSplineEnd.AddListener(() => InTavern());
                camera.OnKnotPassed.AddListener((knotIndex) => OnKnotPassed(knotIndex));

                camera.OnSplineStarted();
            } else {
                Debug.LogWarning($"[UIMainMenuView]: No CinemachineCamera found in the scene. The spline will not be played. Automatically open the 'UITavernView'.");

                InTavern();
            }
        }

        private void OnSettingsButton()
        {
            Open<UISettingsView>();
        }

        private void OnQuitButton()
        {
            UIYesNoDialog dialog = Open<UIYesNoDialog>();

            dialog.Title.SetTextSafe("EXIT GAME");
            dialog.Description.SetTextSafe("Are you sure you want to exit the game?");

            dialog.YesButtonText.SetTextSafe("EXIT");
            dialog.NoButtonText.SetTextSafe("CANCEL");

            dialog.HasClosed += (result) => {
                if (result)
                    SceneUI.Scene.Quit();
            };
        }

        #endregion
    }
}
