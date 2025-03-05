using UnityEngine;
using UnityEngine.Splines;
using Unity.Cinemachine;
using System.Collections.Generic;
using OMGG.DesignPattern;
using Newtonsoft.Json.Bson;

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
    
    private int _SplineID = 0;
    private int _Offset = 0;

    private bool _IsAnimated = false;

    private void Start()
    {
        _CinemachineSplineDolly = _CineCam.GetComponent<CinemachineSplineDolly>();

        if (_CinemachineSplineDolly == null )
        {
            Debug.Log("Cannot find _CinemachineSplineDolly");
        }
    }

    private void ResetAnimation(bool isReversed = false)
    {
        _SplineCameraAnimator.gameObject.SetActive(true);
        _SplineCameraAnimator.enabled = true;

        Debug.Log("ResetAnimation set the isReversed to " + isReversed);

        _SplineCameraAnimator.Rebind();  // Resets the animator's state
        _SplineCameraAnimator.SetBool("IsReversed", isReversed);
        _SplineCameraAnimator.Play(0, -1, 0f);  // Restart animation from the beginning
    }

    /// <summary>
    /// Check if we can go from one shop to another one instead of going form the sky to a shop
    /// </summary>
    /// <param name="splineID"></param>
    private void CheckBeforeProceed(int splineID)
    {
        Debug.Log($"[CheckBeforeProceed]: _SplineID({_SplineID}) > ((int)CamSplineType.MainViewToCourtyard - 1)({((int)CamSplineType.MainViewToCourtyard - 1)})");

        if (splineID == _SplineID) return;

        // TODO: Change the (int)CamSplineType.None to futur last index of none shop location OR use different container (maybe both) 
        if (splineID > ((int)CamSplineType.MainViewToCourtyard - 1) && _SplineID > ((int)CamSplineType.MainViewToCourtyard - 1))
        {

            // splineId - LastNoneShop + (LastShop - splineId)
            _Offset = splineID - (int)CamSplineType.None + ((int)CamSplineType.MainViewToMarket - splineID);

            StartSplineCamAnimation(splineID + _Offset);
            // Set the _SplineID not to the calculated one, but the actual one (it allows the code to do this calcul again and knows where we are)
            _SplineID = splineID;
        }
        else
        {
            Debug.Log("CheckBeforeProceed start animation form main view");
            StartSplineCamAnimation(splineID);
        }
    }

    public void StartSplineCamAnimation(int splineID)
    {
        if (splineID != 0 && (splineID - 1 >= _SplineContainerRef.Splines.Count || _SplineContainerRef.Splines[splineID - 1] == null ||
           splineID >= _LookAt.Count || _LookAt[splineID] == null || _IsAnimated))
        {
            Debug.Log("[CamsManager]: Cannot load this cam spline animation");
            return;
        }

        _IsAnimated = true;

        if (splineID == 0)
        {
            // Play the animation reversed
            Debug.Log($"[CamsManager]: Play {splineID} animation reversed");
            _CinemachineSplineDolly.SplineSettings.Spline.Spline = _SplineContainerRef.Splines[_SplineID - 1];
            _SplineID = splineID;
            ResetAnimation(true);
        }
        else
        {
            _CineCam.Follow = _LookAt[_SplineID].transform;
            _CinemachineSplineDolly.SplineSettings.Spline.Spline = _SplineContainerRef.Splines[splineID - 1];
            _SplineID = splineID;
            // Play the animation
            Debug.Log($"[CamsManager]: Play {splineID} animation");
            ResetAnimation();
        }
    }

    public void TestDeux()
    {
        if (_IsAnimated) return;
        CheckBeforeProceed((int)CamSplineType.None);
    }
    public void TestUn()
    {
        if (_IsAnimated) return;
        CheckBeforeProceed((int)CamSplineType.MainViewToMarket);
    }

    public void TestZero()
    {
        if (_IsAnimated) return;
        CheckBeforeProceed((int)CamSplineType.MainViewToCourtyard);
    }

    public void OnCamSplineCompleted()
    {
        // TODO: send an event
        Debug.Log("[CamsManager]: CamSplineCompleted");

        _SplineCameraAnimator.enabled = false;
        _IsAnimated = false;
    }

    public void ProceedLookAtRequest()
    {
        _CineCam.Follow = _LookAt[_SplineID].transform;
        return;
    }
}
