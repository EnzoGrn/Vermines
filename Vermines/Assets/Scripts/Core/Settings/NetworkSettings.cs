using UnityEngine;
using System;

namespace Vermines.Core.Settings {

    using Vermines.Extension;

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

        public string QueueName;

        public RegionInfo GetRegionInfo(string region)
        {
            return Regions.Find(t => t.Region == region);
        }
    }
}
