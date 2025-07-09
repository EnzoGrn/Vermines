using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vermines.Plugin.Sounds;

public class TooggleSound : MonoBehaviour
{
    [SerializeField] private List<AudioClip> _clipList;
    [SerializeField] private float _minDistance;
    [SerializeField] private float _maxDistance;

    private float _timeToWait = 0;
    private bool _isPlayingSound = false;

    private void Update()
    {
        if (!_isPlayingSound)
        {
            PlayNextSound();
        }
    }

    private void PlayNextSound()
    {
        if (_clipList == null || _clipList.Count == 0)
            return;
        UserMixerPlugin mixerManager = FindFirstObjectByType<UserMixerPlugin>(FindObjectsInactive.Include);

        int randomIndex = Random.Range(0, _clipList.Count);
        AudioClip clip = _clipList[randomIndex];

        SoundManager.Instance.PlaySoundFXClip(clip, gameObject.transform, mixerManager.FXVolume, true, _minDistance, _maxDistance);

        _timeToWait = clip.length;
        _isPlayingSound = true;

        StartCoroutine(SoundDone());
    }

    private IEnumerator SoundDone()
    {
        yield return new WaitForSeconds(_timeToWait + Random.Range(3f, 10f));

        _timeToWait = 0;
        _isPlayingSound = false;
    }
}
