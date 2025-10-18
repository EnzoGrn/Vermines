using Fusion;
using System;
using UnityEngine;

namespace Vermines.Core.Settings {

    [Serializable]
    [CreateAssetMenu(fileName = "GlobalSettings", menuName = "Vermines/Settings/Global Settings")]
    public class GlobalSettings : ScriptableObject {

        public NetworkRunner RunnerPrefab;

        public string LoadingScene = "LoadingScene";
        public string MenuScene = "Menu";

        public NetworkSettings Network;
    }
}
