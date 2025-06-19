using Fusion;
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
    /// Handles the behavior and visuals of a single page in the book UI during gameplay.
    /// Displays player-specific information and handles page visibility and content updates.
    /// </summary>
    public class BookPagePlugin : GameplayScreenPlugin
    {
        /// <summary>
        /// The type of this page, corresponding to a tab in the book.
        /// </summary>
        [SerializeField] private BookTabType _PageType;

        /// <summary>
        /// The public accessor for this page's tab type.
        /// </summary>
        public BookTabType PageType => _PageType;

        [Header("Player Info UI")]

        [SerializeField] private GameObject leftPage;
        [SerializeField] private GameObject rightPage;

        [SerializeField] private Text playerNameText;
        [SerializeField] private Text eloquenceText;
        [SerializeField] private Text soulsText;
        [SerializeField] private Text familyText;

        [SerializeField] private Image familyIcon;
        [SerializeField] private Image cultistImage;
        [SerializeField] private Image cultistBgImage;

        private PlayerRef _playerRef;

        [Header("Card Decks Buttons")]

        /// <summary>
        /// Button to access the player's discarded cards.
        /// </summary>
        [InlineHelp, SerializeField]
        private Button discardedButton;

        /// <summary>
        /// Button to access the player's played cards.
        /// </summary>
        [InlineHelp, SerializeField]
        private Button playedButton;

        /// <summary>
        /// Button to access the player's sacrificed cards.
        /// </summary>
        [InlineHelp, SerializeField]
        private Button sacrificedButton;

        [Header("Card Decks Holder")]

        /// <summary>
        /// The UI GameObject that holds the player's deck.
        /// </summary>
        [InlineHelp, SerializeField]
        private PlayerDeckBook deckHolder;

        /// <summary>
        /// Initializes the plugin, preparing the canvas groups for both pages.
        /// Called by Unity when the script instance is being loaded.
        /// </summary>
        public virtual void Awake()
        {
            if (leftPage.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (rightPage.TryGetComponent<CanvasGroup>(out var canvasGroupRight))
            {
                canvasGroupRight.alpha = 0f;
                canvasGroupRight.interactable = false;
                canvasGroupRight.blocksRaycasts = false;
            }

            // Set up button listeners
            discardedButton.onClick.AddListener(ShowDiscardedCards);
            playedButton.onClick.AddListener(ShowPlayedCards);
            sacrificedButton.onClick.AddListener(ShowSacrificedCards);
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
            if (leftPage.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (rightPage.TryGetComponent<CanvasGroup>(out var canvasGroupRight))
            {
                canvasGroupRight.alpha = 0f;
                canvasGroupRight.interactable = false;
                canvasGroupRight.blocksRaycasts = false;
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

            EquipmentBookSection equipmentSection = GetComponentInChildren<EquipmentBookSection>();
            if (equipmentSection != null)
            {
                equipmentSection.UpdateEquipment(GameDataStorage.Instance.PlayerDeck[player.PlayerRef].Equipments);
            }

            if (leftPage.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            if (rightPage.TryGetComponent<CanvasGroup>(out var canvasGroupRight))
            {
                canvasGroupRight.alpha = 1f;
                canvasGroupRight.interactable = true;
                canvasGroupRight.blocksRaycasts = true;
            }

            _playerRef = player.PlayerRef;
        }

        public void ShowDiscardedCards()
        {
            deckHolder.Show(GameDataStorage.Instance.PlayerDeck[_playerRef].Discard);
            deckHolder.SetTitle("Discarded Cards");
        }

        public void ShowPlayedCards()
        {
            deckHolder.Show(GameDataStorage.Instance.PlayerDeck[_playerRef].PlayedCards);
            deckHolder.SetTitle("Played Cards");
        }

        public void ShowSacrificedCards()
        {
            deckHolder.Show(GameDataStorage.Instance.PlayerDeck[_playerRef].Graveyard);
            deckHolder.SetTitle("Sacrificed Cards");
        }
    }
}