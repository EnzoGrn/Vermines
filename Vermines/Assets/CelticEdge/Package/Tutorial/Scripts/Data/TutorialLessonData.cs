using System.Collections.Generic;
using System;
using UnityEngine;

namespace CelticEdge.Tutorial {

    public enum LessonEndCondition {

        [Tooltip("The lesson is passed at the end of the last step.")]
        AfterLastStep,

        [Tooltip("The lesson is passed when the game publishes TutorialEvents.GameEnded (\"game_ended\").")]
        OnGameEnd
    }

    /// <summary>
    /// A lesson: its setup, its steps, its side sequences and its hints.
    /// (Equivalent to a "mission" of the Hearthstone tutorial.)
    /// </summary>
    [CreateAssetMenu(fileName = "NewTutorialLesson", menuName = "CelticEdge/Tutorial/Lesson")]
    public class TutorialLessonData : ScriptableObject {

        [Tooltip("Stable ID, written to the player's save. Do not change it once the game has shipped.")]
        public string LessonId;

        public TutorialText DisplayName;
        public TutorialText Description;
        public Sprite       Icon;

        [Tooltip("Scene to load for the lesson. Empty = current scene.")]
        public string SceneName;

        [Tooltip("Lessons to complete before this one is unlocked. Empty = the previous lesson of the campaign.")]
        public List<string> RequiredLessonIds = new();

        [Header("Setup")]

        [SerializeReference][TutorialSetupPicker][Tooltip("Game-specific data (a class implementing ITutorialLessonSetup), read by the bridge in PrepareLesson.")]
        public ITutorialLessonSetup Setup;

        [Tooltip("Randomness of the lesson: forced shuffle orders and draws (see GameRandom).")]
        public ScriptedRandomSetup Random = new();

        [Tooltip("Opponents or allies whose turns are written in advance (turn-based games).")]
        public List<ScriptedAgent> ScriptedAgents = new();

        [Header("Flow")]
        public List<TutorialStepData> Steps        = new();
        public LessonEndCondition     EndCondition = LessonEndCondition.AfterLastStep;

        [Tooltip("Short sequences of steps requested by the game during the lesson (TutorialManager.PlayInterlude): help after a forbidden move, a tip... They play over the current step.")]
        public List<TutorialInterlude> Interludes = new();

        [Tooltip("Hints played automatically (side sequences) when a trigger occurs.")]
        public TutorialHintSet Hints = new();

        [Tooltip("The player can skip this lesson.")]
        public bool AllowSkip = true;

        [Tooltip("Skipping the lesson counts it as completed (unlocks the next one).")]
        public bool SkipCountsAsCompleted = true;

        public T GetSetup<T>() where T : class, ITutorialLessonSetup => Setup as T;

        public TutorialStepData FindStep(string stepId)
        {
            foreach (var s in Steps) {
                if (s != null && s.StepId == stepId)
                    return s;
            }

            foreach (var i in Interludes) {
                if (i == null)
                    continue;
                foreach (var s in i.Steps) {
                    if (s != null && s.StepId == stepId)
                        return s;
                }
            }

            return null;
        }

        public TutorialInterlude FindInterlude(string interludeId)
        {
            foreach (var i in Interludes) {
                if (i != null && i.InterludeId == interludeId)
                    return i;
            }

            return null;
        }
    }

    /// <summary>
    /// A side sequence of a lesson. In the CSV table, its rows have
    /// "lesson_id/sequence_id" as their lesson (e.g. lecon_04/trop_cher).
    /// </summary>
    [Serializable]
    public class TutorialInterlude {

        public string                 InterludeId;
        public List<TutorialStepData> Steps = new();
    }
}
