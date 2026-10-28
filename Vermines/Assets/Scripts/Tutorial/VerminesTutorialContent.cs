using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Tutorial.Integration {

    using Vermines.Characters;
    using Vermines.Core;
    using CelticEdge.Tutorial;
    using CelticEdge.Tutorial.Authoring;

    /// <summary>
    /// Builds the tutorial campaign from a VerminesTutorialConfig:
    /// each lesson of the configuration gets its steps from the rows of the scenario table with its ID.
    /// Nothing about the lessons is written here: everything is in the configuration asset.
    /// </summary>
    public static class VerminesTutorialContent {

        #region Attributes

        /// <summary>
        /// The configuration of the tutorial, set by Build.
        /// </summary>
        public static VerminesTutorialConfig Config { get; private set; }

        #endregion

        #region Methods

        public static TutorialCampaignData Build(VerminesTutorialConfig config)
        {
            Config = config;

            VerminesTutorialActions.RegisterTableAliases();
            VerminesTutorialCommands.RegisterInTable();
            VerminesTutorialEvents.RegisterPassiveEvents();

            TutorialCampaignData campaign = ScriptableObject.CreateInstance<TutorialCampaignData>();

            campaign.name                   = "VerminesCampaign";
            campaign.CampaignId             = config != null ? config.CampaignId : "vermines";
            campaign.AutoStartOnFirstLaunch = false;

            if (config == null) {
                Debug.LogError("[Tutorial] No VerminesTutorialConfig given (UIMainMenuView._TutorialConfig): the tutorial has no lesson.");

                return campaign;
            }

            if (config.Scenario == null)
                Debug.LogError($"[Tutorial] No scenario table in {config.name}: the lessons have no step.", config);
            List<string>     warnings = new();
            List<ParsedStep> rows     = config.Scenario != null ? TutorialTable.Parse(config.Scenario.text, warnings) : new List<ParsedStep>();

            foreach (VerminesLessonDefinition definition in config.Lessons) {
                if (definition == null || string.IsNullOrEmpty(definition.LessonId))
                    continue;
                campaign.Lessons.Add(Lesson(definition, rows, warnings));
            }

            foreach (string warning in warnings)
                Debug.LogWarning("[Tutorial] Table: " + warning);
            return campaign;
        }

        /// <summary>
        /// The offers and tips of a free-play lesson (TutorialFreePlayCoach), null for a guided lesson.
        /// </summary>
        public static TutorialFreePlayRules FreePlayRules(string lessonId)
        {
            VerminesLessonDefinition definition = Config != null ? Config.FindLesson(lessonId) : null;

            if (definition == null || definition.FreePlay == null || definition.FreePlay.IsEmpty)
                return null;
            return definition.FreePlay;
        }

        /// <summary>
        /// A lesson of the campaign: its steps are the rows of the table for this lesson,
        /// the rows of "lecon_04/trop_cher" are the interlude "trop_cher" (played on the game's request).
        /// The board, the opponent and the hints are copies: the match never changes the configuration asset.
        /// </summary>
        private static TutorialLessonData Lesson(VerminesLessonDefinition definition, List<ParsedStep> rows, List<string> warnings)
        {
            TutorialLessonData lesson = TutorialTableLoader.CreateLesson(definition.LessonId, rows, warnings.Add);

            lesson.DisplayName = new TutorialText(definition.DisplayName);
            lesson.Description = new TutorialText(definition.Description);
            lesson.Setup       = Copy(definition.Setup);
            lesson.Hints       = Copy(definition.Hints);

            lesson.ScriptedAgents.Add(Copy(definition.Opponent));

            if (!string.IsNullOrEmpty(definition.RequiredLessonId))
                lesson.RequiredLessonIds.Add(definition.RequiredLessonId);
            if (lesson.Steps.Count == 0)
                warnings.Add($"lesson \"{definition.LessonId}\" has no step in the table.");
            foreach (VerminesStepPortrait portrait in definition.Portraits)
                ShowPortrait(lesson, portrait, warnings);
            return lesson;
        }

        private static T Copy<T>(T source) where T : class, new()
        {
            return source != null ? JsonUtility.FromJson<T>(JsonUtility.ToJson(source)) : new T();
        }

        /// <summary>
        /// The image on the popup of a step: its sprite, or the image of its god.
        /// </summary>
        private static void ShowPortrait(TutorialLessonData lesson, VerminesStepPortrait portrait, List<string> warnings)
        {
            TutorialStepData step = portrait != null ? lesson.FindStep(portrait.StepId) : null;

            if (step == null) {
                warnings.Add($"lesson \"{lesson.LessonId}\": portrait of an unknown step \"{portrait?.StepId}\".");

                return;
            }
            if (portrait.Sprite != null) {
                step.Portrait = portrait.Sprite;

                return;
            }

            God[] gods = Global.Settings != null && Global.Settings.Gods != null ? Global.Settings.Gods.GetAllGods() : null;

            if (gods == null || !int.TryParse(portrait.GodId, out int id))
                return;
            foreach (God god in gods) {
                if (god != null && god.ID == id)
                    step.Portrait = god.DivinitySprite;
            }
        }

        #endregion
    }
}
