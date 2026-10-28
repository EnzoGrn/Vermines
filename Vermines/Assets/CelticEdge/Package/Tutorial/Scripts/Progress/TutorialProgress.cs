using System.Collections.Generic;
using System;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// What is saved in the player's profile. Serializable using JsonUtility.
    ///
    /// Saving is done at the lesson level: an interrupted micro-game resumes from the beginning, like Hearthstone missions.
    /// Resuming in the middle would require saving the entire board state, which offers little benefit for games lasting only a few minutes.
    /// </summary>
    [Serializable]
    public class TutorialProgressData {

        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;

        public List<string> CompletedLessonIds = new();

        /// <summary>Lesson started but not yet finished ("Resume" in the menu).</summary>
        public string InProgressLessonId;

        /// <summary>The player chose to skip the entire tutorial.</summary>
        public bool TutorialSkipped;

        /// <summary>Date of last modification, ISO 8601 UTC.</summary>
        public string LastUpdatedUtc;
    }

    /// <summary>
    /// Where to store progress.
    /// The game can provide its own implementation to store it in its profile system or cloud save system.
    /// </summary>
    public interface ITutorialProgressStore {

        /// <summary>Returns null if no record exists for this profile.</summary>
        TutorialProgressData Load(string profileId);

        void Save(string profileId, TutorialProgressData data);

        void Delete(string profileId);
    }

    /// <summary>In-memory storage (tests, guest mode).</summary>
    public sealed class InMemoryTutorialProgressStore : ITutorialProgressStore {

        private readonly Dictionary<string, TutorialProgressData> _Data = new();

        public TutorialProgressData Load(string profileId) => _Data.TryGetValue(profileId ?? string.Empty, out var d) ? Clone(d) : null;

        public void Save(string profileId, TutorialProgressData data) => _Data[profileId ?? string.Empty] = Clone(data);

        public void Delete(string profileId) => _Data.Remove(profileId ?? string.Empty);

        private static TutorialProgressData Clone(TutorialProgressData d) => new() {
            Version            = d.Version,
            CompletedLessonIds = new List<string>(d.CompletedLessonIds),
            InProgressLessonId = d.InProgressLessonId,
            TutorialSkipped    = d.TutorialSkipped,
            LastUpdatedUtc     = d.LastUpdatedUtc
        };
    }

    /// <summary>
    /// Reading and writing a profile's progress.
    /// Each change is saved immediately.
    /// </summary>
    public sealed class TutorialProgressService {

        private readonly ITutorialProgressStore _Store;

        public string ProfileId { get; }

        public TutorialProgressData Data { get; private set; }

        public event Action<TutorialProgressData> OnChanged;

        public TutorialProgressService(ITutorialProgressStore store, string profileId)
        {
            _Store    = store ?? throw new ArgumentNullException(nameof(store));
            ProfileId = string.IsNullOrEmpty(profileId) ? "default" : profileId;
            Data      = Migrate(_Store.Load(ProfileId)) ?? new TutorialProgressData();
        }

        public bool IsCompleted(string lessonId) => !string.IsNullOrEmpty(lessonId) && Data.CompletedLessonIds.Contains(lessonId);

        public bool IsSkipped => Data.TutorialSkipped;

        public void MarkStarted(string lessonId)
        {
            Data.InProgressLessonId = lessonId;

            Commit();
        }

        public void MarkCompleted(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId))
                return;
            if (!Data.CompletedLessonIds.Contains(lessonId))
                Data.CompletedLessonIds.Add(lessonId);
            if (Data.InProgressLessonId == lessonId)
                Data.InProgressLessonId = null;
            Commit();
        }

        public void ClearInProgress()
        {
            Data.InProgressLessonId = null;

            Commit();
        }

        public void SkipTutorial()
        {
            Data.TutorialSkipped    = true;
            Data.InProgressLessonId = null;

            Commit();
        }

        public void ResetAll()
        {
            Data = new TutorialProgressData();

            _Store.Delete(ProfileId);
            OnChanged?.Invoke(Data);
        }

        private void Commit()
        {
            Data.Version        = TutorialProgressData.CurrentVersion;
            Data.LastUpdatedUtc = DateTime.UtcNow.ToString("o");
            
            _Store.Save(ProfileId, Data);

            OnChanged?.Invoke(Data);
        }

        /// <summary>
        /// Entry point for future changes to the format (renaming lesson IDs, etc.) without losing players' progress.
        /// </summary>
        private static TutorialProgressData Migrate(TutorialProgressData loaded)
        {
            if (loaded == null)
                return null;
            loaded.CompletedLessonIds ??= new List<string>();
            loaded.Version              = TutorialProgressData.CurrentVersion;

            return loaded;
        }
    }
}
