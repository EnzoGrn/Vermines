using UnityEngine;
using Vermines.Player;
using Vermines.UI.Utils;

namespace Vermines.UI.Plugin
{
    using static Vermines.UI.Screen.GameplayUIBook;
    using Text = TMPro.TMP_Text;
    using Button = UnityEngine.UI.Button;
    using Image = UnityEngine.UI.Image;

    /// <summary>
    /// Manages the display of the phase banner in the gameplay screen.
    /// </summary>
    public class BookPagePlugin : GameplayScreenPlugin
    {
        [SerializeField] private BookTabType _PageType;
        public BookTabType PageType => _PageType;

        [Header("Player Info UI")]
        [SerializeField] private GameObject playerInfoPanel;
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text eloquenceText;
        [SerializeField] private Text soulsText;
        [SerializeField] private Text familyText;
        [SerializeField] private Image familyIcon;
        [SerializeField] private Image cultistImage;
        [SerializeField] private Image cultistBgImage;

        public virtual void Awake()
        {
            if (playerInfoPanel.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Show the plugin.
        /// </summary>
        /// <param name="screen">
        /// The parent screen that this plugin is attached to.
        /// </param>
        public override void Show(GameplayUIScreen screen)
        {
            base.Show(screen);
        }

        /// <summary>
        /// Hide the plugin.
        /// </summary>
        public override void Hide()
        {
            if (playerInfoPanel.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            base.Hide();
        }

        public void ShowPlayerInfo(PlayerData player)
        {
            playerNameText.text = player.PlayerRef == PlayerController.Local.PlayerRef ? "You" : player.Nickname;
            eloquenceText.text = $"{player.Eloquence} / 20";
            soulsText.text = $"{player.Souls} / 100";
            familyText.text = $"{player.Family}";
            familyIcon.sprite = UISpriteLoader.GetDefaultSprite(CardSystem.Enumerations.CardType.Partisan, player.Family, "Icon");
            cultistImage.sprite = UISpriteLoader.GetDefaultSprite(CardSystem.Enumerations.CardType.Partisan, player.Family, "Cultist");
            cultistBgImage.sprite = UISpriteLoader.GetDefaultSprite(CardSystem.Enumerations.CardType.Partisan, player.Family, "Background");
            if (playerInfoPanel.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }
}