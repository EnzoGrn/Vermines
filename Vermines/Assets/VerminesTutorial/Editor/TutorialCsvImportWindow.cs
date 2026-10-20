using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Vermines.Tutorial.Authoring;

namespace Vermines.Tutorial.EditorTools
{
    /// Importe le tableau de scénario (CSV exporté depuis Excel / Google Sheets) dans
    /// une leçon : crée ou met à jour un asset TutorialStepData par ligne, dans l'ordre
    /// du tableau. Une étape est reconnue par son "id_etape" : réimporter le tableau
    /// après modification met les assets à jour sans perdre les portraits.
    public class TutorialCsvImportWindow : EditorWindow
    {
        [MenuItem("Tools/Vermines/Tutoriel/Importer un tableau CSV")]
        public static void Open() => GetWindow<TutorialCsvImportWindow>("Import du tableau");

        public static void Open(TutorialLessonData lesson)
        {
            var w = GetWindow<TutorialCsvImportWindow>("Import du tableau");
            w._lesson = lesson;
        }

        private TutorialLessonData _lesson;
        private string _csvPath;
        private List<ParsedStep> _parsed;
        private readonly List<string> _warnings = new List<string>();
        private Vector2 _scroll;

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "1. Exportez le tableau en CSV (Fichier > Télécharger > CSV, ou Enregistrer sous > CSV).\n" +
                "2. Choisissez la leçon et le fichier, cliquez sur Analyser, vérifiez les avertissements.\n" +
                "3. Importer crée ou met à jour les étapes. Un modèle est fourni dans Samples/tableau_modele.csv.",
                MessageType.Info);

            _lesson = (TutorialLessonData)EditorGUILayout.ObjectField("Leçon", _lesson, typeof(TutorialLessonData), false);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Fichier CSV", _csvPath);
            if (GUILayout.Button("Choisir…", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFilePanel("Tableau du tutoriel", Application.dataPath, "csv,tsv,txt");
                if (!string.IsNullOrEmpty(path))
                {
                    _csvPath = path;
                    _parsed = null;
                }
            }
            EditorGUILayout.EndHorizontal();

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_csvPath)))
            {
                if (GUILayout.Button("Analyser"))
                    Analyse();
            }

            if (_parsed != null)
            {
                var rows = RowsForLesson();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"{rows.Count} étape(s) pour cette leçon, {_warnings.Count} avertissement(s).", EditorStyles.boldLabel);

                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
                foreach (var w in _warnings)
                    EditorGUILayout.HelpBox(w, MessageType.Warning);
                foreach (var r in rows)
                    EditorGUILayout.LabelField($"{r.stepId}", $"{r.popupStyle} · {r.advanceCondition} · {Shorten(r.body)}");
                EditorGUILayout.EndScrollView();

                using (new EditorGUI.DisabledScope(_lesson == null || rows.Count == 0))
                {
                    if (GUILayout.Button("Importer dans la leçon", GUILayout.Height(28)))
                        Import(rows);
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(_lesson == null))
            {
                if (GUILayout.Button("Exporter la leçon en CSV…"))
                    Export(_lesson);
            }
        }

        private void Analyse()
        {
            _warnings.Clear();
            try
            {
                _parsed = TutorialTable.Parse(File.ReadAllText(_csvPath), _warnings);
            }
            catch (IOException e)
            {
                _parsed = null;
                EditorUtility.DisplayDialog("Lecture impossible", e.Message, "OK");
            }
        }

        private List<ParsedStep> RowsForLesson()
        {
            if (_parsed == null)
                return new List<ParsedStep>();
            if (_lesson == null)
                return _parsed;
            bool anyLessonColumn = _parsed.Any(p => !string.IsNullOrEmpty(p.lessonId));
            return anyLessonColumn ? _parsed.Where(p => p.lessonId == _lesson.lessonId).ToList() : _parsed;
        }

        private void Import(List<ParsedStep> rows)
        {
            string lessonPath = AssetDatabase.GetAssetPath(_lesson);
            string folder = Path.Combine(Path.GetDirectoryName(lessonPath), _lesson.name + "_Steps").Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(lessonPath).Replace('\\', '/'), _lesson.name + "_Steps");

            Undo.RecordObject(_lesson, "Import du tableau de tutoriel");
            var existing = _lesson.steps.Where(s => s != null && !string.IsNullOrEmpty(s.stepId))
                                        .GroupBy(s => s.stepId).ToDictionary(g => g.Key, g => g.First());
            var newOrder = new List<TutorialStepData>();
            int created = 0, updated = 0;

            foreach (var row in rows)
            {
                if (!existing.TryGetValue(row.stepId, out var step))
                {
                    step = CreateInstance<TutorialStepData>();
                    string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{Sanitize(row.stepId)}.asset");
                    AssetDatabase.CreateAsset(step, path);
                    created++;
                }
                else
                {
                    Undo.RecordObject(step, "Import du tableau de tutoriel");
                    updated++;
                }
                TutorialStepAssetBinding.Apply(row, step);
                EditorUtility.SetDirty(step);
                newOrder.Add(step);

                foreach (var problem in step.Validate())
                    Debug.LogWarning($"[Tutoriel] {row.stepId} (ligne {row.sourceLine}) : {problem}", step);
            }

            var removed = _lesson.steps.Where(s => s != null && !newOrder.Contains(s)).ToList();
            _lesson.steps = newOrder;
            EditorUtility.SetDirty(_lesson);
            AssetDatabase.SaveAssets();

            string msg = $"{created} étape(s) créée(s), {updated} mise(s) à jour.";
            if (removed.Count > 0)
                msg += $"\n{removed.Count} étape(s) absente(s) du tableau retirée(s) de la leçon (fichiers conservés) : " +
                       string.Join(", ", removed.Select(s => s.name));
            EditorUtility.DisplayDialog("Import terminé", msg, "OK");
        }

        public static void Export(TutorialLessonData lesson)
        {
            string path = EditorUtility.SaveFilePanel("Exporter la leçon", Application.dataPath, lesson.name, "csv");
            if (string.IsNullOrEmpty(path))
                return;
            var rows = lesson.steps.Where(s => s != null).Select(s => TutorialStepAssetBinding.From(s, lesson.lessonId));
            // BOM UTF-8 pour qu'Excel affiche correctement les accents.
            File.WriteAllText(path, TutorialTable.ToCsv(rows), new System.Text.UTF8Encoding(true));
            EditorUtility.RevealInFinder(path);
        }

        private static string Shorten(string s)
        {
            if (string.IsNullOrEmpty(s)) return "(pas de texte)";
            s = s.Replace('\n', ' ');
            return s.Length > 70 ? s.Substring(0, 70) + "…" : s;
        }

        private static string Sanitize(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s;
        }
    }
}
