using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// The game-specific setup of a lesson: decks, starting resources, map,
    /// level... The game declares its class, and each lesson holds an instance of it
    /// (TutorialLessonData.Setup, picked in the inspector among the project's classes):
    ///
    ///   [Serializable]
    ///   public class MyLessonSetup : ITutorialLessonSetup {
    ///       public int          StartingGold  = 10;
    ///       public List<string> StartingItems = new();
    ///   }
    ///
    /// The bridge reads it in PrepareLesson (lesson.GetSetup<MyLessonSetup>()), the rest of the
    /// game through TutorialSession.GetSetup<MyLessonSetup>().
    /// </summary>
    public interface ITutorialLessonSetup {}

    /// <summary>
    /// Shows in the inspector a list of the project's ITutorialLessonSetup classes to
    /// pick the one used by the [SerializeReference] field.
    /// </summary>
    public sealed class TutorialSetupPickerAttribute : PropertyAttribute {}
}
