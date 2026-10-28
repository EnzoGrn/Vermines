using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace CelticEdge.Tutorial.EditorTools {

    using CelticEdge.Tutorial.Authoring;

    /// <summary>
    /// Imports the script table (CSV exported from Excel / Google Sheets) into
    /// a lesson: creates or updates one TutorialStepData asset per row, in the order
    /// of the table, for the steps and the interludes ("lesson_id/interlude_id").
    /// A step is identified by its "id_etape": re-importing the table after
    /// editing it updates the assets without losing the portraits.
    /// </summary>
    public class TutorialCsvImportWindow : EditorWindow {

        private readonly List<string> _Warnings = new();

        private TutorialLessonData _Lesson;
        private string             _CsvPath;
        private List<ParsedStep>   _Parsed;
        private Vector2            _Scroll;

        [MenuItem("Tools/CelticEdge/Tutorial/Import a CSV Table")]
        public static void Open() => GetWindow<TutorialCsvImportWindow>("Table Import");

        public static void Open(TutorialLessonData lesson)
        {
            var w = GetWindow<TutorialCsvImportWindow>("Table Import");

            w._Lesson = lesson;
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "1. Export the table as CSV (File > Download > CSV, or Save As > CSV).\n" +
                "2. Choose the lesson and the file, click Analyze, check the warnings.\n" +
                "3. Import creates or updates the steps and the interludes. A template is provided in Demo/tableau_modele.csv.",
                MessageType.Info);

            _Lesson = (TutorialLessonData)EditorGUILayout.ObjectField("Lesson", _Lesson, typeof(TutorialLessonData), false);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("CSV File", _CsvPath);

            if (GUILayout.Button("Browse…", GUILayout.Width(80))) {
                string path = EditorUtility.OpenFilePanel("Tutorial Table", Application.dataPath, "csv,tsv,txt");

                if (!string.IsNullOrEmpty(path)) {
                    _CsvPath = path;
                    _Parsed  = null;
                }
            }

            EditorGUILayout.EndHorizontal();

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_CsvPath))) {
                if (GUILayout.Button("Analyze"))
                    Analyse();
            }

            if (_Parsed != null) {
                var rows = RowsForLesson();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"{rows.Count} step(s) for this lesson, {_Warnings.Count} warning(s).", EditorStyles.boldLabel);

                _Scroll = EditorGUILayout.BeginScrollView(_Scroll, GUILayout.ExpandHeight(true));

                foreach (var w in _Warnings)
                    EditorGUILayout.HelpBox(w, MessageType.Warning);
                foreach (var r in rows)
                    EditorGUILayout.LabelField($"{r.StepId}", $"{r.PopupStyle} · {r.AdvanceCondition} · {Shorten(r.Body)}");
                EditorGUILayout.EndScrollView();

                using (new EditorGUI.DisabledScope(_Lesson == null || rows.Count == 0)) {
                    if (GUILayout.Button("Import into Lesson", GUILayout.Height(28)))
                        Import(rows);
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(_Lesson == null)) {
                if (GUILayout.Button("Export Lesson as CSV…"))
                    Export(_Lesson);
            }
        }

        private void Analyse()
        {
            _Warnings.Clear();

            try {
                _Parsed = TutorialTable.Parse(File.ReadAllText(_CsvPath), _Warnings);
            } catch (IOException e) {
                _Parsed = null;

                EditorUtility.DisplayDialog("Cannot Read File", e.Message, "OK");
            }
        }

        private List<ParsedStep> RowsForLesson()
        {
            if (_Parsed == null)
                return new List<ParsedStep>();
            if (_Lesson == null)
                return _Parsed;
            bool anyLessonColumn = _Parsed.Any(p => !string.IsNullOrEmpty(p.LessonId));

            return anyLessonColumn ? _Parsed.Where(p => TutorialTableLoader.BelongsTo(p, _Lesson.LessonId, out _)).ToList() : _Parsed;
        }

        private void Import(List<ParsedStep> rows)
        {
            string lessonPath = AssetDatabase.GetAssetPath(_Lesson);
            string folder     = Path.Combine(Path.GetDirectoryName(lessonPath), _Lesson.name + "_Steps").Replace('\\', '/');

            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(lessonPath).Replace('\\', '/'), _Lesson.name + "_Steps");
            Undo.RecordObject(_Lesson, "Import Tutorial Table");

            var allSteps = _Lesson.Steps.Concat(_Lesson.Interludes.Where(i => i != null).SelectMany(i => i.Steps)).ToList();
            var existing = allSteps.Where(s => s != null && !string.IsNullOrEmpty(s.StepId))
                                   .GroupBy(s => s.StepId).ToDictionary(g => g.Key, g => g.First());

            List<TutorialStepData>  newOrder      = new();
            List<TutorialInterlude> newInterludes = new();
            List<TutorialStepData>  imported      = new();

            int created = 0;
            int updated = 0;

            foreach (var row in rows) {
                if (!existing.TryGetValue(row.StepId, out var step)) {
                    step = CreateInstance<TutorialStepData>();

                    string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{Sanitize(row.StepId)}.asset");

                    AssetDatabase.CreateAsset(step, path);

                    created++;
                } else {
                    Undo.RecordObject(step, "Import Tutorial Table");

                    updated++;
                }

                TutorialStepAssetBinding.Apply(row, step);
                EditorUtility.SetDirty(step);

                imported.Add(step);

                TutorialTableLoader.BelongsTo(row, _Lesson.LessonId, out string interludeId);

                if (interludeId == null) {
                    newOrder.Add(step);
                } else {
                    var interlude = newInterludes.FirstOrDefault(i => i.InterludeId == interludeId);

                    if (interlude == null)
                        newInterludes.Add(interlude = new TutorialInterlude { InterludeId = interludeId });
                    interlude.Steps.Add(step);
                }

                foreach (var problem in step.Validate())
                    Debug.LogWarning($"[Tutorial] {row.StepId} (line {row.SourceLine}): {problem}", step);
            }

            var removed = allSteps.Where(s => s != null && !imported.Contains(s)).ToList();

            _Lesson.Steps      = newOrder;
            _Lesson.Interludes = newInterludes;

            EditorUtility.SetDirty(_Lesson);
            AssetDatabase.SaveAssets();

            string msg = $"{created} step(s) created, {updated} updated.";

            if (removed.Count > 0)
                msg += $"\n{removed.Count} step(s) missing from the table removed from the lesson (files kept): " +
                       string.Join(", ", removed.Select(s => s.name));
            EditorUtility.DisplayDialog("Import Complete", msg, "OK");
        }

        public static void Export(TutorialLessonData lesson)
        {
            string path = EditorUtility.SaveFilePanel("Export Lesson", Application.dataPath, lesson.name, "csv");

            if (string.IsNullOrEmpty(path))
                return;
            var rows = lesson.Steps.Where(s => s != null).Select(s => TutorialStepAssetBinding.From(s, lesson.LessonId))
                .Concat(lesson.Interludes.Where(i => i != null).SelectMany(i => i.Steps.Where(s => s != null)
                    .Select(s => TutorialStepAssetBinding.From(s, $"{lesson.LessonId}/{i.InterludeId}"))));

            // UTF-8 BOM so that Excel displays accented characters correctly.
            File.WriteAllText(path, TutorialTable.ToCsv(rows), new System.Text.UTF8Encoding(true));
            EditorUtility.RevealInFinder(path);
        }

        private static string Shorten(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "(no text)";
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
