using System.Collections.Generic;
using UnityEngine;
using System;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// What the TutorialUIController needs to highlight the targets of a step (veil with holes, outlines, glow...).
    /// TutorialSpotlightOverlay is the default implementation: a darkened veil with holes, drawn by a shader.
    /// </summary>
    public interface ITutorialHighlight {

        /// <summary>
        /// The player clicked outside the highlighted areas while clicks are blocked.
        /// </summary>
        event Action OnBlockedClick;

        bool IsVisible { get; }

        /// <summary>
        /// Highlights the given screen rectangles (empty = nothing highlighted).
        /// blockClicks = clicks outside the rectangles must not reach the game.
        /// </summary>
        void Show(IReadOnlyList<Rect> screenRects, SpotlightShape shape, float padding, bool dim, bool blockClicks);

        /// <summary>
        /// Called every frame with the new rectangles, so the highlight follows moving targets.
        /// </summary>
        void Track(IReadOnlyList<Rect> screenRects, SpotlightShape shape, float padding);

        void Hide();
    }
}
