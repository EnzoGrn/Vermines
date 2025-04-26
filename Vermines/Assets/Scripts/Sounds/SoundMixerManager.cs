using log4net.Core;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;

    private void SetVolume(string parameterName, float level)
    {
        if (level <= 0f)
            level = 0.0001f; // Avoid Mathf.Log10(0) error

        _audioMixer.SetFloat(parameterName, Mathf.Log10(level) * 20f);
    }

    public void SetMasterVolume(float level)
    {
        SetVolume("MasterVolume", level);
        PlayerPrefs.SetFloat("MasterVolume", level);
    }

    public void SetSoundFXVolume(float level)
    {
        SetVolume("SoundFXVolume", level);
        PlayerPrefs.SetFloat("SoundFXVolume", level);
    }

    public void SetAtmosphereVolume(float level)
    {
        SetVolume("AtmosphereVolume", level);
        PlayerPrefs.SetFloat("AtmosphereVolume", level);
    }

    public void SetMusicVolume(float level)
    {
        SetVolume("MusicVolume", level);
        PlayerPrefs.SetFloat("MusicVolume", level);
    }
}
