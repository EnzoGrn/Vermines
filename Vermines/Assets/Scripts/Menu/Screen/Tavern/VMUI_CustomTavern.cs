using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using OMGG.Menu.Connection;
using OMGG.Menu.Screen;
using Fusion;
using TMPro;

namespace Vermines.Menu.Screen {

    /// <summary>
    /// Vermines Custom Tarvern UI partial class (Custom Game UI).
    /// Extends of the <see cref="MenuUIScreen" /> from OMGG Menu package.
    /// </summary>
    public partial class VMUI_CustomTavern : MenuUIScreen {

        [Tooltip("The scene reference to load when the game is ready. This scene will be loaded when the game is ready to start.")]
        public SceneRef _SceneToLoadWhenReady;

        #region Fields

        [SerializeField, InlineHelp]
        private TMP_Text _CodeText;

        [SerializeField, InlineHelp]
        private GameObject _SessionGameObject;

        [SerializeField, InlineHelp]
        private Button _CopySessionButton;

        #endregion

        #region Methods

        private async void UnloadScene()
        {
            // Get the party menu instance, because it's the screen that have the reference of the custom lobby scene.
            VMUI_PartyMenu party = FindAnyObjectByType<VMUI_PartyMenu>(FindObjectsInactive.Include);

            if (party) {
                SceneRef lobby = party.SceneRef;

                if (lobby.IsValid) {
                    Scene scene = SceneManager.GetSceneByBuildIndex(lobby.AsIndex);

                    if (scene.IsValid() && scene.isLoaded)
                        await SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        #endregion

        #region Overrides Methods

        public override void Show()
        {
            base.Show();

            // Only show the session UI if it is a party code
            if (Config.CodeGenerator != null && Config.CodeGenerator.IsValid(Connection.SessionName)) {
                _CodeText.text = Connection.SessionName;

                _SessionGameObject.SetActive(true);
            } else {
                _CodeText.text = string.Empty;

                _SessionGameObject.SetActive(false);
            }
        }

        #endregion

        #region Events

        public virtual async void OnLeaveButtonPressed()
        {
            await Connection.DisconnectAsync(ConnectFailReason.UserRequest);
            
            UnloadScene();

            Controller.Show<VMUI_Tavern>(this);
        }

        protected virtual void OnSettingsButtonPressed()
        {
            Controller.Show<VMUI_Settings>(this);
        }

        /// <summary>
        /// Is called when the <see cref="_CopySessionButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnCopySessionPressed()
        {
            GUIUtility.systemCopyBuffer = _CodeText.text;
        }

        #endregion
    }
}
