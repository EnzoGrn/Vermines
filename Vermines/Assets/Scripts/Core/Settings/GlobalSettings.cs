using Fusion;
using System;
using UnityEngine;
using Vermines.Characters;

namespace Vermines.Core.Settings {

    [Serializable]
    [CreateAssetMenu(fileName = "GlobalSettings", menuName = "Vermines/Settings/Global Settings")]
    public class GlobalSettings : ScriptableObject {

        public NetworkRunner RunnerPrefab;

        public string LoadingScene = "LoadingScene";
        public string MenuScene = "Menu";

        public CultistDatabase Cultists;

        public NetworkSettings Network;
        public OptionsData DefaultOptions;
    }
}
