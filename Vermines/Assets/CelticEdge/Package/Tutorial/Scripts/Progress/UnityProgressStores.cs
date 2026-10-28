using UnityEngine;
using System.IO;
using System;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// Progression stored in PlayerPrefs as JSON, with one key per profile.
    /// Simple, but machine-specific.
    /// </summary>
    public sealed class PlayerPrefsTutorialProgressStore : ITutorialProgressStore {

        private const string KeyPrefix = "celtic_edge.tutorial.";

        public TutorialProgressData Load(string profileId)
        {
            string json = PlayerPrefs.GetString(KeyPrefix + profileId, null);

            if (string.IsNullOrEmpty(json))
                return null;
            try {
                return JsonUtility.FromJson<TutorialProgressData>(json);
            } catch (Exception e) {
                Debug.LogWarning($"[Tutorial] Unreadable progression for the \"{profileId}\" profile, it's being ignored: {e.Message}");

                return null;
            }
        }

        public void Save(string profileId, TutorialProgressData data)
        {
            PlayerPrefs.SetString(KeyPrefix + profileId, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public void Delete(string profileId)
        {
            PlayerPrefs.DeleteKey(KeyPrefix + profileId);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Progress data stored in a JSON file by profile:
    /// {persistentDataPath}/profiles/{profileId}/tutorial.json
    /// Easy to integrate with cloud synchronization (Steam Cloud synchronizes a folder).
    /// </summary>
    public sealed class JsonFileTutorialProgressStore : ITutorialProgressStore {

        private readonly string _Root;

        public JsonFileTutorialProgressStore(string rootFolder = null)
        {
            _Root = rootFolder ?? Path.Combine(Application.persistentDataPath, "profiles");
        }

        private string PathFor(string profileId) => Path.Combine(_Root, Sanitize(profileId), "tutorial.json");

        public TutorialProgressData Load(string profileId)
        {
            string path = PathFor(profileId);

            if (!File.Exists(path))
                return null;
            try {
                return JsonUtility.FromJson<TutorialProgressData>(File.ReadAllText(path));
            } catch (Exception e) {
                Debug.LogWarning($"[Tutorial] Unreadable progress file ({path}), it's being ignored: {e.Message}");

                return null;
            }
        }

        public void Save(string profileId, TutorialProgressData data)
        {
            string path = PathFor(profileId);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Write to a temporary file and then replace: a crash during the save process does not corrupt the existing progress.
            string tmp = path + ".tmp";

            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));

            if (File.Exists(path))
                File.Replace(tmp, path, null);
            else
                File.Move(tmp, path);
        }

        public void Delete(string profileId)
        {
            string path = PathFor(profileId);

            if (File.Exists(path))
                File.Delete(path);
        }

        private static string Sanitize(string id)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                id = id.Replace(c, '_');
            return id;
        }
    }
}
