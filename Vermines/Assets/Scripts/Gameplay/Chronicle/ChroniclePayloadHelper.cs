using Newtonsoft.Json;
using UnityEngine;
using System;

namespace Vermines.Gameplay.Chronicle {

    public static class ChroniclePayloadHelper {

        public static string[] GetDescriptionArgs(string payloadJson)
        {
            if (string.IsNullOrEmpty(payloadJson))
                return null;
            try {
                var obj = JsonConvert.DeserializeObject<PayloadShape>(payloadJson);

                return obj?.DescriptionArgs;
            } catch (Exception e) {
                Debug.LogWarning($"Failed to parse payload: {e.Message}");

                return null;
            }
        }

        private class PayloadShape {

            /// <summary>
            /// [0] -> Description Key (Localization)
            /// [1...] -> Description Args (Localization)
            /// </summary>
            public string[] DescriptionArgs { get; set; }
        }
    }
}
