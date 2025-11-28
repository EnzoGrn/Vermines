using OMGG.Menu.Tools;
using UnityEngine;
using System;

namespace Vermines.Core.Settings {

    [Serializable]
    public class RegionInfo {
        public string DisplayName;
        public string Region;
        public Sprite Icon;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "NetworkSettings", menuName = "Vermines/Settings/Network Settings")]
    public class NetworkSettings : ScriptableObject {

        public RegionInfo[] Regions;

        public CodeGenerator CodeGenerator => _CodeGenerator;

        [SerializeField]
        private CodeGenerator _CodeGenerator;

        public string QueueName;
    }
}
