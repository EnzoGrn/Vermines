using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using Vermines.CardSystem.Enumerations;
using Vermines.Gameplay.Phases;
using Vermines.Player;
using Vermines.CardSystem.Elements;

namespace Vermines.UI.Screen
{
    using Text = TMPro.TMP_Text;

    public partial class GameplayUITurn : GameplayUIScreen
    {
        #region Attributes

        [Header("Navigation Buttons")]

        /// <summary>
        /// The turn button.
        /// </summary>
        [InlineHelp, SerializeField]
        protected UnityEngine.UI.Button _CloseButton;

        [Header("Message Source")]

        /// <summary>
        /// The config of the summary message.
        /// </summary>
        [InlineHelp, SerializeField]
        private CardFamily _raceName;

        [Header("Message Text")]

        /// <summary>
        /// The text component of the message.
        /// </summary>
        [InlineHelp, SerializeField]
        protected Text _MessageText;

        #endregion

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #region Override Methods

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
        /// Will check is the session code is compatible with the party code to toggle the session UI part.
        /// </summary>
        public override void Show()
        {
            base.Show();

            ShowUser();

            LoadAndAnnounce();
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

        #endregion

        #region Methods

        public void LoadAndAnnounce()
        {
            if (_raceName == CardFamily.None)
            {
                foreach (var player in GameDataStorage.Instance.PlayerData)
                {
                    Debug.Log(player);
                    if (player.Key == PlayerController.Local.PlayerRef)
                    {
                        PlayerData playerData = player.Value;
                        _raceName = playerData.Family;
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_CloseButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnClose()
        {
            Controller.Hide();
        }

        #endregion
    }
}
