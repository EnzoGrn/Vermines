using UnityEngine;
using System;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// What the TutorialUIController needs from a popup (speech bubble, window, narrator...).
    /// TutorialPopupView is the default implementation, driven by a prefab: implement this interface
    /// on your own component if your game needs a different behaviour.
    /// </summary>
    public interface ITutorialPopup {

        /// <summary>
        /// The player pressed the "Next" button.
        /// </summary>
        event Action NextClicked;

        /// <summary>
        /// The player pressed the "Skip" button.
        /// </summary>
        event Action SkipClicked;

        bool IsShown { get; }

        /// <summary>
        /// Shows the popup. title and portrait may be empty: hide the matching elements in that case.
        /// </summary>
        void Show(string title, string body, Sprite portrait, bool showNext, bool showSkip);

        void Hide();

        /// <summary>
        /// A slight jolt when the player attempts a prohibited action.
        /// </summary>
        void Nudge();

        /// <summary>
        /// Called every frame with the screen rectangle of the main target (null = no target).
        /// A popup that does not follow its target can ignore it.
        /// </summary>
        void SetAnchor(Rect? targetScreenRect, PopupPlacement placement, Camera canvasCamera);
    }
}
