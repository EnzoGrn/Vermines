using UnityEngine;

public class CinemachineCameraHandling : MonoBehaviour {

    private CamManager _Manager;

    public void OnSplineAnimationEnd()
    {
        if (GetCameraManager() != null)
            _Manager.OnCamSplineCompleted();
    }

    public void OnSplineAnimationStart()
    {
        if (GetCameraManager() != null)
            _Manager.OnCamSplineStart();
    }

    public void LookAtRequest()
    {
        if (GetCameraManager() != null)
            _Manager.ProceedLookAtRequest();
    }

    private CamManager GetCameraManager()
    {
        if (_Manager)
            return _Manager;
        _Manager = FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);

        return _Manager;
    }
}
