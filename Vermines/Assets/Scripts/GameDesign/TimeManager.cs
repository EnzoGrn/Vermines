using TMPro;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _TimeText;

    [SerializeField] private Light _Moon;
    [SerializeField] private AnimationCurve _LightIntensityCurve;
    [SerializeField] float _MaxMoonIntensity = 35f;
    [SerializeField] private Color _NightAmbientLight;
    [SerializeField] private Volume _Volume;

    public float MaxSouls = 100;
    public float BestPlayerSoul = 0;

    private ColorAdjustments _ColorAdjustments;
    private WhiteBalance _WhiteBalance;

    private void Start()
    {
        if (_Volume.profile.TryGet(out _ColorAdjustments) == false)
        {
            Debug.LogError("Color Adjustments not found in Volume Profile.");
        }

        if (_Volume.profile.TryGet(out _WhiteBalance) == false)
        {
            Debug.LogError("White Balance not found in Volume Profile.");
        }
    }

    private void Update()
    {
        UpdateLightSettings();
        UpdateColorTemperature();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (BestPlayerSoul + 2 <= MaxSouls)
                BestPlayerSoul += 2;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (BestPlayerSoul - 2 >= 0)
                BestPlayerSoul -= 2;
        }
    }

    private void UpdateLightSettings()
    {
        float ratio = BestPlayerSoul / MaxSouls;

        Debug.Log("Ratio -> " + ratio.ToString() + ", MaxSouls " + MaxSouls + ", BestPlayerSoul " + BestPlayerSoul);

        float lightItensityCurve = _LightIntensityCurve.Evaluate(ratio);

        Debug.Log("lightItensityCurve -> " + lightItensityCurve);

        _Moon.intensity = Mathf.Lerp(0, _MaxMoonIntensity, lightItensityCurve);

        if (_TimeText != null)
        {
            _TimeText.text = BestPlayerSoul.ToString();
        }
    }

    private void UpdateColorTemperature()
    {
        float ratio = BestPlayerSoul / MaxSouls;

        // Adjust Color Temperature (White Balance)
        if (_WhiteBalance != null)
        {
            _WhiteBalance.temperature.value = Mathf.Lerp(-10, 100, ratio); // Warmer color as ratio increases
            _WhiteBalance.tint.value = Mathf.Lerp(0, 30, ratio); // Slightly redder tint
        }

        // Adjust Color Filter (Color Adjustments)
        if (_ColorAdjustments != null)
        {
            Color warmColor = Color.Lerp(Color.white, new Color(1.0f, 0.5f, 0.2f), ratio); // White to Orange
            _ColorAdjustments.colorFilter.value = warmColor;
        }
    }
}
