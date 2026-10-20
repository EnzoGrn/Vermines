using System.Collections.Generic;
using Vermines.Tutorial.Authoring;

namespace Vermines.Tutorial.EditorTools
{
    /// Copie entre une ligne du tableau (ParsedStep) et un asset TutorialStepData.
    /// Le portrait n'est pas dans le tableau : il est conservé à la mise à jour.
    internal static class TutorialStepAssetBinding
    {
        public static void Apply(ParsedStep src, TutorialStepData dst) => src.ApplyTo(dst);

        public static ParsedStep From(TutorialStepData s, string lessonId) => new ParsedStep
        {
            lessonId = lessonId,
            stepId = s.stepId,
            popupStyle = s.popupStyle,
            title = s.title.text,
            body = s.body.text,
            textKey = s.body.key,
            targetIds = new List<string>(s.targetIds),
            shape = s.shape,
            padding = s.padding,
            dimScreen = s.dimScreen,
            blockClicksOutsideTargets = s.blockClicksOutsideTargets,
            pointer = s.pointer,
            pointerDropTargetId = s.pointerDropTargetId,
            placement = s.placement,
            advanceCondition = s.advanceCondition,
            eventId = s.eventId,
            eventArg = s.eventArg,
            eventCount = s.eventCount,
            delaySeconds = s.delaySeconds,
            restrictGameActions = s.restrictGameActions,
            allowedActions = new List<AllowedAction>(s.allowedActions),
            onEnter = new List<TutorialCommand>(s.onEnter),
            onExit = new List<TutorialCommand>(s.onExit)
        };
    }
}
