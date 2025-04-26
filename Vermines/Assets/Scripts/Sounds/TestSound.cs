using System.Collections.Generic;
using UnityEngine;

public class TestSound : MonoBehaviour
{
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
            SoundManager.Instance.PlayRandomSoundFXClip(_testOneShotSoundFXList, transform, 1f);
        }
    }
}
