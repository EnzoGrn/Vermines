using UnityEngine.EventSystems;
using UnityEngine;

namespace CelticEdge.Tutorial.Demo {

    using CelticEdge.Tutorial.Authoring;

    /// <summary>
    /// Entry point of the demo (scene Demo/Scenes/Demo_Tutorial), or put it on an empty GameObject of any scene, then press Play.
    /// Everything is built by code: the game, the tutorial system and the lesson list.
    /// The steps come from the table Demo/Resources/quickstart.csv.
    /// </summary>
    public class QuickStartDemo : MonoBehaviour {

        private QuickStartLessonMenu _Menu;

        private void Start()
        {
            EnsureEventSystem();

            var game = QuickStartGame.Create(transform);

            // The lessons: steps and interludes come from the table, the rest is set by code.
            TutorialTable.RegisterCommands("AddGold");

            var campaign = TutorialTableLoader.LoadCampaign("quickstart", "quickstart");

            if (campaign == null)
                return;
            var lessonOne = campaign.FindLesson("lesson_01");

            lessonOne.DisplayName = new TutorialText("Harvest");
            lessonOne.Description = new TutorialText("The gold counter and the Harvest button.");
            lessonOne.Setup       = new QuickStartSetup { StartingGold = 0 };

            var lessonTwo = campaign.FindLesson("lesson_02");

            lessonTwo.DisplayName = new TutorialText("Buy");
            lessonTwo.Description = new TutorialText("Free play: buy a sword, with hints.");
            lessonTwo.Setup       = new QuickStartSetup { StartingGold = 1 };

            // Hint played when the game reports a lack of gold, at most once every 5 seconds.
            lessonTwo.Hints.PeriodEventId = null;

            lessonTwo.Hints.Hints.Add(new TutorialHint("not_enough_gold", "too_expensive", cooldownPeriods: 0) { CooldownSeconds = 5f });

            // The tutorial system (manager + views), built by code with the demo's look.
            var manager = QuickStartUI.BuildSystem(campaign);

            manager.Initialize(new InMemoryTutorialProgressStore(), "demo");

            var bridge = gameObject.AddComponent<QuickStartBridge>();

            bridge.Game = game;

            manager.SetBridge(bridge);

            manager.VerboseLogs = true;

            _Menu = QuickStartLessonMenu.Create(transform);

            _Menu.LessonChosen      += manager.StartLesson;
            manager.LessonCompleted += (lesson, skipped) => _Menu.Show(manager);
            manager.LessonAborted   += lesson => _Menu.Show(manager);

            _Menu.Show(manager);
        }

        private static void EnsureEventSystem()
        {
            #if UNITY_2023_1_OR_NEWER
                bool exists = FindFirstObjectByType<EventSystem>() != null;
            #else
                bool exists = FindObjectOfType<EventSystem>() != null;
            #endif

            if (!exists)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
