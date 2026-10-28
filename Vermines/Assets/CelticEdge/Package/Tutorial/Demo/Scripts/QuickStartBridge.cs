using System.Collections;
using UnityEngine;
using System;

namespace CelticEdge.Tutorial.Demo {

    /// <summary>
    /// The lesson setup specific to this game: each lesson can choose the starting gold.
    /// </summary>
    [Serializable]
    public class QuickStartSetup : ITutorialLessonSetup {

        public int StartingGold;
    }

    /// <summary>
    /// The bridge between the tutorial and the demo game.
    /// </summary>
    public class QuickStartBridge : TutorialGameBridge {

        public QuickStartGame Game { get; set; }

        public override IEnumerator PrepareLesson(TutorialLessonData lesson)
        {
            var setup = lesson.GetSetup<QuickStartSetup>();

            Game.ResetGame(setup != null ? setup.StartingGold : 0);

            yield break;
        }

        /// <summary>
        /// Game commands usable in the table: AddGold(amount=2).
        /// </summary>
        public override IEnumerator ExecuteCommand(TutorialCommand command)
        {
            if (command.Is("AddGold")) {
                Game.AddGold(command.GetInt("amount", "nombre"));

                yield break;
            }

            yield return base.ExecuteCommand(command);
        }

        public override void CleanupLesson(TutorialLessonData lesson, bool completed)
        {
            Debug.Log($"[Tutorial Demo] Lesson {lesson.LessonId} ended, completed: {completed}.");
        }
    }
}
