using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Core.Scene {

    /// <summary>
    /// Learns how long a loading lasts after each of its stages starts (stage = network status + description),
    /// and uses it to estimate the time left during the next loadings. The learned times are saved in the PlayerPrefs.
    /// </summary>
    public class LoadingTimeEstimator {

        #region Types

        /// <summary>
        /// Learned remaining time of one stage, as saved in the PlayerPrefs.
        /// </summary>
        [System.Serializable]
        private class StageTime {

            public string Stage;

            public float Remaining;
        }

        /// <summary>
        /// Every learned stage, as saved in the PlayerPrefs.
        /// </summary>
        [System.Serializable]
        private class SaveData {

            public List<StageTime> Stages = new();
        }

        #endregion

        #region Constants

        /// <summary>
        /// PlayerPrefs key of the learned times.
        /// </summary>
        private const string PrefsKey = "Vermines.LoadingTimes";

        /// <summary>
        /// Weight of the last loading in the learned times, the rest is the previous average.
        /// </summary>
        private const float LearningRate = 0.5f;

        #endregion

        #region Attributes

        /// <summary>
        /// Learned time between the start of a stage and the end of the loading, by stage.
        /// </summary>
        private readonly Dictionary<string, float> _RemainingByStage = new();

        /// <summary>
        /// Stages reached by the current loading, with their start time since the beginning of the loading.
        /// </summary>
        private readonly Dictionary<string, float> _VisitedStages = new();

        /// <summary>
        /// Real time when the current loading started, negative when no loading is running.
        /// </summary>
        private float _StartTime = -1f;

        /// <summary>
        /// Current stage of the loading.
        /// </summary>
        private string _Stage;

        /// <summary>
        /// Real time when the current stage started.
        /// </summary>
        private float _StageStartTime;

        #endregion

        #region Getters & Setters

        public bool IsRunning => _StartTime >= 0f;

        private static float Now => Time.realtimeSinceStartup;

        #endregion

        #region Methods

        public LoadingTimeEstimator()
        {
            Load();
        }

        /// <summary>
        /// Starts measuring a loading.
        /// </summary>
        public void Begin()
        {
            _StartTime = Now;
            _Stage     = null;

            _VisitedStages.Clear();
        }

        /// <summary>
        /// Tells the current stage of the loading, call it every frame.
        /// </summary>
        public void SetStage(string stage)
        {
            if (!IsRunning || stage == _Stage)
                return;
            _Stage          = stage;
            _StageStartTime = Now;

            if (!_VisitedStages.ContainsKey(stage))
                _VisitedStages.Add(stage, _StageStartTime - _StartTime);
        }

        /// <summary>
        /// Estimated time left before the loading ends, in seconds, or a negative value when it is unknown
        /// (stage never seen, or taking longer than usual).
        /// </summary>
        public float GetRemainingTime()
        {
            if (!IsRunning || _Stage == null || !_RemainingByStage.TryGetValue(_Stage, out float remaining))
                return -1f;
            float left = remaining - (Now - _StageStartTime);

            return left > 0f ? left : -1f;
        }

        /// <summary>
        /// Ends the measure and learns the time left after each stage that was reached.
        /// </summary>
        public void End()
        {
            if (!IsRunning)
                return;
            float duration = Now - _StartTime;

            foreach (KeyValuePair<string, float> visited in _VisitedStages) {
                float remaining = duration - visited.Value;

                _RemainingByStage[visited.Key] = _RemainingByStage.TryGetValue(visited.Key, out float previous) ? Mathf.Lerp(previous, remaining, LearningRate) : remaining;
            }
            _StartTime = -1f;

            Save();
        }

        private void Load()
        {
            string json = PlayerPrefs.GetString(PrefsKey, string.Empty);

            if (string.IsNullOrEmpty(json))
                return;
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data?.Stages == null)
                return;
            foreach (StageTime stage in data.Stages) {
                if (!string.IsNullOrEmpty(stage.Stage))
                    _RemainingByStage[stage.Stage] = stage.Remaining;
            }
        }

        private void Save()
        {
            SaveData data = new();

            foreach (KeyValuePair<string, float> stage in _RemainingByStage)
                data.Stages.Add(new StageTime { Stage = stage.Key, Remaining = stage.Value });
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        #endregion
    }
}
