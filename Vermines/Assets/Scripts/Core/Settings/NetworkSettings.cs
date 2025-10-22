using UnityEngine;
using System;

namespace Vermines.Core.Settings {

    [Serializable]
    [CreateAssetMenu(fileName = "NetworkSettings", menuName = "Vermines/Settings/Network Settings")]
    public class NetworkSettings : ScriptableObject {

        public string QueueName;
    }
}
