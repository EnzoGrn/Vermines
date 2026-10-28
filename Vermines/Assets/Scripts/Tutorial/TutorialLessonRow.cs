using CelticEdge.Tutorial;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

namespace Vermines.Tutorial.Integration {

    /// <summary>
    /// One lesson of the lesson menu (TutorialLessonMenu): its number, name, state and description.
    /// Clickable only when the lesson is unlocked.
    /// </summary>
    public class TutorialLessonRow : MonoBehaviour {

        #region Attributes

        [SerializeField]
        private Button _Button;

        [SerializeField]
        [Tooltip("Takes the color of the lesson state. Empty = the colors are not used.")]
        private Image _Background;

        [SerializeField]
        private TMP_Text _Label;

        [SerializeField]
        [TextArea]
        [Tooltip("Text of the label: {0} = number, {1} = name, {2} = state, {3} = description.")]
        private string _LabelFormat = "<b>{0}. {1}</b>  {2}\n<size=19>{3}</size>";

        [Header("States")]

        [SerializeField]
        private Color _AvailableColor = new(0.80f, 0.58f, 0.16f);

        [SerializeField]
        private Color _CompletedColor = new(0.22f, 0.30f, 0.22f);

        [SerializeField]
        private Color _LockedColor = new(0.25f, 0.25f, 0.25f);

        [SerializeField]
        private string _CompletedText = "<color=#8FD18F>Terminée</color>";

        [SerializeField]
        private string _LockedText = "<color=#999999>Verrouillée</color>";

        #endregion

        #region Methods

        public void Setup(int index, TutorialLessonData lesson, bool completed, bool unlocked, Action<TutorialLessonData> chosen)
        {
            string state = completed ? _CompletedText : unlocked ? string.Empty : _LockedText;

            name = $"Lesson_{lesson.LessonId}";

            if (_Background != null)
                _Background.color = unlocked ? (completed ? _CompletedColor : _AvailableColor) : _LockedColor;
            if (_Label != null)
                _Label.text = string.Format(_LabelFormat, index, lesson.DisplayName.Resolve(), state, lesson.Description.Resolve());
            if (_Button != null) {
                _Button.interactable = unlocked;
                _Button.onClick.RemoveAllListeners();
                _Button.onClick.AddListener(() => chosen?.Invoke(lesson));
            }
        }

        #endregion
    }
}
