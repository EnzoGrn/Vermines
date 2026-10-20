using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Tutorial
{
    public enum LessonEndCondition
    {
        [Tooltip("La leçon est réussie à la fin de la dernière étape.")]
        AfterLastStep,
        [Tooltip("La leçon est réussie quand la partie se termine (TutorialEvents.MatchEnded).")]
        OnMatchEnd
    }

    /// Une micro-partie scriptée : sa mise en place, son adversaire, ses étapes.
    /// (Équivalent d'une "mission" du tutoriel de Hearthstone.)
    [CreateAssetMenu(fileName = "NewTutorialLesson", menuName = "Vermines/Tutoriel/Leçon")]
    public class TutorialLessonData : ScriptableObject
    {
        [Tooltip("Identifiant stable, écrit dans la sauvegarde du joueur. Ne plus le changer après la sortie du jeu.")]
        public string lessonId;

        public TutorialText displayName;
        public TutorialText description;
        public Sprite icon;

        [Tooltip("Scène à charger pour la leçon. Vide = scène courante.")]
        public string sceneName;

        [Tooltip("Leçons à terminer avant de débloquer celle-ci. Vide = la leçon précédente de la campagne.")]
        public List<string> requiredLessonIds = new List<string>();

        [Header("Partie scriptée")]
        public ScriptedMatchSetup matchSetup = new ScriptedMatchSetup();
        public OpponentScript opponent = new OpponentScript();

        [Header("Déroulé")]
        public List<TutorialStepData> steps = new List<TutorialStepData>();
        public LessonEndCondition endCondition = LessonEndCondition.AfterLastStep;

        [Tooltip("Le joueur peut passer cette leçon.")]
        public bool allowSkip = true;

        [Tooltip("Passer la leçon la compte comme terminée (débloque la suivante).")]
        public bool skipCountsAsCompleted = true;

        public TutorialStepData FindStep(string stepId)
        {
            foreach (var s in steps)
                if (s != null && s.stepId == stepId)
                    return s;
            return null;
        }
    }
}
