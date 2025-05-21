using UnityEngine;

public class SkyboxEvolution : MonoBehaviour
{
    [Header("References")]
    public Light DirectionalLight;

    [Header("Target Values")]
    public Color ToColorSkyboxTint;
    public Color ToColorFog;
    public Color ToColorDL;
    public float ToIntensityDL;
    public float ToExposure;
    public float Duration;

    private Color _originalSkyboxTint;
    private Color _originalFogColor;
    private Color _originalDLCOlor;
    private float _originalExposure;
    private float _originalIntensity;
    private float _timer = 0f;

    void Start()
    {
        if (RenderSettings.skybox != null)
        {
            _originalSkyboxTint = RenderSettings.skybox.GetColor("_Tint");
            _originalExposure = RenderSettings.skybox.GetFloat("_Exposure");
            _originalFogColor = RenderSettings.fogColor;
        }

        if (DirectionalLight != null)
        {
            _originalDLCOlor = DirectionalLight.color;
            _originalIntensity = DirectionalLight.intensity;
        }
    }

    void Update()
    {
        if (RenderSettings.skybox == null || !RenderSettings.skybox.HasProperty("_Tint") || !RenderSettings.skybox.HasProperty("_Exposure"))
            return;

        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / Duration);

        // Lerp values
        float exposure = Mathf.Lerp(_originalExposure, ToExposure, t);
        float intensity = Mathf.Lerp(_originalIntensity, ToIntensityDL, t);
        Color skyboxTint = Color.Lerp(_originalSkyboxTint, ToColorSkyboxTint, t);
        Color fogColor = Color.Lerp(_originalFogColor, ToColorFog, t);
        Color lightColor = Color.Lerp(_originalDLCOlor, ToColorDL, t);

        // Apply values
        RenderSettings.skybox.SetFloat("_Exposure", exposure);
        RenderSettings.skybox.SetColor("_Tint", skyboxTint);
        RenderSettings.fogColor = fogColor;

        if (DirectionalLight != null)
        {
            DirectionalLight.color = lightColor;
            DirectionalLight.intensity = intensity;
        }

        DynamicGI.UpdateEnvironment(); // For real-time reflection updates
    }

    void OnDestroy()
    {
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetColor("_Tint", _originalSkyboxTint);
            RenderSettings.fogColor = _originalFogColor;
            RenderSettings.skybox.SetFloat("_Exposure", _originalExposure);
        }

        if (DirectionalLight != null)
        {
            DirectionalLight.color = _originalDLCOlor;
            DirectionalLight.intensity = _originalIntensity;
        }
    }
}
