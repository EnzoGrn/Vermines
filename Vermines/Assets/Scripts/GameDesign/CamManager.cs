using UnityEngine;
using UnityEngine.Splines;
using Unity.Cinemachine;
using System.Collections.Generic;
using OMGG.DesignPattern;
using UnityEngine.Events;
using Vermines.HUD;

public class CamManager : MonoBehaviourSingleton<CamManager>
{
    #region Exposed Fields
    [Header("Camera")]
    [SerializeField] private CinemachineCamera _CineCam;
    [SerializeField] private Camera _MainFixCam;
    [SerializeField] private Camera _MainCam;
    [SerializeField] private List<GameObject> _LookAt;

    [Header("Spline")]
    [SerializeField] private SplineContainer _SplineContainerRef;
    [SerializeField] private Animator _SplineCameraAnimator;

    public UnityEvent<CamSplineType> OnCamLocationChanged = new();
    public UnityEvent<CamSplineType> OnCamLocationIsChanging = new();
    #endregion

    #region Private Fields

    private CinemachineSplineDolly _CinemachineSplineDolly;
    private int _SplineID = 0;
    private int _Offset = 0;
    private bool _IsAnimated = false;

    #endregion

    #region Methods

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

        Debug.Log("[ResetAnimation]: set the isReversed to " + isReversed);
        _SplineCameraAnimator.SetBool("IsReversed", isReversed);
        if (!isReversed)
        {
            _SplineCameraAnimator.Rebind();  // Resets the animator's state
            _SplineCameraAnimator.Play(0, -1, 0f);  // Restart animation from the beginning
        }
            
    }

    /// <summary>
    /// Check if we can go from one shop to another one instead of going form the sky to a shop
    /// </summary>
    /// <param name="splineID"></param>
    public void OnSplineAnimationRequest(int splineID)
    {
        // Debug.Log($"[CheckBeforeProceed]: _SplineID({_SplineID}) > ((int)CamSplineType.MainViewToCourtyard - 1)({((int)CamSplineType.MainViewToCourtyard - 1)})");

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
            StartSplineCamAnimation(splineID);
        }
    }

    public void StartSplineCamAnimation(int splineID)
    {
        if (splineID != 0 && (splineID - 1 >= _SplineContainerRef.Splines.Count || _SplineContainerRef.Splines[splineID - 1] == null ||
           splineID >= _LookAt.Count || _LookAt[splineID] == null || _IsAnimated))
        {
            Debug.Log("[CamManager]: Cannot load this cam spline animation");
            return;
        }

        _IsAnimated = true;

        if (splineID == 0)
        {
            _CinemachineSplineDolly.SplineSettings.Spline.Spline = _SplineContainerRef.Splines[_SplineID - 1];
            _SplineID = splineID;
            //_CinemachineSplineType = (CamSplineType)splineID;

            Debug.Log($"[CamManager]: Play {splineID} animation reversed");

            // Play the animation reversed
            ResetAnimation(true);
        }
        else
        {
            _CineCam.Follow = _LookAt[_SplineID].transform;
            _CinemachineSplineDolly.SplineSettings.Spline.Spline = _SplineContainerRef.Splines[splineID - 1];
            _SplineID = splineID;
            //_CinemachineSplineType = (CamSplineType)splineID;

            Debug.Log($"[CamManager]: Play {splineID} animation");

            // Play the animation
            ResetAnimation();
        }
    }

    public void GoOnNoneLocation()
    {
        if (_IsAnimated) return;
        OnSplineAnimationRequest((int)CamSplineType.None);
    }
    public void GoOnMarketLocation()
    {
        if (_IsAnimated) return;
        OnSplineAnimationRequest((int)CamSplineType.MainViewToMarket);
    }

    public void GoOnCourtyardLocation()
    {
        if (_IsAnimated) return;
        OnSplineAnimationRequest((int)CamSplineType.MainViewToCourtyard);
    }

    public void ProceedLookAtRequest()
    {
        _CineCam.Follow = _LookAt[_SplineID].transform;
        return;
    }

    #endregion

    #region Events
    public void OnCamSplineCompleted()
    {
        Debug.Log("[CamManager]: OnCamSplineCompleted");
         
        _SplineCameraAnimator.enabled = false;
        _IsAnimated = false;

        OnCamLocationChanged.Invoke((CamSplineType)_SplineID);

        if ((CamSplineType)_SplineID ==  CamSplineType.MainViewToCourtyard || (CamSplineType)_SplineID == CamSplineType.MarketToCourtyard)
        {
            //ShopManager.Instance.OpenCourtyard();
            // TODO: Reimplement the courtyard camera movement
        }
        else if ((CamSplineType)_SplineID == CamSplineType.MainViewToMarket || (CamSplineType)_SplineID == CamSplineType.CourtyardToMarket)
        {
            //ShopManager.Instance.OpenMarket();
            // TODO: Reimplement the market camera movement
        }
    }

    public void OnCamSplineStart()
    {
        Debug.Log("[CamManager]: OnCamSplineStart");

        OnCamLocationIsChanging.Invoke((CamSplineType)_SplineID);
    }
    #endregion
}
