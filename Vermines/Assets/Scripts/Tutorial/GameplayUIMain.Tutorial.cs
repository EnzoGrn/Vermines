using CelticEdge.Tutorial;

namespace Vermines.UI.Screen {

    public partial class GameplayUIMain {

        /// <summary>IDs the tutorial steps use to highlight the main HUD buttons.</summary>
        partial void AwakeUser()
        {
            TagForTutorial(_TurnButton   , "turn_button");
            TagForTutorial(_TableButton  , "table_button");
            TagForTutorial(_RecycleButton, "recycle_button");
        }

        private static void TagForTutorial(UnityEngine.Component component, string targetId)
        {
            if (component == null)
                return;
            if (!component.TryGetComponent(out TutorialTarget target))
                target = component.gameObject.AddComponent<TutorialTarget>();
            target.SetTargetId(targetId);
        }
    }
}
