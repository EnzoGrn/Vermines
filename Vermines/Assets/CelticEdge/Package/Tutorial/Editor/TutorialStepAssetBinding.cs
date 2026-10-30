using System.Collections.Generic;

namespace CelticEdge.Tutorial.EditorTools {

    using CelticEdge.Tutorial.Authoring;

    /// <summary>
    /// Copies between a table row (ParsedStep) and a TutorialStepData asset.
    /// The portrait is not in the table: it is kept when updating.
    /// </summary>
    internal static class TutorialStepAssetBinding {

        public static void Apply(ParsedStep src, TutorialStepData dst) => src.ApplyTo(dst);

        public static ParsedStep From(TutorialStepData s, string lessonId) => new ParsedStep {
            LessonId                  = lessonId,
            StepId                    = s.StepId,
            PopupStyle                = s.PopupStyle,
            Title                     = s.Title.Text,
            Body                      = s.Body.Text,
            TextKey                   = s.Body.Key,
            TargetIds                 = new List<string>(s.TargetIds),
            Shape                     = s.Shape,
            Padding                   = s.Padding,
            DimScreen                 = s.DimScreen,
            BlockClicksOutsideTargets = s.BlockClicksOutsideTargets,
            Pointer                   = s.Pointer,
            PointerDropTargetId       = s.PointerDropTargetId,
            Placement                 = s.Placement,
            AdvanceCondition          = s.AdvanceCondition,
            EventId                   = s.EventId,
            EventArg                  = s.EventArg,
            EventCount                = s.EventCount,
            DelaySeconds              = s.DelaySeconds,
            RestrictGameActions       = s.RestrictGameActions,
            AllowedActions            = new List<AllowedAction>(s.AllowedActions),
            OnEnter                   = new List<TutorialCommand>(s.OnEnter),
            OnExit                    = new List<TutorialCommand>(s.OnExit)
        };
    }
}
