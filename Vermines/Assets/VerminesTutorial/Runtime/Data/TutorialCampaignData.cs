using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Tutorial
{
    /// Le tutoriel complet : la liste ordonnée des leçons.
    [CreateAssetMenu(fileName = "TutorialCampaign", menuName = "Vermines/Tutoriel/Campagne")]
    public class TutorialCampaignData : ScriptableObject
    {
        public string campaignId = "main";
        public List<TutorialLessonData> lessons = new List<TutorialLessonData>();

        [Tooltip("Lancer automatiquement la première leçon non terminée au premier démarrage du jeu.")]
        public bool autoStartOnFirstLaunch = true;

        public TutorialLessonData FindLesson(string lessonId)
        {
            foreach (var l in lessons)
                if (l != null && l.lessonId == lessonId)
                    return l;
            return null;
        }

        public bool IsUnlocked(TutorialLessonData lesson, TutorialProgressService progress)
        {
            if (lesson == null)
                return false;
            if (progress == null)
                return true;

            if (lesson.requiredLessonIds != null && lesson.requiredLessonIds.Count > 0)
            {
                foreach (var id in lesson.requiredLessonIds)
                    if (!progress.IsCompleted(id))
                        return false;
                return true;
            }

            int index = lessons.IndexOf(lesson);
            return index <= 0 || progress.IsCompleted(lessons[index - 1]?.lessonId);
        }

        /// Première leçon débloquée et non terminée, ou null si tout est fini.
        public TutorialLessonData GetNextLesson(TutorialProgressService progress)
        {
            foreach (var l in lessons)
                if (l != null && !progress.IsCompleted(l.lessonId) && IsUnlocked(l, progress))
                    return l;
            return null;
        }

        public bool IsFinished(TutorialProgressService progress)
        {
            if (progress.IsSkipped)
                return true;
            foreach (var l in lessons)
                if (l != null && !progress.IsCompleted(l.lessonId))
                    return false;
            return true;
        }

        public float CompletionRatio(TutorialProgressService progress)
        {
            if (lessons.Count == 0)
                return 1f;
            int done = 0;
            foreach (var l in lessons)
                if (l != null && progress.IsCompleted(l.lessonId))
                    done++;
            return (float)done / lessons.Count;
        }
    }
}
