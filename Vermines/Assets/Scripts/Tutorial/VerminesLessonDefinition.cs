using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Tutorial.Integration {

    using CelticEdge.Tutorial;

    /// <summary>
    /// A lesson of the tutorial (VerminesTutorialConfig.Lessons): everything but its steps, which are in the scenario table.
    /// </summary>
    [Serializable]
    public class VerminesLessonDefinition {

        #region Attributes

        [Tooltip("ID of the lesson in the scenario table (\"lecon_01\").")]
        public string LessonId;

        public string DisplayName;

        [TextArea]
        public string Description;

        [Tooltip("Lesson to finish before this one. Empty = always available.")]
        public string RequiredLessonId;

        [Tooltip("The board when the lesson starts.")]
        public VerminesLessonSetup Setup = new();

        [Tooltip("The scripted opponent (AgentIndex = its index in Setup.Players).\n" +
                 "UseGameAIForUnscriptedTurns: it plays by itself (VerminesTutorialBridge.PlayAITurn); otherwise its unscripted turns are reduced to UnscriptedTurnAction.")]
        public ScriptedAgent Opponent = new();

        [Tooltip("Interludes played on a trigger: a key of the game's errors (Shop_Buy_NotEnoughEloquence...) or of TutorialFreePlayCoach (court_incomplete...).\n" +
                 "The interludes are the rows \"<lesson>/<interlude>\" of the table.")]
        public TutorialHintSet Hints = new();

        [Tooltip("Free-play lessons only: cards offered in the court, advice on cards, a match to win. Empty = a guided lesson.")]
        public TutorialFreePlayRules FreePlay = new();

        [Tooltip("Steps whose popup shows an image (a god the game does not show elsewhere during a match).")]
        public List<VerminesStepPortrait> Portraits = new();

        #endregion
    }

    /// <summary>
    /// The image of a step's popup (TutorialStepData.Portrait).
    /// </summary>
    [Serializable]
    public class VerminesStepPortrait {

        #region Attributes

        public string StepId;

        [Tooltip("ID of the god whose image is shown (Global.Settings.Gods). Ignored when Sprite is set.")]
        public string GodId;

        public Sprite Sprite;

        #endregion
    }
}
