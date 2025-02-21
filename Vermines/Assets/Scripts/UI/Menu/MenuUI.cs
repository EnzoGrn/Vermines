using UnityEngine;

namespace Vermines {

    public class MenuUI : MenuUIController {

        protected override void Awake()
        {
            base.Awake();

            ShowCursor();
        }

        /// <summary>
        /// Function called when the game is started.
        /// </summary>
        public override void OnGameStarted()
        {
            base.OnGameStarted();

            foreach (Transform child in transform)
                child.gameObject.SetActive(false);
        }

        /// <summary>
        /// Function called when the game is stopped.
        /// </summary>
        public override void OnGameStopped()
        {
            foreach (Transform child in transform)
                child.gameObject.SetActive(true);
            ShowCursor();

            base.OnGameStopped();
        }

        /// <summary>
        /// Function that shows the cursor.
        /// </summary>
        internal void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
