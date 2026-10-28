using System.Collections.Generic;
using UnityEngine;
using System;

namespace CelticEdge.Tutorial.Authoring {

    /// <summary>
    /// Builds lessons at runtime from the scenario array, without using assets:
    /// the game can deliver the array as-is (TextAsset in Resources) and modify it without reimporting.
    /// The "lesson_04" lines are the lesson steps, and the "lesson_04/too_expensive" lines are the "too_expensive" sub-sequence.
    ///
    /// <code>
    ///   var campaign = TutorialTableLoader.LoadCampaign("Tutorial/scenario");
    ///
    ///   campaign.FindLesson("lecon_01").Setup = new MyLessonSetup {
    ///     StartingGold = 5
    ///   };
    ///
    ///   TutorialManager.Instance.SetCampaign(campaign);
    /// </code>
    /// </summary>
    public static class TutorialTableLoader {

        /// <summary>
        /// A campaign with one lesson per lesson ID in the array, in the order in which they appear;
        /// each lesson depends on the previous one. Null if the resource is missing.
        /// </summary>
        public static TutorialCampaignData LoadCampaign(string resourcePath, string campaignId = "main")
        {
            var text = Resources.Load<TextAsset>(resourcePath);

            if (text == null) {
                Debug.LogError($"[Tutorial] Table not found: Resources/{resourcePath}.");

                return null;
            }

            var warnings = new List<string>();
            var campaign = CreateCampaign(TutorialTable.Parse(text.text, warnings), campaignId, warnings.Add);

            foreach (var warning in warnings)
                Debug.LogWarning("[Tutorial] Table: " + warning);
            return campaign;
        }

        public static TutorialCampaignData CreateCampaign(IReadOnlyList<ParsedStep> rows, string campaignId = "main", Action<string> warn = null)
        {
            var campaign = ScriptableObject.CreateInstance<TutorialCampaignData>();

            campaign.name       = campaignId;
            campaign.CampaignId = campaignId;

            foreach (string lessonId in LessonIds(rows))
                campaign.Lessons.Add(CreateLesson(lessonId, rows, warn));
            return campaign;
        }

        /// <summary>
        /// The lesson IDs in the table (excluding supplementary units), in order.
        /// </summary>
        public static List<string> LessonIds(IEnumerable<ParsedStep> rows)
        {
            var ids = new List<string>();

            foreach (var row in rows) {
                string id = MainLessonId(row.LessonId);

                if (!string.IsNullOrEmpty(id) && !ids.Contains(id))
                    ids.Add(id);
            }

            return ids;
        }

        public static TutorialLessonData CreateLesson(string lessonId, IEnumerable<ParsedStep> rows, Action<string> warn = null)
        {
            var lesson = ScriptableObject.CreateInstance<TutorialLessonData>();

            lesson.name        = lessonId;
            lesson.LessonId    = lessonId;
            lesson.DisplayName = new TutorialText(lessonId);

            Fill(lesson, rows, warn);

            return lesson;
        }

        /// <summary>
        /// Replaces the lesson's steps and related sequences with those in the table.
        /// </summary>
        public static void Fill(TutorialLessonData lesson, IEnumerable<ParsedStep> rows, Action<string> warn = null)
        {
            lesson.Steps.Clear();
            lesson.Interludes.Clear();

            foreach (var row in rows) {
                if (!BelongsTo(row, lesson.LessonId, out string interludeId))
                    continue;
                var step = ScriptableObject.CreateInstance<TutorialStepData>();

                step.name = row.StepId;

                row.ApplyTo(step);

                foreach (string problem in step.Validate())
                    warn?.Invoke($"Line {row.SourceLine} ({row.StepId}): {problem}");
                if (interludeId == null) {
                    lesson.Steps.Add(step);

                    continue;
                }

                var interlude = lesson.FindInterlude(interludeId);

                if (interlude == null) {
                    lesson.Interludes.Add(interlude = new TutorialInterlude {
                        InterludeId = interludeId
                    });
                }

                interlude.Steps.Add(step);
            }

            if (lesson.Steps.Count == 0)
                warn?.Invoke($"No steps for this lesson \"{lesson.LessonId}\".");
        }

        /// <summary>
        /// True if the line belongs to the lesson; interludeId = the associated sequence, or null for a step within the lesson itself.
        /// </summary>
        public static bool BelongsTo(ParsedStep row, string lessonId, out string interludeId)
        {
            interludeId = null;

            if (row == null || string.IsNullOrEmpty(row.LessonId) || string.IsNullOrEmpty(lessonId))
                return false;
            if (row.LessonId == lessonId)
                return true;
            if (!row.LessonId.StartsWith(lessonId + "/", StringComparison.Ordinal))
                return false;
            interludeId = row.LessonId[(lessonId.Length + 1)..];

            return interludeId.Length > 0;
        }

        private static string MainLessonId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            int slash = id.IndexOf('/');

            return slash < 0 ? id : id.Substring(0, slash);
        }
    }
}
