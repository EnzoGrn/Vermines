using System.Collections.Generic;
using UnityEngine;

namespace CelticEdge.Tutorial {

    public enum PopupStyle {

        [Tooltip("Bubble attached to the target, with an arrow.")]
        Bubble,

        [Tooltip("Centered window, for long explanations (rule, symbol...).")]
        Modal,

        [Tooltip("Narrator character in a corner of the screen.")]
        Narrator,

        [Tooltip("No text: only the highlight and/or the pointer.")]
        None
    }

    public enum PopupPlacement {
        Auto,
        Above,
        Below,
        Left,
        Right
    }

    public enum SpotlightShape {
        RoundedRect,
        Circle
    }

    public enum PointerMode {

        None,

        [Tooltip("A hand/arrow tapping the main target.")]
        Tap,

        [Tooltip("A hand sliding from the main target to the drop target.")]
        Drag
    }

    public enum AdvanceCondition {

        [Tooltip("Moves on to the next step right away (commands-only step).")]
        Immediate,

        [Tooltip("Moves on after a delay.")]
        WaitForDelay,

        [Tooltip("Waits for the popup's \"Next\" button.")]
        WaitForButton,

        [Tooltip("Waits for a game event published through TutorialEventBus.")]
        WaitForEvent,

        [Tooltip("Waits for a click on the main target.")]
        WaitForTargetClick
    }

    /// <summary>
    /// A step: what is said, what is highlighted, what the player is allowed to
    /// do, and what they must do to move on. Based on the TutorialStepData of the
    /// TutorialSystem prototype.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTutorialStep", menuName = "CelticEdge/Tutorial/Step")]
    public class TutorialStepData : ScriptableObject {

        [Tooltip("Stable ID of the step (\"step_id\" column of the table). Used to update it from the CSV.")]
        public string StepId;

        [Header("Text")]

        public PopupStyle     PopupStyle = PopupStyle.Bubble;
        public TutorialText   Title;
        public TutorialText   Body;
        public Sprite         Portrait;
        public PopupPlacement Placement  = PopupPlacement.Auto;

        [Header("Highlight")]

        [Tooltip("IDs of the TutorialTargets to highlight (4 max). The first one anchors the popup and the pointer.")]
        public List<string> TargetIds = new();

        public SpotlightShape Shape = SpotlightShape.RoundedRect;

        [Tooltip("Margin around the target, in canvas pixels.")]
        public float Padding = 16f;

        [Tooltip("Darkens the screen around the targets.")]
        public bool DimScreen = true;

        [Tooltip("Blocks clicks outside the highlighted areas.")]
        public bool BlockClicksOutsideTargets = true;

        [Header("Pointer")]

        public PointerMode Pointer = PointerMode.None;

        [Tooltip("For PointerMode.Drag: ID of the target to drop onto.")]
        public string PointerDropTargetId;

        [Header("Progression")]

        public AdvanceCondition AdvanceCondition = AdvanceCondition.WaitForButton;

        [Tooltip("WaitForEvent: ID of the event published by the game (TutorialEventBus.Publish).")]
        public string EventId;

        [Tooltip("WaitForEvent: expected argument (e.g. the object's ID). Empty = any.")]
        public string EventArg;

        [Min(1)][Tooltip("WaitForEvent: number of occurrences to wait for.")]
        public int EventCount = 1;

        [Tooltip("WaitForDelay: duration in seconds.")]
        public float DelaySeconds = 2f;

        [Header("Rules during the step")]

        [Tooltip("Limits the player's actions to the list below (empty list = no action).")]
        public bool RestrictGameActions = true;

        public List<AllowedAction> AllowedActions = new();

        [Header("Commands")]

        [Tooltip("Executed before the step is displayed.")]
        public List<TutorialCommand> OnEnter = new();

        [Tooltip("Executed once the condition is met.")]
        public List<TutorialCommand> OnExit = new();

        public string MainTargetId => TargetIds != null && TargetIds.Count > 0 ? TargetIds[0] : null;

        /// <summary>
        /// Input problems that can be detected without running the game (shown by the editor
        /// and by the CSV import).
        /// </summary>
        public IEnumerable<string> Validate()
        {
            if (AdvanceCondition == AdvanceCondition.WaitForButton && PopupStyle == PopupStyle.None)
                yield return "Waits for the \"Next\" button but shows no popup: the player will be stuck.";
            if (AdvanceCondition == AdvanceCondition.WaitForEvent && string.IsNullOrEmpty(EventId))
                yield return "Waits for an event but no event ID is set.";
            if (AdvanceCondition == AdvanceCondition.WaitForTargetClick && string.IsNullOrEmpty(MainTargetId))
                yield return "Waits for a click on the target but no target is set.";
            if (Pointer == PointerMode.Drag && string.IsNullOrEmpty(PointerDropTargetId))
                yield return "Drag pointer without a drop target.";
            if (Pointer != PointerMode.None && string.IsNullOrEmpty(MainTargetId))
                yield return "Pointer requested without a target.";
            if (TargetIds != null && TargetIds.Count > TutorialSpotlightOverlay.MaxHoles)
                yield return $"More than {TutorialSpotlightOverlay.MaxHoles} targets: the extra ones will not be highlighted.";
            if (AdvanceCondition == AdvanceCondition.WaitForEvent && RestrictGameActions && AllowedActions.Count == 0 && !TutorialEvents.IsPassive(EventId))
                yield return "Waits for a game action but no action is allowed: check \"allowed actions\".";
            if (PopupStyle != PopupStyle.None && Body.IsEmpty && Title.IsEmpty)
                yield return "Popup displayed without any text.";
        }
    }
}
