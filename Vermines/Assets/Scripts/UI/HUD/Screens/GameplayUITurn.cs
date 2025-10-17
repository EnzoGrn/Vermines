using Fusion;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Vermines.CardSystem.Enumerations;
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
            PlayerRef currentPlayer = GameManager.Instance.GetCurrentPlayer();
            GameDataStorage.Instance.PlayerData.TryGet(currentPlayer, out PlayerData playerData);
            _CultistImage.sprite = UISpriteLoader.GetDefaultSprite(CardType.Partisan, playerData.Family, "Cultist");

             if (PlayerController.Local != null && currentPlayer == PlayerController.Local.PlayerRef)
                LocalizeMessage("Turn.your");
            else
                LocalizeMessage("Turn.text", playerData.Nickname);

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
