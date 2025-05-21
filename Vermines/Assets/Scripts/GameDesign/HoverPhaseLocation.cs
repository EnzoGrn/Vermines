using System.Collections.Generic;
using UnityEngine;
using Vermines.GameDesign.Initializer;

public class HoverPhaseLocation : IGGD_Behaviour {
    #region Exposed Fields
    [SerializeField] private CamSplineType _CamSplineType;
    [SerializeField] private Material _OutlineMaterial;
    [SerializeField] private List<MeshRenderer> _MeshRendererList;
    [SerializeField] private Color _OutlineColor = new (0f, 1f, 0f, 1f); // Green default color
    [SerializeField] private List<HoverPhaseLocation> _Locations = new();
    #endregion

    #region Private Fields
    private bool _CanClickLocations = true;
    private bool _CanHoverLocations = true;
    private Material _InstanceOfMaterial;
    private MaterialPropertyBlock _PropBlock;

    private Color _TransparentColor = new(0f, 1f, 0f, 0f); // Transparent color
    #endregion

    CamManager _CameraManager;

    public override void Init()
    {
        if (_CameraManager == null)
            _CameraManager = FindAnyObjectByType<CamManager>(FindObjectsInactive.Include);

        // TODO: Called the Init() methods when launch a game.
        if (_CameraManager) {
            _InstanceOfMaterial = new Material(_OutlineMaterial);
            _PropBlock = new MaterialPropertyBlock();

            foreach (MeshRenderer r in _MeshRendererList) {
                // Get the current list of materials
                Material[] materials = r.materials;

                // Create a new array with one more slot
                Material[] newMaterials = new Material[materials.Length + 1];

                // Copy the existing materials to the new array
                materials.CopyTo(newMaterials, 0);

                // Add the new material to the last slot in the new array
                newMaterials[materials.Length] = _InstanceOfMaterial;

                // Assign the new material array to the MeshRenderer
                r.materials = newMaterials;
            }

            ApplyOutline(false);

            //CamManager.Instance.OnCamLocationIsChanging.AddListener(OnCamNotOnLocation);
            CamManager.Instance.OnCamLocationChanged.AddListener(OnCamAnimationCompleted);
            CamManager.Instance.OnCamLocationIsChanging.AddListener(OnCamNotOnLocation);
        }
    }

    private void OnMouseEnter()
    {
        if (_CameraManager == null)
            return;
        if (_CanHoverLocations)
            ApplyOutline(true);
        foreach(HoverPhaseLocation location in _Locations)
            location.ApplyOutline(false);
    }

    private void OnMouseExit()
    {
        if (_CameraManager == null)
            return;
        if (_CanHoverLocations)
            ApplyOutline(false);
    }

    private void OnMouseDown()
    {
        if (_CameraManager == null)
            return;
        if (!_CanHoverLocations || !_CanClickLocations)
            return;        
        _CanHoverLocations = false;

        ApplyOutline(false);

        CamManager.Instance.OnSplineAnimationRequest((int)_CamSplineType);
    }

    // TODO: set to false to avoid over when traveling with the event is changing
    private void OnCamNotOnLocation(CamSplineType camSplineType)
    {
        _CanHoverLocations = false;
    }

    private void OnCamAnimationCompleted(CamSplineType camSplineType)
    {
        _CanClickLocations = (camSplineType == CamSplineType.None);
        _CanHoverLocations = (camSplineType == CamSplineType.None);
    }

    public void ApplyOutline(bool enable)
    {
        foreach (MeshRenderer r in _MeshRendererList) {
            r.GetPropertyBlock(_PropBlock);
            _PropBlock.SetColor("_OutlineColor", enable ? _OutlineColor : _TransparentColor);
            r.SetPropertyBlock(_PropBlock);
        }
    }
}
