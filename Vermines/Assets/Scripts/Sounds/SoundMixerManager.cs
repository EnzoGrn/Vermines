using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;

    public void SetMasterVolume(float level)
    {
        // Linear interpolation of the volume instead of logarithmic one
        _audioMixer.SetFloat("MasterVolume", Mathf.Log10(level) * 20f);
    }

    public void SetSoundFXVolume(float level)
    {
        // Linear interpolation of the volume instead of logarithmic one
        _audioMixer.SetFloat("SoundFXVolume", Mathf.Log10(level) * 20f);
    }

    public void SetAtmosphereVolume(float level)
    {
        // Linear interpolation of the volume instead of logarithmic one
        _audioMixer.SetFloat("AtmosphereVolume", Mathf.Log10(level) * 20f);
    }

    public void SetMusicVolume(float level)
    {
        // Linear interpolation of the volume instead of logarithmic one
        _audioMixer.SetFloat("MusicVolume", Mathf.Log10(level) * 20f);
    }
}
