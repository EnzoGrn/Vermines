using System;
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

        [Tooltip("Petites suites d'étapes que le jeu demande pendant la leçon (TutorialManager.PlayInterlude) : aide après un coup interdit, carte proposée... Elles passent par-dessus l'étape en cours.")]
        public List<TutorialInterlude> interludes = new List<TutorialInterlude>();

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

        public TutorialInterlude FindInterlude(string interludeId)
        {
            foreach (var i in interludes)
                if (i != null && i.interludeId == interludeId)
                    return i;
            return null;
        }
    }

    /// Une séquence annexe d'une leçon. Dans le tableau CSV, ses lignes ont pour leçon
    /// "id_leçon/id_séquence" (ex : lecon_04/trop_cher).
    [Serializable]
    public class TutorialInterlude
    {
        public string interludeId;
        public List<TutorialStepData> steps = new List<TutorialStepData>();
    }
}
