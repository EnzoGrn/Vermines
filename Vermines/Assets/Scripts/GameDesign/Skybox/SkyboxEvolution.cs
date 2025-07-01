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
        //InitSkyboxSettings();
    }

    void OnDestroy()
    {
        StopAllCoroutines();

        ResetSkybox();

        //RenderSettings.skybox = OriginalSkyboxMaterial;
        //RenderSettings.skybox = new Material(OriginalSkyboxMaterial);

        GameEvents.OnPlayerWin.RemoveListener(OnPlayerWin);
        GameEvents.OnPlayersUpdated.RemoveListener(UpdateSkybox);
    }

    /// <summary>
    /// Save the default config of the skybox
    /// </summary>
    public void InitSkyboxSettings()
    {
        if (DirectionalLight == null || OriginalSkyboxMaterial == null)
        {
            Debug.LogWarning("[InitSkyboxSettings] A reference is null.");
            return;
        }

        if (_skyboxMaterial == null)
        {
            // Instantiate a new material to avoid modifying the original one
            Debug.Log("[InitSkyboxSettings] Creating a new skybox material from the original one.");
            _skyboxMaterial = new Material(OriginalSkyboxMaterial);
            RenderSettings.skybox = _skyboxMaterial;
        }

        _originalSkyboxTint = _skyboxMaterial.GetColor("_Tint");
        _originalExposure = _skyboxMaterial.GetFloat("_Exposure");
        _originalDLCOlor = DirectionalLight.color;
        _originalIntensity = DirectionalLight.intensity;

        GameEvents.OnPlayersUpdated.AddListener(UpdateSkybox);
        GameEvents.OnPlayerWin.AddListener(OnPlayerWin);

        DynamicGI.UpdateEnvironment();

        Debug.Log($"[InitSkyboxSettings]: Original data, sky tint {_originalSkyboxTint}, sky sxposure {_originalExposure}\n" +
            $"dl color {DirectionalLight.color}, dl intensity {DirectionalLight.intensity}");
    }

    /// <summary>
    /// Get the max souls value
    /// </summary>
    /// <param name="playerData"></param>
    /// <returns></returns>
    private int GetMaxSoulsValue(NetworkDictionary<PlayerRef, PlayerData> playerData)
    {
        int maxSouls = 0;

        foreach (var key in playerData)
        {
            maxSouls = Mathf.Max(maxSouls, key.Value.Souls);
        }

        return maxSouls;
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

        Debug.Log("[ResetSkybox] Resetting skybox to original settings.");
        Debug.Log($"[ResetSkybox] Original data, sky tint {_originalSkyboxTint}, sky exposure {_originalExposure}\n" +
            $"dl color {DirectionalLight.color}, dl intensity {DirectionalLight.intensity}");

        //_skyboxMaterial.SetColor("_Tint", _originalSkyboxTint);
        //_skyboxMaterial.SetFloat("_Exposure", _originalExposure);
        RenderSettings.fogColor = FromColorFog;

        RenderSettings.skybox.SetColor("_Tint", _originalSkyboxTint);
        RenderSettings.skybox.SetFloat("_Exposure", _originalExposure);

        DirectionalLight.color = _originalDLCOlor;
        DirectionalLight.intensity = _originalIntensity;

        DynamicGI.UpdateEnvironment();
    }

    /// <summary>
    /// Call ResetSkybox method when a player win
    /// </summary>
    /// <param name="winnerRef"></param>
    /// <param name="localPlayerRef"></param>
    public void OnPlayerWin(PlayerRef winnerRef, PlayerRef localPlayerRef)
    {
        GameEvents.OnPlayerWin.RemoveListener(OnPlayerWin);
        GameEvents.OnPlayersUpdated.RemoveListener(UpdateSkybox);
        
        StopAllCoroutines();

        StartCoroutine(DelayedResetSkybox());
        //ResetSkybox();
    }

    private IEnumerator DelayedResetSkybox()
    {
        // Laisse un frame à Unity pour terminer toute mise à jour du rendu en cours
        yield return null;

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
