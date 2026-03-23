using System.Collections.Generic;
using Vermines.CardSystem.Elements;
using Vermines.Core.Player;
using Vermines.Gameplay.Phases.Data;
using Vermines.Player;

namespace Vermines.UI.Screen
{
    using Text = TMPro.TMP_Text;

    public class GameplayUIPlayerDetails : GameplayUIScreen, IParamReceiver<PlayerController>
    {
        PlayerController _player;

        #region Override Methods

        /// <summary>
        /// The Unity awake method.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// The screen init method.
        /// </summary>
        public override void Init()
        {
            base.Init();
        }

        /// <summary>
        /// The screen show method.
        /// </summary>
        public override void Show()
        {
            base.Show();
        }

        /// <summary>
        /// The screen hide method.
        /// </summary>
        public override void Hide()
        {
            base.Hide();
        }

        #endregion

        #region Methods

        public void SetParam(PlayerController param)
        {
            _player = param;
        }

        #endregion

        #region Events

        /// <summary>
        /// Is called when the <see cref="_CloseButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        protected virtual void OnCloseButtonPressed()
        {
            Controller.Hide();
        }

        #endregion

    }
}
