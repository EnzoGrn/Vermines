using UnityEngine.UI;

namespace Vermines.UI.Screen {

    public partial class GameplayUIGainSummary {

        public Button TutorialCloseButton => _CloseButton;

        /// <summary>
        /// Tutorial only: closes the summary as the close button does (next phase).
        /// </summary>
        public void TutorialClose()
        {
            OnCloseButtonPressed();
        }
    }
}
