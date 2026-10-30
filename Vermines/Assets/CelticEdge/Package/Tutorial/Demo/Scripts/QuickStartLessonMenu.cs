using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

namespace CelticEdge.Tutorial.Demo {

    /// <summary>
    /// List of campaign lessons (completed, locked, progress), built by code with the same look as the demo's popups.
    /// This is a sample: a real game shows the lessons in its own screen, with the same data
    /// (TutorialManager.Campaign, TutorialManager.IsUnlocked, TutorialManager.Progress, TutorialCampaignData.CompletionRatio).
    ///
    /// <code>
    ///   var menu = QuickStartLessonMenu.Create(transform);
    ///   
    ///   menu.LessonChosen += lesson => TutorialManager.Instance.StartLesson(lesson);
    ///   
    ///   menu.Show(TutorialManager.Instance);
    /// </code>
    /// </summary>
    public class QuickStartLessonMenu : MonoBehaviour {

        public static string TitleLabel     = "Tutorial";
        public static string BackLabel      = "Back";
        public static string EmptyLabel     = "No lessons available.";
        public static string ProgressFormat = "Progress: {0}%";
        public static string CompletedLabel = "Completed";
        public static string LockedLabel    = "Locked";

        private Canvas        _Canvas;
        private RectTransform _List;
        private TMP_Text      _Progress;

        public event Action<TutorialLessonData> LessonChosen;
        public event Action Closed;

        public bool IsOpen => _Canvas != null && _Canvas.enabled;

        public static QuickStartLessonMenu Create(Transform parent)
        {
            var root = new GameObject("QuickStartLessonMenu", typeof(RectTransform));

            root.transform.SetParent(parent, false);

            var menu = root.AddComponent<QuickStartLessonMenu>();

            menu.Build();

            menu._Canvas.enabled = false;

            return menu;
        }

        private void Build()
        {
            _Canvas              = gameObject.AddComponent<Canvas>();
            _Canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            _Canvas.sortingOrder = 900;

            var scaler = gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            // Dark background: prevents clicks on the game menu behind it.
            var blocker = QuickStartUI.NewUI("Blocker", transform);

            QuickStartUI.Stretch((RectTransform)blocker.transform);

            blocker.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            var panel   = QuickStartUI.NewUI("Panel", transform);
            var panelRT = (RectTransform)panel.transform;

            panelRT.anchorMin = panelRT.anchorMax = panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(820, 0);

            panel.AddComponent<Image>().color = QuickStartUI.PanelColor;

            var layout = panel.AddComponent<VerticalLayoutGroup>();

            layout.padding                = new RectOffset(32, 32, 28, 28);
            layout.spacing                = 16;
            layout.childControlHeight     = true;
            layout.childControlWidth      = true;
            layout.childForceExpandHeight = false;

            panel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = QuickStartUI.NewText("Title", panel.transform, 40, true);

            title.text      = TitleLabel;
            title.alignment = TextAlignmentOptions.Center;

            _Progress           = QuickStartUI.NewText("Progress", panel.transform, 22, false);
            _Progress.alignment = TextAlignmentOptions.Center;

            var list       = QuickStartUI.NewUI("Lessons", panel.transform);
            var listLayout = list.AddComponent<VerticalLayoutGroup>();

            listLayout.spacing                = 10;
            listLayout.childControlHeight     = true;
            listLayout.childControlWidth      = true;
            listLayout.childForceExpandHeight = false;

            _List = (RectTransform)list.transform;

            var footer       = QuickStartUI.NewUI("Footer", panel.transform);
            var footerLayout = footer.AddComponent<HorizontalLayoutGroup>();

            footerLayout.childAlignment        = TextAnchor.MiddleCenter;
            footerLayout.childForceExpandWidth = false;
            footerLayout.childControlWidth     = true;
            footerLayout.childControlHeight    = true;

            var back = QuickStartUI.NewButton("Back", footer.transform, BackLabel, new Color(0.3f, 0.3f, 0.3f), 220);

            back.onClick.AddListener(Hide);
        }

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

        /// <summary>
        /// Hide without raising "Closed": a lesson begins.
        /// </summary>
        private void HideForLesson(TutorialLessonData lesson)
        {
            _Canvas.enabled = false;

            LessonChosen?.Invoke(lesson);
        }

        private void Refresh(TutorialManager manager)
        {
            for (int i = _List.childCount - 1; i >= 0; i--)
                Destroy(_List.GetChild(i).gameObject);
            var campaign = manager != null ? manager.Campaign : null;

            if (campaign == null || campaign.Lessons.Count == 0) {
                _Progress.text = EmptyLabel;

                return;
            }

            _Progress.text = string.Format(ProgressFormat, Mathf.RoundToInt(campaign.CompletionRatio(manager.Progress) * 100f));

            int index = 1;

            foreach (var lesson in campaign.Lessons) {
                if (lesson == null)
                    continue;
                AddLessonRow(index++, lesson, manager.Progress.IsCompleted(lesson.LessonId), manager.IsUnlocked(lesson));
            }
        }

        private void AddLessonRow(int index, TutorialLessonData lesson, bool completed, bool unlocked)
        {
            string state = completed ? $"<color=#8FD18F>{CompletedLabel}</color>"  : unlocked ? string.Empty : $"<color=#999999>{LockedLabel}</color>";
            Color  color = unlocked  ? (completed ? new Color(0.22f, 0.30f, 0.22f) : QuickStartUI.AccentColor) : new Color(0.25f, 0.25f, 0.25f);
            var   button = QuickStartUI.NewButton($"Lesson_{lesson.LessonId}", _List, string.Empty, color, 0);
            var  element = button.GetComponent<LayoutElement>();

            element.minHeight     = 84;
            element.flexibleWidth = 1;

            var label = button.GetComponentInChildren<TMP_Text>();

            label.richText                = true;
            label.fontSize                = 24;
            label.alignment               = TextAlignmentOptions.Left;
            label.margin                  = new Vector4(20, 6, 20, 6);
            label.text                    = $"<b>{index}. {lesson.DisplayName.Resolve()}</b>  {state}\n<size=19>{lesson.Description.Resolve()}</size>";
            button.interactable           = unlocked;
            button.onClick.AddListener(() => HideForLesson(lesson));
        }
    }
}
