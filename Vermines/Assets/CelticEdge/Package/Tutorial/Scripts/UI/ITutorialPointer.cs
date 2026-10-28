using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// What the TutorialUIController needs from a pointer (hand, arrow...).
    /// TutorialPointerView is the default implementation, animating the RectTransform you give it.
    /// </summary>
    public interface ITutorialPointer {

        /// <summary>
        /// Called every frame while the step shows a pointer. from/to are in screen coordinates, to is ignored in Tap mode.
        /// </summary>
        void Show(PointerMode mode, Vector2 fromScreen, Vector2 toScreen, Camera canvasCamera);

        void Hide();
    }
}
