using Fusion;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Vermines.CardSystem.Enumerations;
using Vermines.Core.Player;
using Vermines.Core.Scene;
using Vermines.Player;
using Vermines.UI.Utils;

namespace Vermines.UI.Screen
{
    using Text = TMPro.TMP_Text;

    public partial class GameplayUITurn : GameplayUIScreen
    {
        #region Attributes

        [Header("Message Text")]
        [InlineHelp, SerializeField]
        protected Text _MessageText;

        [Header("Character")]
        [InlineHelp, SerializeField]
        private Image _CultistImage;

        #endregion

        partial void AwakeUser();
        partial void InitUser();
        partial void ShowUser();
        partial void HideUser();

        #region Override Methods

        public override void Awake()
        {
            base.Awake();
            AwakeUser();
        }

        public override void Init()
        {
            base.Init();
            InitUser();
        }

        public override void Show()
        {
            // FIX: was reading PlayerController.Local.Context with no guard.
            // This screen is shown from PhaseManager.SacrificeRoutine() - an
            // unhandled exception here would kill that ENTIRE coroutine
            // before it reaches its RPC_ProcessPhase(...) call at the end,
            // not just fail silently. See header comment for the full chain.
            if (!PlayerController.Local)
            {
                Debug.LogWarning("[GameplayUITurn] Show() called before PlayerController.Local was ready - skipping this turn banner.");

                base.Show();
                ShowUser();

                return;
            }

            SceneContext context = PlayerController.Local.Context;
            PlayerRef currentPlayer = context.GameplayMode.CurrentPlayer;
            PlayerController player = context.NetworkGame.GetPlayer(currentPlayer);
            PlayerStatistics stat = player.Statistics;

            _CultistImage.sprite = UISpriteLoader.GetDefaultSprite(CardType.Partisan, stat.Family, "Cultist");

            if (currentPlayer == context.Runner.LocalPlayer)
                LocalizeMessage("Turn.your");
            else
                LocalizeMessage("Turn.text", player.Statistics.Family.ToString());

            base.Show();
            ShowUser();
        }

        public override void Hide()
        {
            base.Hide();
            HideUser();
        }

        #endregion

        #region Methods

        private void LocalizeMessage(string key, params object[] args)
        {
            var localizedString = new LocalizedString("UIUtils", key);
            string localizedText = localizedString.GetLocalizedString();

            if (string.IsNullOrEmpty(localizedText))
            {
                Debug.LogWarning($"[Localization] Missing key: {key}");
                localizedText = key;
            }

            if (args != null && args.Length > 0)
            {
                try
                {
                    localizedText = string.Format(localizedText, args);
                }
                catch (FormatException e)
                {
                    Debug.LogError($"[Localization] Format error for key '{key}': {e.Message}");
                }
            }

            _MessageText.text = localizedText;
        }

        #endregion

        #region Events

        protected virtual void OnClose()
        {
            Controller.Hide();
        }

        #endregion
    }
}
