using UnityEngine;
using UnityEngine.Splines;
using Unity.Cinemachine;
using System.Collections.Generic;
using OMGG.DesignPattern;
using UnityEngine.Events;
using Vermines.UI;

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

    // This value is used to go on special location from the none a market
    // If different of 0 must be a special location to go after completed first cam animation to return in the none location
    private int _GoOnSpecialLocation = 0;

    private RoutineManager _routineManager;

    #endregion

    #region Methods

    private void Start()
    {
        _CinemachineSplineDolly = _CineCam.GetComponent<CinemachineSplineDolly>();

        if (_CinemachineSplineDolly == null )
        {
            Debug.Log("[CamManager]: Cannot find _CinemachineSplineDolly");
        }

        _routineManager = FindFirstObjectByType<RoutineManager>();

        if (!_routineManager)
        {
            Debug.LogError("[CamManager]: Cannot find RoutineManager in the scene, please add it to the scene.");
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
            _Offset = splineID - (int)CamSplineType.MainViewToSacrifice + ((int)CamSplineType.MainViewToMarket - splineID);

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

    public void GoOnSacrificeLocation()
    {
        if (_IsAnimated) return;

        // Check if current location is not none, if it is just go the the sacrifice location else return to none before going to sacrifice

        if (_SplineID != 0)
        {
            OnSplineAnimationRequest((int)CamSplineType.None);
            _GoOnSpecialLocation = (int)CamSplineType.MainViewToSacrifice;
        }
        else
        {
            OnSplineAnimationRequest((int)CamSplineType.MainViewToSacrifice);
        }
    }

    public void GoOnMarketLocation()
    {
        if (_IsAnimated) return;

        if (_SplineID == (int)CamSplineType.MainViewToSacrifice)
        {
            OnSplineAnimationRequest((int)CamSplineType.None);
            _GoOnSpecialLocation = (int)CamSplineType.MainViewToMarket;
        }
        else
        {
            OnSplineAnimationRequest((int)CamSplineType.MainViewToMarket);
        }
    }

    public void GoOnCourtyardLocation()
    {
        if (_IsAnimated) return;
        
        if (_SplineID == (int)CamSplineType.MainViewToSacrifice)
        {
            OnSplineAnimationRequest((int)CamSplineType.None);
            _GoOnSpecialLocation = (int)CamSplineType.MainViewToCourtyard;
        }
        else
        {
            OnSplineAnimationRequest((int)CamSplineType.MainViewToCourtyard);
        }
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

        if (_GoOnSpecialLocation != 0)
        {
            OnSplineAnimationRequest(_GoOnSpecialLocation);
            _GoOnSpecialLocation = (int)CamSplineType.None;
            Debug.Log("[CamManager]: Resume Npc Routine now");
            _routineManager.ResumeNpcRoutine();
        }

        if (!UIManager.Instance)
            return;

        Debug.Log($"[CamManager]: Check the current spline type {((CamSplineType)_SplineID).ToString()}");

        switch ((CamSplineType)_SplineID)
        {
            case CamSplineType.None:
                _routineManager.ResumeNpcRoutine();
                Debug.Log("[CamManager]: Resume Npc Routine now");
                break;
            case CamSplineType.MainViewToCourtyard:
                UIManager.Instance.OpenShopCourtyard();
                _routineManager.InterruptNpcRoutine();
                Debug.Log("[CamManager]: Interrupt Npc Routine now");
                break;
            case CamSplineType.MarketToCourtyard:
                UIManager.Instance.OpenShopCourtyard();
                _routineManager.InterruptNpcRoutine();
                Debug.Log("[CamManager]: Interrupt Npc Routine now");
                break;
            case CamSplineType.MainViewToMarket:
                UIManager.Instance.OpenShopMarket();
                _routineManager.ResumeNpcRoutine();
                break;
            case CamSplineType.CourtyardToMarket:
                UIManager.Instance.OpenShopMarket();
                _routineManager.ResumeNpcRoutine();
                Debug.Log("[CamManager]: Resume Npc Routine now");
                break;
            case CamSplineType.MainViewToSacrifice:
                UIManager.Instance.OpenTable();
                _routineManager.ResumeNpcRoutine();
                break;
        }
    }

    public void OnCamSplineStart()
    {
        Debug.Log("[CamManager]: OnCamSplineStart");

        OnCamLocationIsChanging.Invoke((CamSplineType)_SplineID);
    }
    #endregion
}
