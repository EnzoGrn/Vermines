using UnityEngine;

namespace Vermines.UI.Plugin
{
    using Text = TMPro.TMP_Text;
    using Button = UnityEngine.UI.Button;
    using Image = UnityEngine.UI.Image;
    using Vermines.UI.Book;
    using TMPro;
    using static Vermines.UI.Screen.GameplayUIBook;

    public class RulesBookPlugin : GameplayScreenPlugin
    {
        public RuleBook ruleBook;
        public Text leftTitleText;
        public Text leftContentText;
        public Text rightTitleText;
        public Text rightContentText;

        private RuleBookSection currentSection;
        private int currentPageIndexInSection = 0;

        [SerializeField] private GameObject leftPage;
        [SerializeField] private GameObject rightPage;

        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;

        [SerializeField] private BookTabType _PageType;
        public BookTabType PageType => _PageType;

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
        }

        /// <summary>
        /// Show the plugin.
        /// </summary>
        /// <param name="screen">
        /// The parent screen that this plugin is attached to.
        /// </param>
        public override void Show(GameplayUIScreen screen)
        {
            Debug.Log("Showing RulesBookPlugin");
            base.Show(screen);
        }

        /// <summary>
        /// Hide the plugin.
        /// </summary>
        public override void Hide()
        {
            Debug.Log("Hiding RulesBookPlugin");
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

        public void ShowPages()
        {
            var pages = currentSection.pages;

            var left = pages[currentPageIndexInSection];
            var right = currentPageIndexInSection + 1 < pages.Count
                ? pages[currentPageIndexInSection + 1]
                : null;

            LoadPage(left, leftTitleText, leftContentText);

            if (right != null)
                LoadPage(right, rightTitleText, rightContentText);
            else
            {
                rightTitleText.text = "";
                rightContentText.text = "";
            }

            if (leftPage.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                Debug.Log($"Setting left page canvas group: {leftPage.name} to active");
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            if (rightPage.TryGetComponent<CanvasGroup>(out var canvasGroupRight))
            {
                Debug.Log($"Setting right page canvas group: {rightPage.name} to active");
                canvasGroupRight.alpha = right != null ? 1f : 0f;
                canvasGroupRight.interactable = right != null;
                canvasGroupRight.blocksRaycasts = right != null;
            }

            previousButton.gameObject.SetActive(currentPageIndexInSection > 0);
            nextButton.gameObject.SetActive(currentPageIndexInSection + 2 < pages.Count);
        }

        void LoadPage(LocalizedRulePage page, Text title, Text content)
        {
            page.title.StringChanged += value =>
            {
                title.text = value;
                // If the title is empty, we can hide the title text component to avoid empty space
                title.gameObject.SetActive(!string.IsNullOrEmpty(value));
            };
            page.content.StringChanged += value => content.text = value;
            page.title.RefreshString();
            page.content.RefreshString();
        }

        public void NextPages()
        {
            if (currentPageIndexInSection + 2 < currentSection.pages.Count)
            {
                currentPageIndexInSection += 2;
                ShowPages();
            }
        }

        public void PreviousPages()
        {
            if (currentPageIndexInSection - 2 >= 0)
            {
                currentPageIndexInSection -= 2;
                ShowPages();
            }
        }

        public void GoToSection(RuleBookSectionType sectionType)
        {
            var section = ruleBook.GetSectionByType(sectionType);
            if (section != null)
            {
                currentSection = section;
                currentPageIndexInSection = 0;
                ShowPages();
            }
        }
    }
}