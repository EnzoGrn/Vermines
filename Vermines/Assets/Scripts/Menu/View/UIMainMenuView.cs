using UnityEngine;
using TMPro;

namespace Vermines.Menu.View {

    using Vermines.Environment.Interaction.Button;
    using Vermines.Core.UI;

    public class UIMainMenuView : UIView {

        #region Attributes

        [SerializeField]
        private ButtonSignInteraction _PlayButton;

        [SerializeField]
        private ButtonSignInteraction _SettingsButton;

        [SerializeField]
        private ButtonSignInteraction _QuitButton;

        [SerializeField]
        private TextMeshProUGUI _ApplicationVersion;

        [SerializeField]
        private GameObject SignGameObject;

        #endregion

        #region Events

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _PlayButton.onClick.AddListener(OnPlayButton);
            _SettingsButton.onClick.AddListener(OnSettingsButton);
            _QuitButton.onClick.AddListener(OnQuitButton);

            _ApplicationVersion.text = $"Version ${Application.version}";
        }

        protected override void OnDeinitialize()
        {
            _PlayButton.onClick.RemoveListener(OnPlayButton);
            _SettingsButton.onClick.RemoveListener(OnSettingsButton);
            _QuitButton.onClick.RemoveListener(OnQuitButton);

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
            Debug.Log("Play");
            // TODO: Open Tavern
        }

        private void OnSettingsButton()
        {
            Debug.Log("Settings");
            // TODO: Open Settings
        }

        private void OnQuitButton()
        {
            Debug.Log("Quit");
            // TODO: Open Dialog and quit.
        }

        #endregion
    }
}
