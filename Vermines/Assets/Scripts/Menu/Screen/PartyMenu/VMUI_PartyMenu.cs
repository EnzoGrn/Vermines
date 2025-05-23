using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using OMGG.Menu.Screen;
using OMGG.Menu.Region;
using UnityEngine;
using Fusion;

namespace Vermines.Menu.Screen {

    using InputField = TMPro.TMP_InputField;
    using Button     = UnityEngine.UI.Button;

    /// <summary>
    /// Vermines Party Menu UI partial class.
    /// Extends of the <see cref="MenuUIScreen" /> from OMGG Menu package.
    /// </summary>
    public partial class VMUI_PartyMenu : MenuUIScreen {

        #region Fields

        /// <summary>
        /// The session code input field.
        /// </summary>
        [InlineHelp, SerializeField]
        protected InputField _SessionCodeField;

        /// <summary>
        /// The create game button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _CreateButton;

        /// <summary>
        /// The join game button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _JoinButton;

        /// <summary>
        /// The back button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Button _BackButton;

        /// <summary>
        /// The task of requesting the regions.
        /// </summary>
        protected Task<List<OnlineRegion>> _RegionRequest;

        #endregion

        #region Partial Methods

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #endregion

        #region Overrides Methods

        /// <summary>
        /// The Unity awake method.
        /// Calls partial method <see cref="AwakeUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            AwakeUser();
        }

        /// <summary>
        /// The screen init method.
        /// Calls partial method <see cref="InitUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Init()
        {
            base.Init();

            InitUser();
        }

        /// <summary>
        /// The screen show method.
        /// Calls partial method <see cref="ShowUser"/> to be implemented on the SDK side.
        /// When entering this screen an async request to retrieve the available regions is started.
        /// </summary>
        public override void Show()
        {
            base.Show();

            if (Config.CodeGenerator == null)
                Debug.LogError("Add a CodeGenerator to the Server config");
            _SessionCodeField.SetTextWithoutNotify("".PadLeft(Config.CodeGenerator.Length, '-'));

            _SessionCodeField.characterLimit = Config.CodeGenerator.Length;

            if (_RegionRequest == null || _RegionRequest.IsFaulted)
                _RegionRequest = Connection.RequestAvailableOnlineRegionsAsync(ConnectionArgs);
            ShowUser();
        }

        /// <summary>
        /// The screen hide method.
        /// Calls partial method <see cref="HideUser"/> to be implemented on the SDK side.
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            HideUser();
        }

        /// <summary>
        /// The connect method to handle create and join.
        /// Internally the region request is awaited.
        /// </summary>
        /// <param name="creating">Create or join</param>
        /// <returns></returns>
        protected virtual async Task ConnectAsync(bool creating)
        {
            var inputRegionCode = _SessionCodeField.text.ToUpper();

            if (creating == false && Config.CodeGenerator.IsValid(inputRegionCode) == false) {
                await Controller.PopupAsync($"The session code '{inputRegionCode}' is not a valid session code. Please enter {Config.CodeGenerator.Length} characters or digits.", "Invalid Session Code");

                return;
            }

            if (_RegionRequest.IsCompleted == false) {
                Controller.Show<VMUI_Loading>(this);
                Controller.Get<VMUI_Loading>().SetStatusText("Fetching Regions");

                try {
                    await _RegionRequest;
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }

            if (_RegionRequest.IsCompletedSuccessfully == false && _RegionRequest.Result.Count == 0) {
                await Controller.PopupAsync($"Failed to request regions.", "Connection Failed");

                Controller.Show<VMUI_MainMenu>(this);

                return;
            }

            if (creating) {
                var regionIndex = -1;

                if (string.IsNullOrEmpty(ConnectionArgs.PreferredRegion))
                    regionIndex = Region.FindBestAvailableOnlineRegionIndex(_RegionRequest.Result);
                else
                    regionIndex = _RegionRequest.Result.FindIndex(r => r.Code == ConnectionArgs.PreferredRegion);
                if (regionIndex == -1) {
                    await Controller.PopupAsync($"Selected region is not available.", "Connection Failed");

                    Controller.Show<VMUI_MainMenu>(this);

                    return;
                }

                ConnectionArgs.Session = Config.CodeGenerator.EncodeRegion(Config.CodeGenerator.Create(), regionIndex);
                ConnectionArgs.Region = _RegionRequest.Result[regionIndex].Code;
            } else {
                int regionIndex = Config.CodeGenerator.DecodeRegion(inputRegionCode);

                Debug.LogError($"ConnectAsync: {inputRegionCode} - {creating}");
                Debug.LogError(regionIndex);

                if (regionIndex < 0 || regionIndex > Config.AvailableRegions.Count) {
                    await Controller.PopupAsync($"The session code '{inputRegionCode}' is not a valid session code (cannot decode the region).", "Invalid Session Code");

                    return;
                }

                ConnectionArgs.Session = _SessionCodeField.text.ToUpper();
                ConnectionArgs.Region  = Config.AvailableRegions[regionIndex];
            }

            ConnectionArgs.Creating = creating;

            Controller.Show<VMUI_Loading>(this);

            var result = await Connection.ConnectAsync(ConnectionArgs);

            await Controller.HandleConnectionResult(result, Controller);
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_CreateButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual async void OnCreateButtonPressed()
        {
            await ConnectAsync(true);
        }

        /// <summary>
        /// Is called when the <see cref="_JoinButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual async void OnJoinButtonPressed()
        {
            await ConnectAsync(false);
        }

        /// <summary>
        /// Is called when the <see cref="_BackButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed()
        {
            Controller.ShowLast();
        }

        #endregion
    }
}
