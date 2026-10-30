using CelticEdge.Tutorial;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

namespace Vermines.Tutorial.Integration {

    /// <summary>
    /// The lesson list opened by the Tutorial button of the main menu.
    /// Its look is the prefab of VerminesTutorialConfig (LessonMenuPrefab): one row (TutorialLessonRow) is created per lesson.
    /// </summary>
    public class TutorialLessonMenu : MonoBehaviour {

        #region Attributes

        [SerializeField]
        [Tooltip("Enabled while the list is open.")]
        private Canvas _Canvas;

        [SerializeField]
        [Tooltip("Parent of the lesson rows (usually with a VerticalLayoutGroup).")]
        private RectTransform _List;

        [SerializeField]
        [Tooltip("The row created for each lesson.")]
        private TutorialLessonRow _RowPrefab;

        [SerializeField]
        [Tooltip("Completion of the campaign.")]
        private TMP_Text _Progress;

        [SerializeField]
        [Tooltip("Closes the list.")]
        private Button _BackButton;

        [Header("Texts")]

        [SerializeField]
        [Tooltip("{0} = completion of the campaign, in percent.")]
        private string _ProgressFormat = "Progression : {0} %";

        [SerializeField]
        private string _NoLessonText = "Aucune leçon disponible.";

        public event Action<TutorialLessonData> LessonChosen;
        public event Action Closed;

        public bool IsOpen => _Canvas != null && _Canvas.enabled;

        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            if (_Canvas == null)
                _Canvas = GetComponent<Canvas>();
            if (_BackButton != null)
                _BackButton.onClick.AddListener(Hide);
            if (_Canvas != null)
                _Canvas.enabled = false;
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

        /// <summary>
        /// Hides without raising Closed (a lesson is starting).
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
            TutorialCampaignData campaign = manager != null ? manager.Campaign : null;

            if (campaign == null || campaign.Lessons.Count == 0) {
                SetProgress(_NoLessonText);

                return;
            }

            SetProgress(string.Format(_ProgressFormat, Mathf.RoundToInt(campaign.CompletionRatio(manager.Progress) * 100f)));

            if (_RowPrefab == null) {
                Debug.LogError("[Tutorial] TutorialLessonMenu has no row prefab: the lessons can't be listed.", this);

                return;
            }

            int index = 1;

            foreach (TutorialLessonData lesson in campaign.Lessons) {
                if (lesson == null)
                    continue;
                TutorialLessonRow row = Instantiate(_RowPrefab, _List);

                row.Setup(index++, lesson, manager.Progress.IsCompleted(lesson.LessonId), manager.IsUnlocked(lesson), HideForLesson);
            }
        }

        private void SetProgress(string text)
        {
            if (_Progress != null)
                _Progress.text = text;
        }

        #endregion
    }
}
