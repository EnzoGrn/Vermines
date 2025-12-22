using System;

namespace OMGG.Chronicle {

    [Serializable]
    public partial class ChronicleEntry {

        /// <summary>
        /// Unique identifier for this entry
        /// </summary>
        public string Id;

        /// <summary>
        /// UTC timestamp in ticks
        /// </summary>
        public long TimestampUtc;

        /// <summary>
        /// The type of event this entry represents
        /// </summary>
        public IEventType EventType;

        /// <summary>
        /// The priority of this entry (used for sorting and filtering)
        /// </summary>
        public ChroniclePriority Priority = ChroniclePriority.Normal;

        /// <summary>
        /// The icon key is used to load a sprite from the Resources folder.
        /// </summary>
        public string IconKey;

        /// <summary>
        /// Localization key for the title
        /// </summary>
        public string TitleKey;

        /// <summary>
        /// Localization key for the message/description
        /// </summary>
        public string MessageKey;

        /// <summary>
        /// (Optional) Arguments for the message localization key
        /// </summary>
        public string[] DescriptionArgs;

        public string ActionKey;    // (Optional) If you want a callback action on this entry

        public string PayloadJson; // (Optional) Extra data in JSON format
    }
}
