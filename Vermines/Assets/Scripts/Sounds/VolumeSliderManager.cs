using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderManager : MonoBehaviour
{
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _soundFXSlider;
    [SerializeField] private Slider _atmosphereSlider;
    [SerializeField] private Slider _musicSlider;

    private void Start()
    {
        // Load saved values and set sliders
        _masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        _soundFXSlider.value = PlayerPrefs.GetFloat("SoundFXVolume", 1f);
        _atmosphereSlider.value = PlayerPrefs.GetFloat("AtmosphereVolume", 1f);
        _musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
    }
}
