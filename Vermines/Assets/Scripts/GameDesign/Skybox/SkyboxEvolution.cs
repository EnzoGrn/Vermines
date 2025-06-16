using Fusion;
using System.Collections;
using UnityEngine;
using Vermines;
using Vermines.Player;

public class SkyboxEvolution : MonoBehaviour
{
    #region Public Properties
    [Header("References")]
    public Light DirectionalLight;
    public Material OriginalSkyboxMaterial;

    [Header("Target Values")]
    public Color ToColorSkyboxTint;
    public Color ToColorFog;
    public Color FromColorFog;
    public Color ToColorDL;
    public float ToIntensityDL;
    public float ToExposure;
    public float Duration;
    #endregion

    #region Private Properties
    private Material _skyboxMaterial;
    private Color _originalSkyboxTint;
    private Color _originalDLCOlor;
    private float _originalExposure;
    private float _originalIntensity;

    private int _actualMaxSouls = 0;
    private float _currentValue = 0.01f;
    private Coroutine _skyboxTransitionCoroutine;
    #endregion

    #region methods

    void Start()
    {
        InitSkyboxSettings();
    }

    void OnDestroy()
    {
        ResetSkybox();

        GameEvents.OnPlayerWin.RemoveListener(OnPlayerWin);
        GameEvents.OnPlayersUpdated.RemoveListener(UpdateSkybox);
    }

    /// <summary>
    /// Save the default config of the skybox
    /// </summary>
    private void InitSkyboxSettings()
    {
        if (DirectionalLight == null || OriginalSkyboxMaterial == null)
        {
            Debug.LogWarning("[InitSkyboxSettings] A reference is null.");
            return;
        }

        // Instantiate a new material to avoid modifying the original one
        _skyboxMaterial = new Material(OriginalSkyboxMaterial);
        RenderSettings.skybox = _skyboxMaterial;

        _originalSkyboxTint = OriginalSkyboxMaterial.GetColor("_Tint");
        _originalExposure = OriginalSkyboxMaterial.GetFloat("_Exposure");
        Debug.Log($"[SkyboxEvolution] Original Fog Color: {RenderSettings.fogColor}");
        _originalDLCOlor = DirectionalLight.color;
        _originalIntensity = DirectionalLight.intensity;

        GameEvents.OnPlayersUpdated.AddListener(UpdateSkybox);
        GameEvents.OnPlayerWin.AddListener(OnPlayerWin);

        //if (OriginalSkyboxMaterial.HasProperty("_Tint"))
        //{
        //    _originalSkyboxTint = OriginalSkyboxMaterial.GetColor("_Tint");
        //}
        //else
        //{
        //    Debug.LogWarning("[SkyboxEvolution] Skybox shader does not have a '_Tint' property.");
        //}

        //if (OriginalSkyboxMaterial.HasProperty("_Exposure"))
        //{
        //    _originalExposure = OriginalSkyboxMaterial.GetFloat("_Exposure");
        //}
        //else
        //{
        //    Debug.LogWarning("[SkyboxEvolution] Skybox shader does not have an '_Exposure' property.");
        //}
    }

    /// <summary>
    /// Get the max souls value
    /// </summary>
    /// <param name="playerData"></param>
    /// <returns></returns>
    private int GetMaxSoulsValue(NetworkDictionary<PlayerRef, PlayerData> playerData)
    {
        int maxSlous = 0;

        foreach (var key in playerData)
        {
            maxSlous = Mathf.Max(maxSlous, key.Value.Souls);
        }

        return maxSlous;
    }

    /// <summary>
    /// Reset skybox default config
    /// </summary>
    private void ResetSkybox()
    {
        if (DirectionalLight == null || _skyboxMaterial == null)
        {
            Debug.LogWarning("[ResetSkybox] A reference is null.");
            return;
        }

        _skyboxMaterial.SetColor("_Tint", _originalSkyboxTint);
        _skyboxMaterial.SetFloat("_Exposure", _originalExposure);
        RenderSettings.fogColor = FromColorFog;

        //RenderSettings.skybox.SetColor("_Tint", _originalSkyboxTint);
        //RenderSettings.skybox.SetFloat("_Exposure", _originalExposure);

        DirectionalLight.color = _originalDLCOlor;
        DirectionalLight.intensity = _originalIntensity;
    }

    /// <summary>
    /// Call ResetSkybox method when a player win
    /// </summary>
    /// <param name="winnerRef"></param>
    /// <param name="localPlayerRef"></param>
    public void OnPlayerWin(PlayerRef winnerRef, PlayerRef localPlayerRef)
    {
        ResetSkybox();
    }

    /// <summary>
    /// UpdateSkybox change the look of the skybox depending on the actual higher souls value from 
    /// max souls player value to max require souls value to win
    /// </summary>
    public void UpdateSkybox(NetworkDictionary<PlayerRef, PlayerData> playerData)
    {
        int maxSouls = GetMaxSoulsValue(playerData);
        
        if (maxSouls == _actualMaxSouls)
            return;

        _actualMaxSouls = maxSouls;
        float value = Mathf.Clamp01((float)_actualMaxSouls / (float)GameManager.Instance.Configuration.MaxSoul);

        if (_skyboxTransitionCoroutine != null)
            StopCoroutine(_skyboxTransitionCoroutine);

        _skyboxTransitionCoroutine = StartCoroutine(SmoothSkyboxTransition(_currentValue, value, Duration));
        _currentValue = value;
    }

    /// <summary>
    /// Apply the progressive skybox config
    /// </summary>
    /// <param name="value"></param>
    private void ApplySkyboxConfig(float value)
    {
        // Lerp values
        float exposure = Mathf.Lerp(_originalExposure, ToExposure, value);
        float intensity = Mathf.Lerp(_originalIntensity, ToIntensityDL, value);
        Color skyboxTint = Color.Lerp(_originalSkyboxTint, ToColorSkyboxTint, value);
        Color fogColor = Color.Lerp(FromColorFog, ToColorFog, value);
        Color lightColor = Color.Lerp(_originalDLCOlor, ToColorDL, value);

        // Apply values
        RenderSettings.skybox.SetFloat("_Exposure", exposure);
        RenderSettings.skybox.SetColor("_Tint", skyboxTint);
        RenderSettings.fogColor = fogColor;
        DirectionalLight.color = lightColor;
        DirectionalLight.intensity = intensity;

        DynamicGI.UpdateEnvironment(); // For real-time reflection updates
    }
    #endregion

    #region Coroutines

    /// <summary>
    /// Progressively update the skybox config 
    /// </summary>
    /// <param name="fromValue"></param>
    /// <param name="toValue"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator SmoothSkyboxTransition(float fromValue, float toValue, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float lerpedValue = Mathf.Lerp(fromValue, toValue, t);
            ApplySkyboxConfig(lerpedValue);

            yield return null;
        }

        ApplySkyboxConfig(toValue); // assurer état final exact
    }
    #endregion
}
