using UnityEngine;
using OMGG.DesignPattern;
using System.Collections.Generic;
using System.Collections;

public class SoundManager : MonoBehaviourSingleton<SoundManager>
{
    [SerializeField] private AudioSource _soundFXObjectPrefab;
    [SerializeField] private AudioSource _3dSoundFXObjectPrefab;
    [SerializeField] private float _fadeDuration;

    public void PlaySoundFXClip(AudioClip audioclip, Transform transform, float volume, bool is3dSound,
                                float minDistance = 0f, float maxDistance = 15f)
    {
        // Spawn the gameobject
        AudioSource audioSource;

        if (!is3dSound)
            audioSource = Instantiate(_soundFXObjectPrefab, transform.position, Quaternion.identity);
        else
        {
            audioSource = Instantiate(_3dSoundFXObjectPrefab, transform.position, Quaternion.identity);
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
        }

        audioSource.clip = audioclip;
        audioSource.volume = volume;
        audioSource.Play();

        Destroy(audioSource.gameObject, audioSource.clip.length);
    }

    public void PlaySoundFXClip(List<AudioClip> audioclipList, Transform transform, float volume, bool is3dSound,
                                float minDistance = 0f, float maxDistance = 15f)
    {
        if (audioclipList == null || audioclipList.Count == 0)
        {
            Debug.LogWarning("AudioClip list is empty!");
            return;
        }

        AudioSource audioSource;

        // Spawn the gameobject
        if (!is3dSound)
            audioSource = Instantiate(_soundFXObjectPrefab, transform.position, Quaternion.identity);
        else
        {
            audioSource = Instantiate(_3dSoundFXObjectPrefab, transform.position, Quaternion.identity);
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
        }

        audioSource.clip = audioclipList[Random.Range(0, audioclipList.Count)];
        audioSource.volume = volume;
        audioSource.Play();

        Destroy(audioSource.gameObject, audioSource.clip.length);
    }

    public void ChangeSmoothlyLoopingSound(AudioSource audioSource, AudioClip audioclip, float volume)
    {
        StartCoroutine(ChangeSoundCoroutine(audioSource, audioclip, volume));
    }

    public void ChangeSmoothlyLoopingSound(AudioSource audioSource, List<AudioClip> audioclipList, float volume)
    {
        if (audioclipList == null || audioclipList.Count == 0)
        {
            Debug.LogWarning("AudioClip list is empty!");
            return;
        }

        // Pick a random clip (you could change this behavior if needed)
        AudioClip randomClip = audioclipList[Random.Range(0, audioclipList.Count)];
        StartCoroutine(ChangeSoundCoroutine(audioSource, randomClip, volume));
    }

    private IEnumerator ChangeSoundCoroutine(AudioSource audioSource, AudioClip newClip, float targetVolume)
    {
        float startVolume = audioSource.volume;

        // Fade Out
        for (float t = 0; t < _fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / _fadeDuration);
            yield return null;
        }
        audioSource.volume = 0f;
        
        // Change the clip
        audioSource.clip = newClip;
        audioSource.loop = true;
        audioSource.Play();

        // Fade In
        for (float t = 0; t < _fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, targetVolume, t / _fadeDuration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }
}
