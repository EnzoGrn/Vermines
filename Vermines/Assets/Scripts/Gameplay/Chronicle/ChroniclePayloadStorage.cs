using System.Collections.Generic;

namespace Vermines.Gameplay.Chronicle {

    public static class ChroniclePayloadStorage {

        private static readonly Dictionary<string, string> _Payloads = new();

        public static void Add(string id, string payloadJson)
        {
            _Payloads[id] = payloadJson;
        }

        public static bool TryGet(string id, out string payloadJson)
        {
            return _Payloads.TryGetValue(id, out payloadJson);
        }

        public static bool Remove(string id)
        {
            return _Payloads.Remove(id);
        }

        public static void Clear()
        {
            _Payloads.Clear();
        }
    }
}
