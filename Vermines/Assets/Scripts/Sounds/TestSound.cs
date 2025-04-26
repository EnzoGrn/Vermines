using System.Collections.Generic;
using UnityEngine;

public class TestSound : MonoBehaviour
{
    [SerializeField] private AudioSource _loopingMusicSource;
    [SerializeField] private AudioClip _testLoopSound;
    [SerializeField] private List<AudioClip> _testLoopSoundList;
    [SerializeField] private AudioClip _testOneShotSoundFX;
    [SerializeField] private List<AudioClip> _testOneShotSoundFXList;

    private void Update()
    {
        // Test unique sound
        if (Input.GetKeyDown(KeyCode.Y))
        {
            SoundManager.Instance.PlaySoundFXClip(_testOneShotSoundFX, transform, 1f);
        }

        // Test random sound
        if (Input.GetKeyDown(KeyCode.U))
        {
            SoundManager.Instance.PlaySoundFXClip(_testOneShotSoundFXList, transform, 1f);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            SoundManager.Instance.ChangeSmoothlyLoopingSound(_loopingMusicSource, _testLoopSound, 1f);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            SoundManager.Instance.ChangeSmoothlyLoopingSound(_loopingMusicSource, _testLoopSoundList, 1f);
        }
    }
}
