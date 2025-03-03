using UnityEngine;
using UnityEngine.Splines;
using Unity.Cinemachine;
using System.Collections.Generic;
using OMGG.DesignPattern;

public class CamsManager : MonoBehaviourSingleton<CamsManager>
{
    [Header("Camera")]
    [SerializeField] private CinemachineCamera _CineCam;
    [SerializeField] private Camera _MainFixCam;
    [SerializeField] private Camera _MainCam;
    [SerializeField] private List<GameObject> _LookAt;

    [Header("Spline")]
    [SerializeField] private SplineContainer _SplineContainerRef;
    [SerializeField] private Animator _SplineCameraAnimator;

    private CinemachineSplineDolly _CinemachineSplineDolly;
    
    // (+1) is used when we get the lookAt gameObject because there is an additional one for the initial lookAt
    private int _SplineID = -1;

    private bool _IsAnimated = false;

    private void Start()
    {
        _CinemachineSplineDolly = _CineCam.GetComponent<CinemachineSplineDolly>();

        if (_CinemachineSplineDolly == null )
        {
            Debug.Log("Cannot find _CinemachineSplineDolly");
        }
    }

    private void ResetAnimation()
    {
        _SplineCameraAnimator.gameObject.SetActive(true);
        _SplineCameraAnimator.enabled = true;
        _SplineCameraAnimator.Rebind();  // Resets the animator's state
        _SplineCameraAnimator.Play(0, -1, 0f);  // Restart animation from the beginning
    }

    public void StartSplineCamAnimation(int splineID)
    {
        if (splineID >= _SplineContainerRef.Splines.Count || _SplineContainerRef.Splines[splineID] == null ||
           splineID + 1 >= _LookAt.Count || _LookAt[splineID + 1] == null || _IsAnimated)
        {
            Debug.Log("[CamsManager]: Cannot load this cam spline animation");
            return;
        }

         if (splineID == _SplineID) return;

         _SplineID = splineID;
        _IsAnimated = true;
        _CineCam.Follow = _LookAt[splineID + 1].transform;
        _CinemachineSplineDolly.SplineSettings.Spline.Spline = _SplineContainerRef.Splines[splineID];
        
        // Play the animation
        Debug.Log($"[CamsManager]: Play {splineID} animation");
        ResetAnimation();
    }

    public void TestDeux()
    {
        if (_IsAnimated) return;
        StartSplineCamAnimation(2);
    }
    public void TestUn()
    {
        if (_IsAnimated) return;
        StartSplineCamAnimation(1);
    }

    public void TestZero()
    {
        if (_IsAnimated) return;
        StartSplineCamAnimation(0);
    }

    public void OnCamSplineCompleted()
    {
        // TODO: send an event

        Debug.Log("[CamsManager]: CamSplineCompleted");

        _SplineCameraAnimator.enabled = false;
        _IsAnimated = false;
    }
}
