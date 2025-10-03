using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace OMGG.Chronicle {

    public class ChronicleManager {

        #region Variables

        public event UnityAction<ChronicleEntry> OnChronicleAdded;
        public event UnityAction<ChronicleEntry> OnChronicleRemoved;
        public event UnityAction<ChronicleEntry> OnChronicleUpdated;

        private readonly List<ChronicleEntry> _History = new();

        private IChronicleBroadcaster _Broadcaster;

        #endregion

        #region Setters

        public void SetBroadcaster(IChronicleBroadcaster caster)
        {
            _Broadcaster = caster;
        }

        public void AddEvent(ChronicleEntry entry)
        {
            entry.Id         ??= Guid.NewGuid().ToString();
            entry.TimestampUtc = DateTime.UtcNow.Ticks;

            _History.Add(entry);

            OnChronicleAdded?.Invoke(entry);

            _Broadcaster?.BroadcastChronicle(entry);
        }

        public void RemoveEvent(string id)
        {
            var entry = _History.Find(e => e.Id == id);

            if (entry == null)
                return;
            _History.Remove(entry);

            OnChronicleRemoved?.Invoke(entry);

            _Broadcaster?.BroadcastChronicleRemoval(id);
        }

        public void UpdateEvent(ChronicleEntry entry)
        {
            var existingEntry = _History.Find(e => e.Id == entry.Id);

            if (existingEntry == null)
                AddEvent(entry);
            else {
                existingEntry = entry;

                _History.Insert(_History.IndexOf(existingEntry), existingEntry);

                OnChronicleUpdated?.Invoke(existingEntry);
            }
        }

        #endregion

        #region Getters

        public IReadOnlyList<ChronicleEntry> GetHistory() => _History;

        #endregion
    }
}
