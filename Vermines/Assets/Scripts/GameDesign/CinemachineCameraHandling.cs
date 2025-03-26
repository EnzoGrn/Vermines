using UnityEngine;

public class CinemachineCameraHandling : MonoBehaviour
{
    public void OnSplineAnimationEnd()
    {
        if (CamManager.Instance)
            CamManager.Instance.OnCamSplineCompleted();
    }

    public void OnSplineAnimationStart()
    {
        if (CamManager.Instance)
            CamManager.Instance.OnCamSplineStart();
    }

    public void LookAtRequest()
    {
        if (CamManager.Instance)
            CamManager.Instance.ProceedLookAtRequest();
    }

}
