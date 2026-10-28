using System.Collections.Generic;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>The whole tutorial: the ordered list of lessons.</summary>
    [CreateAssetMenu(fileName = "TutorialCampaign", menuName = "CelticEdge/Tutorial/Campaign")]
    public class TutorialCampaignData : ScriptableObject {

        public string                   CampaignId = "main";
        public List<TutorialLessonData> Lessons    = new();

        [Tooltip("Automatically start the first unfinished lesson the first time the game is launched.")]
        public bool AutoStartOnFirstLaunch = true;

        public TutorialLessonData FindLesson(string lessonId)
        {
            foreach (var l in Lessons) {
                if (l != null && l.LessonId == lessonId)
                    return l;
            }

            return null;
        }

        public bool IsUnlocked(TutorialLessonData lesson, TutorialProgressService progress)
        {
            if (lesson == null)
                return false;
            if (progress == null)
                return true;
            if (lesson.RequiredLessonIds != null && lesson.RequiredLessonIds.Count > 0) {
                foreach (var id in lesson.RequiredLessonIds) {
                    if (!progress.IsCompleted(id))
                        return false;
                }

                return true;
            }

            int index = Lessons.IndexOf(lesson);

            return index <= 0 || progress.IsCompleted(Lessons[index - 1]?.LessonId);
        }

        /// <summary>First unlocked and unfinished lesson, or null if everything is done.</summary>
        public TutorialLessonData GetNextLesson(TutorialProgressService progress)
        {
            foreach (var l in Lessons) {
                if (l != null && !progress.IsCompleted(l.LessonId) && IsUnlocked(l, progress))
                    return l;
            }

            return null;
        }

        public bool IsFinished(TutorialProgressService progress)
        {
            if (progress.IsSkipped)
                return true;
            foreach (var l in Lessons) {
                if (l != null && !progress.IsCompleted(l.LessonId))
                    return false;
            }

            return true;
        }

        public float CompletionRatio(TutorialProgressService progress)
        {
            if (Lessons.Count == 0)
                return 1f;
            int done = 0;

            foreach (var l in Lessons) {
                if (l != null && progress.IsCompleted(l.LessonId))
                    done++;
            }

            return (float)done / Lessons.Count;
        }
    }
}
