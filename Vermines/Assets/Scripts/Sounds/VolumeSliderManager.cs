using UnityEngine;
using UnityEngine.UI;
using Vermines.Plugin.Sounds;

public class VolumeSliderManager : MonoBehaviour
{
    [SerializeField]
    private Slider _MasterSlider;
    
    [SerializeField]
    private Slider _SoundFXSlider;
        
    [SerializeField]
    private Slider _MusicSlider;

    private void Start()
    {
        UserMixerPlugin mixer = FindAnyObjectByType<UserMixerPlugin>(FindObjectsInactive.Include);

        if (mixer == null) {
            Debug.LogError("[VolumeSliderManager]: There is no 'UserMixerPlugin' in the scene. Cannot be able to set the volume sliders.");

            return;
        }

        _MasterSlider.value  = mixer.MainVolume;
        _SoundFXSlider.value = mixer.FXVolume;
        _MusicSlider.value   = mixer.MusicVolume;
    }
}
