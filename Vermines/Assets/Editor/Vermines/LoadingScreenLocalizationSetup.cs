using UnityEngine.Localization.Tables;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor.Localization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;
using TMPro;

namespace Vermines.EditorTools {

    using Vermines.Core.Scene;

    /// <summary>
    /// Creates the "LoadingScreens" string table, fills it with the French texts of the loading screen prefabs
    /// and their English translations, then adds a <see cref="LoadingScreenLocalization"/> to every prefab.
    /// Existing table entries are never overwritten, so the tool can be run again after adding new screens.
    /// </summary>
    public static class LoadingScreenLocalizationSetup {

        #region Constants

        /// <summary>
        /// Folder of the loading screen prefabs.
        /// </summary>
        private const string PrefabFolder = "Assets/Resources/Prefabs/UI/LoadingScreen";

        /// <summary>
        /// Parent folder of the new table.
        /// </summary>
        private const string TableParentFolder = "Assets/Resources/Localization/Tables";

        /// <summary>
        /// Folder of the new table.
        /// </summary>
        private const string TableFolder = TableParentFolder + "/" + LoadingScreenLocalization.TableName;

        /// <summary>
        /// English texts by loading screen key, the French ones are read from the prefabs.
        /// </summary>
        private static readonly Dictionary<string, (string Title, string Description)> EnglishTexts = new() {
            { "bees", ("The Bees", "Find a way to tame the Bee people and bring down their Kingdom.") },
            { "belzerocera", ("Belzérocera", "Symbolic depiction of Belzérocera, God of the flying creatures.\nIt will stand by your side in your quest if you swear allegiance to it.") },
            { "chilosmodee", ("Chilosmodée", "The Elders say it hides within the Kingdom in the shape of a centipede. Make offerings to it, swear to serve it, and perhaps it will show itself.") },
            { "crickets", ("The Crickets", "Will your song be like the Crickets' song? Sing the praises of your Occult Gods and enlist followers.") },
            { "cultists", ("The Cultists", "Longing to bring their Occult Gods back to life, the Cultists will do anything to sacrifice souls.") },
            { "dracoleon", ("Dracoleon", "This creature, half dragon,\nhalf chameleon, serves the\nBees. Enlist it and you will be able\nto copy the effects of other followers.") },
            { "firefly", ("The Fireflies", "Active at night, they let you add followers to the Courtyard.") },
            { "fly", ("The Flies", "Approach them with caution, they will serve you if you know how to talk to them.") },
            { "high_bees", ("The Bees", "Rulers of the Kingdom, will they be able to defend themselves against the threat you pose? Or will they let your words seduce them?") },
            { "ladybugs", ("The Ladybugs", "A symbol of luck, the ladybug will guide you. May your words bring you many followers.") },
            { "lux_dux", ("Guide of Light", "Of the Firefly family, the Guide of Light is a true warrior. Win it over to your cause to earn many souls.") },
            { "magus_mortis", ("Magus Mortis", "Eager to collect new souls,\nit will give you some at the\nright moment.") },
            { "mantislith", ("Mantislith", "A female demon, she devours the hopes of new lives and terrorizes the Kingdom. Keep your head on your shoulders: if you worship her, she will grant you a certain power.") },
            { "mosquito", ("The Mosquitoes", "A band of mercenaries who will serve anyone, for a price.") },
            { "proles_caduca", ("Proles Caduca", "Rejected by the Bee Kingdom,\nthis abomination makes even\nthe bravest tremble.") },
            { "scarabs", ("The Scarabs", "Will your words pierce the hard shell of the scarabs all the way to their heart?") },
            { "scortopheles", ("Scortophélès", "Hungry for destruction, Scortophélès is eager to be summoned to put the Kingdom to fire and sword.") }
        };

        #endregion

        #region Methods

        [MenuItem("Vermines/Localization/Setup Loading Screens")]
        private static void Setup()
        {
            StringTableCollection collection = GetOrCreateCollection();
            StringTable french = collection.GetTable("fr") as StringTable;
            StringTable english = collection.GetTable("en") as StringTable;

            if (french == null || english == null) {
                Debug.LogError($"[LoadingScreenLocalizationSetup] The table '{LoadingScreenLocalization.TableName}' needs a French (fr) and an English (en) locale.");

                return;
            }
            List<string> paths = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(GetVariantDepth) // Base prefabs first, so variants inherit the component.
                .ToList();
            int linked = 0;

            foreach (string path in paths) {
                if (SetupPrefab(path, french, english))
                    linked++;
            }
            EditorUtility.SetDirty(french);
            EditorUtility.SetDirty(english);
            EditorUtility.SetDirty(collection.SharedData);
            AssetDatabase.SaveAssets();

            Debug.Log($"[LoadingScreenLocalizationSetup] {linked}/{paths.Count} loading screens linked to the '{LoadingScreenLocalization.TableName}' table.");
        }

        private static StringTableCollection GetOrCreateCollection()
        {
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(LoadingScreenLocalization.TableName);

            if (collection != null)
                return collection;
            if (!AssetDatabase.IsValidFolder(TableFolder))
                AssetDatabase.CreateFolder(TableParentFolder, LoadingScreenLocalization.TableName);
            return LocalizationEditorSettings.CreateStringTableCollection(LoadingScreenLocalization.TableName, TableFolder);
        }

        private static bool SetupPrefab(string path, StringTable french, StringTable english)
        {
            string key = GetKey(Path.GetFileNameWithoutExtension(path));
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            try {
                TMP_Text title = FindText(root, "Title");
                TMP_Text description = FindText(root, "Description");

                if (title == null || description == null) {
                    Debug.LogWarning($"[LoadingScreenLocalizationSetup] '{path}' has no 'Title' or 'Description' text, skipped.");

                    return false;
                }
                string titleKey = LoadingScreenLocalization.GetTitleKey(key);
                string descriptionKey = LoadingScreenLocalization.GetDescriptionKey(key);

                AddIfMissing(french, titleKey, title.text);
                AddIfMissing(french, descriptionKey, description.text);

                if (EnglishTexts.TryGetValue(key, out (string Title, string Description) englishText)) {
                    AddIfMissing(english, titleKey, englishText.Title);
                    AddIfMissing(english, descriptionKey, englishText.Description);
                } else {
                    // Unknown screen: the French text is used until a translation is written.
                    AddIfMissing(english, titleKey, title.text);
                    AddIfMissing(english, descriptionKey, description.text);

                    Debug.LogWarning($"[LoadingScreenLocalizationSetup] No English text for '{key}', the French one was copied.");
                }

                if (!root.TryGetComponent(out LoadingScreenLocalization localization))
                    localization = root.AddComponent<LoadingScreenLocalization>();
                SerializedObject serialized = new(localization);

                serialized.FindProperty("_Key").stringValue = key;
                serialized.FindProperty("_Title").objectReferenceValue = title;
                serialized.FindProperty("_Description").objectReferenceValue = description;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, path);

                return true;
            } finally {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>
        /// "LS_ProlesCaduca" and "LS_High_Bees Variant" give "proles_caduca" and "high_bees".
        /// </summary>
        private static string GetKey(string prefabName)
        {
            string name = prefabName.Replace(" Variant", string.Empty);

            if (name.StartsWith("LS_"))
                name = name.Substring(3);
            name = Regex.Replace(name, "([a-z0-9])([A-Z])", "$1_$2");

            return name.Replace(' ', '_').ToLowerInvariant();
        }

        private static int GetVariantDepth(string path)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            int depth = 0;

            for (GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(asset); source != null; source = PrefabUtility.GetCorrespondingObjectFromSource(source))
                depth++;
            return depth;
        }

        private static TMP_Text FindText(GameObject root, string objectName)
        {
            return root.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(text => text.name == objectName);
        }

        private static void AddIfMissing(StringTable table, string key, string value)
        {
            if (table.GetEntry(key) == null)
                table.AddEntry(key, value);
        }

        #endregion
    }
}
