using System.Collections;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// What the game must provide for the tutorial.
    /// This is the only class you need to write on the game side
    /// (a MonoBehaviour in the game scene, dragged into the TutorialManager or registered via TutorialManager.Instance.SetBridge(this)).
    /// TutorialGameBridge provides a basic version to be completed.
    ///
    /// The methods are coroutines: the tutorial waits for the animations to finish before displaying the next step.
    /// </summary>
    public interface ITutorialGameBridge {

        /// <summary>
        /// Prepares the game for the lesson. When this function is called, GameRandom.Current is already set to the lesson's scripted random value:
        /// the game starts as usual, and only the elements specific to the lesson (lesson.Setup, starting resources, etc.) require specific code.
        /// </summary>
        IEnumerator PrepareLesson(TutorialLessonData lesson);

        /// <summary>
        /// Executes a game command (all except those in TutorialCommands).
        /// </summary>
        IEnumerator ExecuteCommand(TutorialCommand command);

        /// <summary>
        /// Causes a scripted agent to perform an action (see TutorialSession.PlayScriptedTurn).
        /// </summary>
        IEnumerator PerformScriptedAction(int agentIndex, ScriptedAction action);

        /// <summary>
        /// Cleans up after the lesson (returns to the menu, end screen, etc.).
        /// completed = true if the lesson is passed, false if abandoned or failed.
        /// </summary>
        void CleanupLesson(TutorialLessonData lesson, bool completed);
    }

    /// <summary>
    /// Base Bridge to be completed: Each method does nothing by default.
    /// </summary>
    public abstract class TutorialGameBridge : MonoBehaviour, ITutorialGameBridge {

        public virtual IEnumerator PrepareLesson(TutorialLessonData lesson)
        {
            yield break;
        }

        public virtual IEnumerator ExecuteCommand(TutorialCommand command)
        {
            Debug.LogWarning($"[Tutorial] The {command} command is not supported by {GetType().Name}.", this);

            yield break;
        }

        public virtual IEnumerator PerformScriptedAction(int agentIndex, ScriptedAction action)
        {
            Debug.LogWarning($"[Tutorial] Scripted action {action} not supported by {GetType().Name}.", this);

            yield break;
        }

        public virtual void CleanupLesson(TutorialLessonData lesson, bool completed) {}
    }
}
