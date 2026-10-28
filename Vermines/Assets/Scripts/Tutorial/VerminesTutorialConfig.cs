using System.Collections.Generic;
using CelticEdge.Tutorial;
using UnityEngine;

namespace Vermines.Tutorial.Integration {

    /// <summary>
    /// Everything that makes the tutorial of Vermines, editable in the inspector:
    ///     the scenario table (the steps of every lesson, see Resources/Tutorial/vermines_scenario.csv);
    ///     the lessons (menu, starting board, opponent, hints, free play);
    ///     the interface (prefabs of the tutorial display and of the lesson menu).
    /// Given to the tutorial by the main menu (UIMainMenuView._TutorialConfig).
    /// </summary>
    [CreateAssetMenu(fileName = "VerminesTutorialConfig", menuName = "Vermines/Tutorial/Config")]
    public class VerminesTutorialConfig : ScriptableObject {

        #region Attributes

        [Header("Tutorial Assets")]

        [SerializeField]
        [Tooltip("The scenario table: the steps of every lesson, and their interludes (rows of the lesson \"lecon_04/trop_cher\").")]
        private TextAsset _Scenario;

        public TextAsset Scenario => _Scenario;

        [Header("Lessons")]

        [SerializeField]
        [Tooltip("ID of the campaign: the saved progress of the players is attached to it.")]
        private string _CampaignId = "vermines";

        public string CampaignId => _CampaignId;

        [SerializeField]
        [Tooltip("In the order of the lesson menu. The steps of a lesson are the rows of the table with its ID.")]
        private List<VerminesLessonDefinition> _Lessons = new();

        public IReadOnlyList<VerminesLessonDefinition> Lessons => _Lessons;

        [Header("Interface")]

        [SerializeField]
        [Tooltip("The tutorial display: veil, pointer, speech bubble, window and narrator (TutorialUIController and its views).")]
        private TutorialUIController _InterfacePrefab;

        public TutorialUIController InterfacePrefab => _InterfacePrefab;

        [SerializeField]
        [Tooltip("The lesson list opened by the Tutorial button of the main menu.")]
        private TutorialLessonMenu _LessonMenuPrefab;

        public TutorialLessonMenu LessonMenuPrefab => _LessonMenuPrefab;

        #endregion

        #region Methods

        /// <summary>
        /// The lesson with this ID, or null.
        /// </summary>
        public VerminesLessonDefinition FindLesson(string lessonId)
        {
            foreach (VerminesLessonDefinition lesson in _Lessons) {
                if (lesson != null && lesson.LessonId == lessonId)
                    return lesson;
            }

            return null;
        }

        #endregion
    }
}
