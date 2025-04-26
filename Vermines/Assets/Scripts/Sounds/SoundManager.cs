using UnityEngine;
using OMGG.DesignPattern;
using System.Collections.Generic;

public class SoundManager : MonoBehaviourSingleton<SoundManager>
{
    [SerializeField] private AudioSource _soundFXObjectPrefab;

    public void PlaySoundFXClip(AudioClip audioclip, Transform transform, float volume)
    {
        // Spawn the gameobject
        AudioSource audioSource = Instantiate(_soundFXObjectPrefab, transform.position, Quaternion.identity);

        audioSource.clip = audioclip;
        audioSource.volume = volume;
        audioSource.Play();

        float timeBeforeDestroy = audioSource.clip.length;

        Debug.Log("timeBeforeDestroy: " + timeBeforeDestroy);

        Destroy(audioSource.gameObject, timeBeforeDestroy);
    }

    public void PlayRandomSoundFXClip(List<AudioClip> audioclipList, Transform transform, float volume)
    {
        // Spawn the gameobject
        AudioSource audioSource = Instantiate(_soundFXObjectPrefab, transform.position, Quaternion.identity);

        audioSource.clip = audioclipList[Random.Range(0, audioclipList.Count)];
        audioSource.volume = volume;
        audioSource.Play();

        float timeBeforeDestroy = audioSource.clip.length;

        Debug.Log("timeBeforeDestroy: " + timeBeforeDestroy);

        Destroy(audioSource.gameObject, audioSource.clip.length);
    }
}
