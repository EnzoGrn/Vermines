using Fusion;
using System;
using Vermines.Gameplay.Chronicle;

namespace OMGG.Chronicle {

    /// <summary>
    /// Network-safe version of ChronicleEntry for sending via RPC
    /// </summary>
    [Serializable]
    public struct NetworkChronicleEntry : INetworkStruct {

        /// <summary>
        /// Guid.NewGuid().ToString() - Unique identifier for this entry
        /// </summary>
        public NetworkString<_16> Id;

        /// <summary>
        /// UTC timestamp in ticks
        /// </summary>
        public long TimestampUtc;

        /// <summary>
        /// Event type (mapped to IEventType on the receiving end)
        /// </summary>
        public VerminesLogsType EventType;

        /// <summary>
        /// Priority of this entry (used for sorting and filtering)
        /// </summary>
        public ChroniclePriority Priority;

        /// <summary>
        /// The icon key is used to load a sprite from the Resources folder.
        /// </summary>
        public NetworkString<_32> IconKey;

        /// <summary>
        /// The localization key for the title
        /// </summary>
        public NetworkString<_32> TitleKey;

        /// <summary>
        /// The localization key for the message/description
        /// </summary>
        public NetworkString<_32> MessageKey;

        /// <summary>
        /// Helper to convert from a non-network ChronicleEntry to a NetworkChronicleEntry
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static NetworkChronicleEntry FromChronicleEntry(ChronicleEntry entry)
        {
            NetworkChronicleEntry nEntry = new() {
                Id           = entry.Id,
                TimestampUtc = entry.TimestampUtc,
                EventType    = entry.EventType is VerminesLogEventType verminesType ? verminesType.Type : VerminesLogsType.Unknown,
                Priority     = entry.Priority,

                IconKey    = entry.IconKey,
                TitleKey   = entry.TitleKey,
                MessageKey = entry.MessageKey
            };

            return nEntry;
        }

        /// <summary>
        /// Helper to convert into a non-network ChronicleEntry
        /// </summary>
        /// <returns>The ChronicleEntry instance</returns>
        public ChronicleEntry ToChronicleEntry()
        {
            return new ChronicleEntry {
                Id           = Id.ToString(),
                TimestampUtc = TimestampUtc,
                EventType    = new VerminesLogEventType(EventType),
                Priority     = Priority,

                IconKey    = IconKey.ToString(),
                TitleKey   = TitleKey.ToString(),
                MessageKey = MessageKey.ToString()
            };
        }
    }
}
