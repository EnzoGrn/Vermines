using System;
using System.Collections.Generic;

namespace Vermines.Tutorial
{
    /// Ce qui est sauvegardé dans le profil du joueur. Sérialisable par JsonUtility.
    ///
    /// La sauvegarde est à la granularité de la leçon : une micro-partie interrompue
    /// reprend à son début, comme les missions de Hearthstone. Reprendre au milieu
    /// demanderait de sauvegarder tout l'état du plateau, pour un gain faible sur des
    /// parties de quelques minutes.
    [Serializable]
    public class TutorialProgressData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public List<string> completedLessonIds = new List<string>();

        /// Leçon commencée et pas encore terminée ("Reprendre" dans le menu).
        public string inProgressLessonId;

        /// Le joueur a choisi de passer tout le tutoriel.
        public bool tutorialSkipped;

        /// Date de dernière modification, ISO 8601 UTC.
        public string lastUpdatedUtc;
    }

    /// Où ranger la progression. Le jeu peut fournir sa propre implémentation pour
    /// la ranger dans son système de profils / de sauvegarde cloud.
    public interface ITutorialProgressStore
    {
        /// Renvoie null si aucun enregistrement n'existe pour ce profil.
        TutorialProgressData Load(string profileId);
        void Save(string profileId, TutorialProgressData data);
        void Delete(string profileId);
    }

    /// Stockage en mémoire (tests, mode invité).
    public sealed class InMemoryTutorialProgressStore : ITutorialProgressStore
    {
        private readonly Dictionary<string, TutorialProgressData> _data = new Dictionary<string, TutorialProgressData>();

        public TutorialProgressData Load(string profileId) =>
            _data.TryGetValue(profileId ?? string.Empty, out var d) ? Clone(d) : null;

        public void Save(string profileId, TutorialProgressData data) => _data[profileId ?? string.Empty] = Clone(data);

        public void Delete(string profileId) => _data.Remove(profileId ?? string.Empty);

        private static TutorialProgressData Clone(TutorialProgressData d) => new TutorialProgressData
        {
            version = d.version,
            completedLessonIds = new List<string>(d.completedLessonIds),
            inProgressLessonId = d.inProgressLessonId,
            tutorialSkipped = d.tutorialSkipped,
            lastUpdatedUtc = d.lastUpdatedUtc
        };
    }

    /// Lecture et écriture de la progression d'un profil. Chaque modification est
    /// sauvegardée immédiatement.
    public sealed class TutorialProgressService
    {
        private readonly ITutorialProgressStore _store;

        public string ProfileId { get; }
        public TutorialProgressData Data { get; private set; }

        public event Action<TutorialProgressData> OnChanged;

        public TutorialProgressService(ITutorialProgressStore store, string profileId)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            ProfileId = string.IsNullOrEmpty(profileId) ? "default" : profileId;
            Data = Migrate(_store.Load(ProfileId)) ?? new TutorialProgressData();
        }

        public bool IsCompleted(string lessonId) =>
            !string.IsNullOrEmpty(lessonId) && Data.completedLessonIds.Contains(lessonId);

        public bool IsSkipped => Data.tutorialSkipped;

        public void MarkStarted(string lessonId)
        {
            Data.inProgressLessonId = lessonId;
            Commit();
        }

        public void MarkCompleted(string lessonId)
        {
            if (string.IsNullOrEmpty(lessonId))
                return;
            if (!Data.completedLessonIds.Contains(lessonId))
                Data.completedLessonIds.Add(lessonId);
            if (Data.inProgressLessonId == lessonId)
                Data.inProgressLessonId = null;
            Commit();
        }

        public void ClearInProgress()
        {
            Data.inProgressLessonId = null;
            Commit();
        }

        public void SkipTutorial()
        {
            Data.tutorialSkipped = true;
            Data.inProgressLessonId = null;
            Commit();
        }

        public void ResetAll()
        {
            Data = new TutorialProgressData();
            _store.Delete(ProfileId);
            OnChanged?.Invoke(Data);
        }

        private void Commit()
        {
            Data.version = TutorialProgressData.CurrentVersion;
            Data.lastUpdatedUtc = DateTime.UtcNow.ToString("o");
            _store.Save(ProfileId, Data);
            OnChanged?.Invoke(Data);
        }

        /// Point d'entrée pour faire évoluer le format plus tard (renommage d'IDs de
        /// leçons, etc.) sans perdre la progression des joueurs.
        private static TutorialProgressData Migrate(TutorialProgressData loaded)
        {
            if (loaded == null)
                return null;
            if (loaded.completedLessonIds == null)
                loaded.completedLessonIds = new List<string>();
            loaded.version = TutorialProgressData.CurrentVersion;
            return loaded;
        }
    }
}
