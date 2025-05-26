using UnityEngine;

namespace Vermines.Sound {

    public class MusicManager : MonoBehaviour {

        public AudioSource MusicSource;

        public void Play()
        {
            MusicSource.Play();
        }
    }
}
