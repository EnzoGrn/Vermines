using System;
using System.IO;
using UnityEngine;

namespace Vermines.Tutorial
{
    /// Progression rangée dans les PlayerPrefs, en JSON, une clé par profil.
    /// Simple, mais local à la machine.
    public sealed class PlayerPrefsTutorialProgressStore : ITutorialProgressStore
    {
        private const string KeyPrefix = "vermines.tutorial.";

        public TutorialProgressData Load(string profileId)
        {
            string json = PlayerPrefs.GetString(KeyPrefix + profileId, null);
            if (string.IsNullOrEmpty(json))
                return null;
            try
            {
                return JsonUtility.FromJson<TutorialProgressData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Tutoriel] Progression illisible pour le profil \"{profileId}\", elle est ignorée : {e.Message}");
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

    /// Progression rangée dans un fichier JSON par profil :
    /// {persistentDataPath}/profiles/{profileId}/tutorial.json
    /// Facile à brancher sur une synchronisation cloud (Steam Cloud synchronise un dossier).
    public sealed class JsonFileTutorialProgressStore : ITutorialProgressStore
    {
        private readonly string _root;

        public JsonFileTutorialProgressStore(string rootFolder = null)
        {
            _root = rootFolder ?? Path.Combine(Application.persistentDataPath, "profiles");
        }

        private string PathFor(string profileId) => Path.Combine(_root, Sanitize(profileId), "tutorial.json");

        public TutorialProgressData Load(string profileId)
        {
            string path = PathFor(profileId);
            if (!File.Exists(path))
                return null;
            try
            {
                return JsonUtility.FromJson<TutorialProgressData>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Tutoriel] Fichier de progression illisible ({path}), il est ignoré : {e.Message}");
                return null;
            }
        }

        public void Save(string profileId, TutorialProgressData data)
        {
            string path = PathFor(profileId);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            // Écriture dans un fichier temporaire puis remplacement : un crash pendant
            // la sauvegarde ne corrompt pas la progression existante.
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
