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

        [Header("Player List UI")]
        [SerializeField] private Transform playerListContainer;
        [SerializeField] private GameObject playerButtonPrefab;

        [Header("Player Info UI")]
        [SerializeField] private GameObject playerInfoPanel;
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text eloquenceText;
        [SerializeField] private Text soulsText;
        [SerializeField] private Text familyText;
        [SerializeField] private Image familyIcon;
        [SerializeField] private Image cultistImage;

        /// <summary>
        /// Show the plugin.
        /// </summary>
        /// <param name="screen">
        /// The parent screen that this plugin is attached to.
        /// </param>
        public override void Show(GameplayUIScreen screen)
        {
            if (playerInfoPanel.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            base.Show(screen);
            RefreshPlayerList();
        }

        /// <summary>
        /// Hide the plugin.
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            ClearPlayerList();
        }

        private void RefreshPlayerList()
        {
            ClearPlayerList();

            foreach (var player in GameDataStorage.Instance.PlayerData)
            {
                GameObject btnObj = Instantiate(playerButtonPrefab, playerListContainer);
                var btnText = btnObj.GetComponentInChildren<Text>();
                var button = btnObj.GetComponent<Button>();

                btnText.text = player.Value.Nickname + (player.Value.PlayerRef == PlayerController.Local.PlayerRef ? " (You)" : string.Empty);

                button.onClick.AddListener(() => ShowPlayerInfo(player.Value));
            }
        }

        private void ClearPlayerList()
        {
            foreach (Transform child in playerListContainer)
                Destroy(child.gameObject);
        }

        private void ShowPlayerInfo(PlayerData player)
        {
            if (playerInfoPanel.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            playerNameText.text = player.Nickname + (player.PlayerRef == PlayerController.Local.PlayerRef ? " (You)" : string.Empty);
            eloquenceText.text = $"Eloquence: {player.Eloquence} / 20";
            soulsText.text = $"Souls: {player.Souls} / 100";
            familyText.text = $"{player.Family}";
            familyIcon.sprite = UISpriteLoader.GetDefaultSprite(CardSystem.Enumerations.CardType.Partisan, player.Family, "Icon");
            //cultistImage.sprite = GameDataStorage.Instance.GetCultistSprite(player.Family);
        }
    }
}