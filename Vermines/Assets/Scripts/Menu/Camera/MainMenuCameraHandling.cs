using UnityEngine;

namespace Vermines.Menu {

    public class MainMenuCameraHandling : MonoBehaviour {

        MainMenuCamera _Camera;

        private void GetCamera()
        {
            if (!_Camera)
                _Camera = FindFirstObjectByType<MainMenuCamera>();
        }

        public void OnSplineAnimationEnd()
        {
            GetCamera();

            if (_Camera)
                _Camera.OnCameraSplineCompleted();
        }

        public void OnSplineAnimationStart()
        {
            GetCamera();

            if (_Camera)
                _Camera.OnCameraSplineStart();
        }
    }
}
