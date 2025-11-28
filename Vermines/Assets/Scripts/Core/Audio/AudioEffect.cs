using System.Collections;
using UnityEngine;

namespace Vermines.Core.Audio {

    using Vermines.Extension;

    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioEffect : MonoBehaviour {

        #region Attributes

        public int LastPlayedClipIndex { get; set; }

        public float BasePitch { get; set; }

        public AudioSetup DefaultSetup => _DefaultSetup;
        public AudioSetup CurrentSetup => _CurrentSetup;
        public AudioSource AudioSource => _AudioSource;

        public bool IsPlaying => _AudioSource.isPlaying == true || _DelayedPlayRoutine != null;

        [SerializeField]
        private AudioSetup _DefaultSetup;

        private AudioSource _AudioSource;

        private bool _PlayOnAwake;

        private int _PlayCount;

        private Coroutine _DelayedPlayRoutine;

        private AudioSetup _CurrentSetup;

        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            _AudioSource = GetComponent<AudioSource>();

            BasePitch = _AudioSource.pitch;

            _PlayOnAwake = _AudioSource.playOnAwake;

            _AudioSource.playOnAwake = false;
            _AudioSource.Stop();

            _DefaultSetup.Loop |= _AudioSource.loop;

            if (_DefaultSetup.Clips.Length == 0 && _AudioSource.clip != null) {
                _DefaultSetup.Clips = new AudioClip[] {
                    _AudioSource.clip
                };
            }
        }

        private void OnEnable()
        {
            _AudioSource.enabled = true;

            if (_PlayOnAwake)
                Play();
        }

        private void OnDisable()
        {
            StopDelayedPlay();

            _AudioSource.enabled = false;
        }

        #endregion

        #region Methods

        public void Play(ForceBehaviour force = ForceBehaviour.None)
        {
            Play(_DefaultSetup, force);
        }

        public void Play(AudioSetup setup, ForceBehaviour force = ForceBehaviour.None)
        {
            if (IsPlaying) {
                if (force == ForceBehaviour.None)
                    return;
                if (force == ForceBehaviour.ForceDifferentSetup && setup == _CurrentSetup)
                    return;

                if (force == ForceBehaviour.ForceSameSetup && setup != _CurrentSetup)
                    return;
            }

            if (setup.Clips == null || setup.Clips.Length == 0)
                return;
            StartPlay(setup);
        }

        public void Stop(bool forceImmediateStop = false)
        {
            StopDelayedPlay();

            if (!forceImmediateStop && _CurrentSetup != null && _CurrentSetup.FadeOut > 0f)
                _AudioSource.FadeOut(this, _CurrentSetup.FadeOut);
            else
                _AudioSource.Stop();
        }

        private void StartPlay(AudioSetup setup)
        {
            AudioSetup previousSetup = _CurrentSetup;

            _CurrentSetup = setup;

            LastPlayedClipIndex = NextClipIndex(setup);

            if (LastPlayedClipIndex < 0)
                return;
            if (_CurrentSetup.Clips[LastPlayedClipIndex] == null)
                return; // Do not start playing if there will be nothing to play
            StopDelayedPlay();

            _PlayCount = 0;

            bool waitForFadeOut = IsPlaying && previousSetup != null && previousSetup.FadeOut > 0.01f;

            if (_CurrentSetup.Delay < 0.01f && !waitForFadeOut)
                PlayClip(LastPlayedClipIndex);
            else {
                float delay = _CurrentSetup.Delay;

                if (waitForFadeOut) {
                    delay += previousSetup.FadeOut;

                    _AudioSource.FadeOut(this, previousSetup.FadeOut);
                }

                _DelayedPlayRoutine = StartCoroutine(PlayDelayed_Coroutine(delay, LastPlayedClipIndex));
            }
        }

        private void PlayClip(int clipIndex)
        {
            _AudioSource.Stop();

            StopAllCoroutines(); // Stop audiosource fadings

            LastPlayedClipIndex = clipIndex;

            _AudioSource.clip   = _CurrentSetup.Clips[clipIndex];
            _AudioSource.volume = _CurrentSetup.Volume;
            _AudioSource.loop   = _CurrentSetup.Loop;
            _AudioSource.pitch  = BasePitch + _CurrentSetup.PitchShift + Random.Range(-_CurrentSetup.MaxPitchChange, _CurrentSetup.MaxPitchChange);

            if (_CurrentSetup.FadeIn > 0f)
                _AudioSource.FadeIn(this, _CurrentSetup.FadeIn, volume: _CurrentSetup.Volume);
            else
                _AudioSource.Play();
            _PlayCount++;

            if (_CurrentSetup.Repeat && _PlayCount < _CurrentSetup.RepeatPlayCount)
                _DelayedPlayRoutine = StartCoroutine(PlayDelayed_Coroutine(_AudioSource.clip.length + _CurrentSetup.RepeatDelay, clipIndex));
        }

        private IEnumerator PlayDelayed_Coroutine(float delay, int clipIndex)
        {
            if (delay > 0.01f)
                yield return new WaitForSeconds(delay);
            _DelayedPlayRoutine = null;

            PlayClip(clipIndex);
        }

        private void StopDelayedPlay()
        {
            if (_DelayedPlayRoutine != null) {
                StopCoroutine(_DelayedPlayRoutine);

                _DelayedPlayRoutine = null;
            }
        }

        private int NextClipIndex(AudioSetup setup)
        {
            if (setup.Clips.Length == 0) {
                Debug.LogWarningFormat("Cannot play sound on {0} - missing audio clip", gameObject.name);

                return -1;
            }
            int clipIndex = Random.Range(0, setup.Clips.Length);

            if (clipIndex == LastPlayedClipIndex)
                clipIndex = (clipIndex + 1) % setup.Clips.Length;
            return clipIndex;
        }

        #endregion
    }
}
