using UnityEngine;

public class CinemachineCameraHandling : MonoBehaviour
{
    public void OnSplineAnimationEnd()
    {
        if (CamsManager.Instance)
            CamsManager.Instance.OnCamSplineCompleted();
    }

    public void LookAtRequest()
    {
        if (CamsManager.Instance)
            CamsManager.Instance.ProceedLookAtRequest();
    }
}
