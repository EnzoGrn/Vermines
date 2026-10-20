namespace Vermines.UI.Screen {

    using Vermines.Tutorial;

    public partial class GameplayUIMain {

        /// <summary>IDs the tutorial steps use to highlight the main HUD buttons.</summary>
        partial void AwakeUser()
        {
            TagForTutorial(_TurnButton, "turn_button");
            TagForTutorial(_TableButton, "table_button");
            TagForTutorial(_RecycleButton != null ? _RecycleButton : FindRecycleButton(), "recycle_button");
        }

        /// <summary>
        /// _RecycleButton is not assigned in the UI scene: the "RecycleButton" object calls OnRecycleButtonPressed with SendMessage.
        /// It is looked up by name, first under this screen, then in the loaded scenes.
        /// </summary>
        private UnityEngine.UI.Button FindRecycleButton()
        {
            const string buttonName = "RecycleButton";

            foreach (UnityEngine.UI.Button button in GetComponentsInChildren<UnityEngine.UI.Button>(true))
                if (button.name == buttonName)
                    return button;
            foreach (UnityEngine.UI.Button button in FindObjectsByType<UnityEngine.UI.Button>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
                if (button.name == buttonName)
                    return button;
            UnityEngine.Debug.LogWarning("[Tutorial] Recycle button not found: the recycle_button target won't exist.");

            return null;
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
