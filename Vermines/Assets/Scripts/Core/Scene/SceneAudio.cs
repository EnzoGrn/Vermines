using UnityEngine.Audio;
using UnityEngine;

namespace Vermines.Core.Scene {

    public class SceneAudio : SceneService {

        #region Attributes

        [SerializeField]
        private AudioMixer _MasterMixer;

        #endregion

        #region Methods

        public void UpdateVolume()
        {
            if (_MasterMixer == null)
                return;
            _MasterMixer.SetFloat("MusicVolume"  , Mathf.Log10(Context.RuntimeSettings.MusicVolume)   * 20);
            _MasterMixer.SetFloat("EffectsVolume", Mathf.Log10(Context.RuntimeSettings.EffectsVolume) * 20);
        }

        #endregion

        #region Interface

        protected override void OnActivate()
        {
            base.OnActivate();

            UpdateVolume();
        }

        #endregion
    }
}
