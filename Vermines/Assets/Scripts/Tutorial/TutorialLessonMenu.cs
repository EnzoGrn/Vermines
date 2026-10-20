using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines.Tutorial.Integration {

    /// <summary>
    /// The lesson list opened by the Tutorial button of the main menu.
    /// Built by code (same look as the tutorial bubbles), to be replaced by a real view of the game later.
    /// </summary>
    public class TutorialLessonMenu : MonoBehaviour {

        #region Attributes

        private Canvas _Canvas;
        private RectTransform _List;
        private TMP_Text _Progress;

        public event Action<TutorialLessonData> LessonChosen;
        public event Action Closed;

        public bool IsOpen => _Canvas != null && _Canvas.enabled;

        #endregion

        #region Build

        public static TutorialLessonMenu Create(Transform parent)
        {
            GameObject root = new("TutorialLessonMenu", typeof(RectTransform));

            root.transform.SetParent(parent, false);

            TutorialLessonMenu menu = root.AddComponent<TutorialLessonMenu>();

            menu.Build();
            menu._Canvas.enabled = false;

            return menu;
        }

        private void Build()
        {
            _Canvas = gameObject.AddComponent<Canvas>();
            _Canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            _Canvas.sortingOrder = 900;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            // Dark background: blocks clicks on the menu behind.
            GameObject blocker = TutorialUIBuilder.NewUI("Blocker", transform);

            TutorialUIBuilder.Stretch((RectTransform)blocker.transform);
            blocker.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            GameObject panel = TutorialUIBuilder.NewUI("Panel", transform);
            RectTransform panelRT = (RectTransform)panel.transform;

            panelRT.anchorMin = panelRT.anchorMax = panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(820, 0);

            panel.AddComponent<Image>().color = TutorialUIBuilder.PanelColor;

            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();

            layout.padding                = new RectOffset(32, 32, 28, 28);
            layout.spacing                = 16;
            layout.childControlHeight     = true;
            layout.childControlWidth      = true;
            layout.childForceExpandHeight = false;

            panel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            TMP_Text title = TutorialUIBuilder.NewText("Title", panel.transform, 40, true);

            title.text      = "Didacticiel";
            title.alignment = TextAlignmentOptions.Center;

            _Progress = TutorialUIBuilder.NewText("Progress", panel.transform, 22, false);
            _Progress.alignment = TextAlignmentOptions.Center;

            GameObject list = TutorialUIBuilder.NewUI("Lessons", panel.transform);
            VerticalLayoutGroup listLayout = list.AddComponent<VerticalLayoutGroup>();

            listLayout.spacing                = 10;
            listLayout.childControlHeight     = true;
            listLayout.childControlWidth      = true;
            listLayout.childForceExpandHeight = false;

            _List = (RectTransform)list.transform;

            GameObject footer = TutorialUIBuilder.NewUI("Footer", panel.transform);
            HorizontalLayoutGroup footerLayout = footer.AddComponent<HorizontalLayoutGroup>();

            footerLayout.childAlignment        = TextAnchor.MiddleCenter;
            footerLayout.childForceExpandWidth = false;
            footerLayout.childControlWidth     = true;
            footerLayout.childControlHeight    = true;

            Button back = TutorialUIBuilder.NewButton("Back", footer.transform, "Retour", new Color(0.3f, 0.3f, 0.3f), 220);

            back.onClick.AddListener(Hide);
        }

        #endregion

        #region Methods

        public void Show(TutorialManager manager)
        {
            Refresh(manager);

            _Canvas.enabled = true;
        }

        public void Hide()
        {
            if (!IsOpen)
                return;
            _Canvas.enabled = false;

            Closed?.Invoke();
        }

        /// <summary>Hides without raising Closed (a lesson is starting).</summary>
        private void HideForLesson(TutorialLessonData lesson)
        {
            _Canvas.enabled = false;

            LessonChosen?.Invoke(lesson);
        }

        private void Refresh(TutorialManager manager)
        {
            for (int i = _List.childCount - 1; i >= 0; i--)
                Destroy(_List.GetChild(i).gameObject);
            TutorialCampaignData campaign = manager != null ? manager.Campaign : null;

            if (campaign == null || campaign.lessons.Count == 0) {
                _Progress.text = "Aucune leçon disponible.";

                return;
            }

            _Progress.text = $"Progression : {Mathf.RoundToInt(campaign.CompletionRatio(manager.Progress) * 100f)} %";

            int index = 1;

            foreach (TutorialLessonData lesson in campaign.lessons) {
                if (lesson == null)
                    continue;
                bool completed = manager.Progress.IsCompleted(lesson.lessonId);
                bool unlocked  = manager.IsUnlocked(lesson);

                AddLessonRow(index++, lesson, completed, unlocked);
            }
        }

        private void AddLessonRow(int index, TutorialLessonData lesson, bool completed, bool unlocked)
        {
            string state = completed ? "<color=#8FD18F>Terminée</color>" : unlocked ? string.Empty : "<color=#999999>Verrouillée</color>";
            Color color = unlocked ? (completed ? new Color(0.22f, 0.30f, 0.22f) : TutorialUIBuilder.AccentColor) : new Color(0.25f, 0.25f, 0.25f);

            Button button = TutorialUIBuilder.NewButton($"Lesson_{lesson.lessonId}", _List, string.Empty, color, 0);
            LayoutElement element = button.GetComponent<LayoutElement>();

            element.minHeight       = 84;
            element.flexibleWidth   = 1;

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();

            label.richText  = true;
            label.fontSize  = 24;
            label.alignment = TextAlignmentOptions.Left;
            label.margin    = new Vector4(20, 6, 20, 6);
            label.text      = $"<b>{index}. {lesson.displayName.Resolve()}</b>  {state}\n<size=19>{lesson.description.Resolve()}</size>";

            button.interactable = unlocked;
            button.onClick.AddListener(() => HideForLesson(lesson));
        }

        #endregion
    }
}
