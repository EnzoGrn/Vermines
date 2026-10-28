using UnityEditorInternal;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace CelticEdge.Tutorial.EditorTools {

    /// <summary>
    /// Lesson editor: reorderable list of the steps, editing of the selected step
    /// with a preview of the popup and input error checks, direct testing in Play Mode.
    /// Based on the sequence editing window of the TutorialSystem prototype.
    /// </summary>
    public class TutorialLessonEditorWindow : EditorWindow {

        private TutorialLessonData _Lesson;
        private SerializedObject   _LessonSO;
        private ReorderableList    _List;
        private TutorialStepData   _SelectedStep;
        private SerializedObject   _StepSO;
        private Vector2            _ScrollSteps;
        private Vector2            _ScrollDetail;
        private bool               _ShowSetup;

        [MenuItem("Tools/CelticEdge/Tutorial/Lesson Editor")]
        public static void Open()
        {
            var window = GetWindow<TutorialLessonEditorWindow>("Tutorial Lessons");

            window.minSize = new Vector2(760, 440);
        }

        [MenuItem("Tools/CelticEdge/Tutorial/Reset Progress (PlayerPrefs, default profile)")]
        public static void ResetProgress()
        {
            new PlayerPrefsTutorialProgressStore().Delete("default");

            if (Application.isPlaying && TutorialManager.Instance != null && TutorialManager.Instance.Progress != null)
                TutorialManager.Instance.Progress.ResetAll();
            Debug.Log("[Tutorial] Progress of the \"default\" profile has been reset.");
        }

        private void OnEnable() => RebuildList();

        private void OnSelectionChange()
        {
            if (Selection.activeObject is TutorialLessonData lesson && lesson != _Lesson) {
                _Lesson       = lesson;
                _SelectedStep = null;

                RebuildList();
                Repaint();
            }
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_Lesson == null) {
                EditorGUILayout.HelpBox("Select or create a lesson (Create > CelticEdge > Tutorial > Lesson).", MessageType.Info);

                return;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(340));
            DrawLessonHeader();

            _ScrollSteps = EditorGUILayout.BeginScrollView(_ScrollSteps);

            _LessonSO.Update();
            _List.DoLayoutList();
            _LessonSO.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
            DrawStepButtons();
            EditorGUILayout.EndVertical();

            GUILayout.Box(GUIContent.none, GUILayout.Width(1), GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginVertical();

            _ScrollDetail = EditorGUILayout.BeginScrollView(_ScrollDetail);

            if (_ShowSetup)
                DrawLessonSetup();
            else
                DrawStepDetail();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        #region Toolbar

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();

            var newLesson = (TutorialLessonData)EditorGUILayout.ObjectField(_Lesson, typeof(TutorialLessonData), false, GUILayout.Width(300));

            if (EditorGUI.EndChangeCheck()) {
                _Lesson       = newLesson;
                _SelectedStep = null;

                RebuildList();
            }

            if (GUILayout.Button("New Lesson", EditorStyles.toolbarButton, GUILayout.Width(110)))
                CreateNewLesson();
            using (new EditorGUI.DisabledScope(_Lesson == null)) {
                if (GUILayout.Button("Import CSV…", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    TutorialCsvImportWindow.Open(_Lesson);
                if (GUILayout.Button("Export CSV…", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    TutorialCsvImportWindow.Export(_Lesson);
            }

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(!Application.isPlaying || _Lesson == null)) {
                if (GUILayout.Button("▶ Test in Play Mode", EditorStyles.toolbarButton, GUILayout.Width(150))) {
                    if (TutorialManager.Instance == null)
                        Debug.LogWarning("[Tutorial] No TutorialManager in the current scene.");
                    else
                        TutorialManager.Instance.StartLesson(_Lesson);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CreateNewLesson()
        {
            string path = EditorUtility.SaveFilePanelInProject("New Lesson", "Lesson_01", "asset", "Where should the lesson be saved?");

            if (string.IsNullOrEmpty(path))
                return;
            var lesson = CreateInstance<TutorialLessonData>();

            lesson.LessonId = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();

            AssetDatabase.CreateAsset(lesson, path);
            AssetDatabase.SaveAssets();

            _Lesson       = lesson;
            _SelectedStep = null;

            RebuildList();
        }

        #endregion

        #region Step List

        private void DrawLessonHeader()
        {
            EditorGUILayout.Space(4);

            _LessonSO.Update();

            EditorGUILayout.PropertyField(_LessonSO.FindProperty("LessonId"), new GUIContent("Lesson ID"));

            _LessonSO.ApplyModifiedProperties();

            _ShowSetup = GUILayout.Toggle(_ShowSetup, _ShowSetup ? "◀ Back to Steps" : "Setup, interludes, hints…", "Button");

            EditorGUILayout.Space(4);
        }

        private void RebuildList()
        {
            if (_Lesson == null) {
                _LessonSO = null;
                _List     = null;

                return;
            }

            _LessonSO = new SerializedObject(_Lesson);

            var stepsProp = _LessonSO.FindProperty("Steps");

            _List = new ReorderableList(_LessonSO, stepsProp, true, true, false, false) { elementHeight = 40 };

            _List.drawHeaderCallback  = rect => EditorGUI.LabelField(rect, $"Steps ({stepsProp.arraySize})");
            _List.drawElementCallback = (rect, index, active, focused) => {
                var step = stepsProp.GetArrayElementAtIndex(index).objectReferenceValue as TutorialStepData;

                rect.y += 2;

                EditorGUI.LabelField(new Rect(rect.x, rect.y, 22, rect.height - 4), (index + 1).ToString());

                var body = new Rect(rect.x + 24, rect.y, rect.width - 24, rect.height - 4);

                if (step == null) {
                    EditorGUI.LabelField(body, "(empty step)", EditorStyles.miniLabel);

                    return;
                }

                bool   hasProblem = step.Validate().Any();
                string text       = step.Body.Text;
                string preview    = string.IsNullOrEmpty(text) ? $"[{step.StepId}]" : text.Replace('\n', ' ');

                if (preview.Length > 50)
                    preview = preview.Substring(0, 50) + "…";
                EditorGUI.LabelField(new Rect(body.x, body.y, body.width, 16), (hasProblem ? "⚠ " : "") + preview, EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(body.x, body.y + 16, body.width, 16), Describe(step), EditorStyles.miniLabel);
            };
            _List.onSelectCallback = l => {
                _SelectedStep = stepsProp.GetArrayElementAtIndex(l.index).objectReferenceValue as TutorialStepData;
                _ShowSetup    = false;
            };
        }

        private static string Describe(TutorialStepData step)
        {
            string advance;

            switch (step.AdvanceCondition) {
                case AdvanceCondition.Immediate:
                    advance = "immediate";
                    break;
                case AdvanceCondition.WaitForDelay:
                    advance = $"waits {step.DelaySeconds}s";
                    break;
                case AdvanceCondition.WaitForButton:
                    advance = "waits for \"Next\"";
                    break;
                case AdvanceCondition.WaitForEvent:
                    advance = $"waits for {step.EventId}{(string.IsNullOrEmpty(step.EventArg) ? "" : ":" + step.EventArg)}";
                    break;
                case AdvanceCondition.WaitForTargetClick:
                    advance = "waits for a click on the target";
                    break;
                default:
                    advance = step.AdvanceCondition.ToString();
                    break;
            }

            string targets = step.TargetIds.Count > 0 ? $" · highlights {string.Join(", ", step.TargetIds)}" : "";

            return advance + targets;
        }

        private void DrawStepButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("+ New Step"))
                AddNewStep();
            using (new EditorGUI.DisabledScope(_SelectedStep == null)) {
                if (GUILayout.Button("Duplicate"))
                    DuplicateSelectedStep();
                if (GUILayout.Button("Delete"))
                    RemoveSelectedStep();
            }

            EditorGUILayout.EndHorizontal();
        }

        private string StepsFolder()
        {
            string lessonPath = AssetDatabase.GetAssetPath(_Lesson);
            string parent     = Path.GetDirectoryName(lessonPath).Replace('\\', '/');
            string folder     = $"{parent}/{_Lesson.name}_Steps";

            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(parent, _Lesson.name + "_Steps");
            return folder;
        }

        private void AddNewStep()
        {
            int    number    = _Lesson.Steps.Count + 1;
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{StepsFolder()}/{_Lesson.name}_{number:00}.asset");
            var    step      = CreateInstance<TutorialStepData>();

            step.StepId = $"{_Lesson.LessonId}_{number:00}";

            AssetDatabase.CreateAsset(step, assetPath);
            AssetDatabase.SaveAssets();

            int insertAt = _SelectedStep != null ? _Lesson.Steps.IndexOf(_SelectedStep) + 1 : _Lesson.Steps.Count;

            Undo.RecordObject(_Lesson, "Add Step");

            _Lesson.Steps.Insert(insertAt, step);

            EditorUtility.SetDirty(_Lesson);
            RebuildList();

            _SelectedStep = step;
            _List.index   = insertAt;
        }

        private void DuplicateSelectedStep()
        {
            string path    = AssetDatabase.GetAssetPath(_SelectedStep);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CopyAsset(path, newPath);

            var copy = AssetDatabase.LoadAssetAtPath<TutorialStepData>(newPath);

            copy.StepId = _SelectedStep.StepId + "_copy";

            EditorUtility.SetDirty(copy);

            int index = _Lesson.Steps.IndexOf(_SelectedStep);

            Undo.RecordObject(_Lesson, "Duplicate Step");

            _Lesson.Steps.Insert(index + 1, copy);

            EditorUtility.SetDirty(_Lesson);
            RebuildList();

            _SelectedStep = copy;
            _List.index   = index + 1;
        }

        private void RemoveSelectedStep()
        {
            int choice = EditorUtility.DisplayDialogComplex(
                "Delete Step",
                $"Remove \"{_SelectedStep.name}\" from the lesson. Also delete the .asset file?",
                "Remove and Delete File", "Cancel", "Remove Only");

            if (choice == 1)
                return;
            Undo.RecordObject(_Lesson, "Delete Step");

            _Lesson.Steps.Remove(_SelectedStep);

            EditorUtility.SetDirty(_Lesson);

            if (choice == 0)
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_SelectedStep));
            _SelectedStep = null;

            RebuildList();
        }

        #endregion

        #region Setup

        private void DrawLessonSetup()
        {
            _LessonSO.Update();

            EditorGUILayout.LabelField("Lesson", EditorStyles.boldLabel);

            foreach (var name in new[] { "DisplayName", "Description", "Icon", "SceneName", "RequiredLessonIds", "EndCondition", "AllowSkip", "SkipCountsAsCompleted" })
                EditorGUILayout.PropertyField(_LessonSO.FindProperty(name), true);
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Game-Specific Setup", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_LessonSO.FindProperty("Setup"), true);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Scripted Randomness (replaces the seed)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Shuffle orders from the TOP to the BOTTOM of the pile, by pile ID (the one passed to GameRandom.Shuffle).", MessageType.None);
            EditorGUILayout.PropertyField(_LessonSO.FindProperty("Random"), true);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Scripted Agents", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_LessonSO.FindProperty("ScriptedAgents"), true);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Interludes and Hints", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_LessonSO.FindProperty("Interludes"), true);
            EditorGUILayout.PropertyField(_LessonSO.FindProperty("Hints"), true);

            _LessonSO.ApplyModifiedProperties();
        }

        #endregion

        #region Step Detail

        private void DrawStepDetail()
        {
            if (_SelectedStep == null) {
                EditorGUILayout.HelpBox("Select a step in the list to edit it.", MessageType.None);

                return;
            }

            if (_StepSO == null || _StepSO.targetObject != _SelectedStep)
                _StepSO = new SerializedObject(_SelectedStep);
            _StepSO.Update();

            EditorGUILayout.LabelField(_SelectedStep.name, EditorStyles.boldLabel);
            DrawPreview();

            foreach (var problem in _SelectedStep.Validate())
                EditorGUILayout.HelpBox(problem, MessageType.Warning);
            DrawMissingTargets();

            EditorGUILayout.Space(6);

            var prop = _StepSO.GetIterator();

            prop.NextVisible(true); // m_Script

            while (prop.NextVisible(false)) {
                if (!IsRelevant(prop.name))
                    continue;
                EditorGUILayout.PropertyField(prop, true);
            }

            _StepSO.ApplyModifiedProperties();
        }

        /// <summary>Hides the fields that have no effect for the selected condition / style.</summary>
        private bool IsRelevant(string field)
        {
            var s = _SelectedStep;

            switch (field) {
                case "EventId":
                case "EventArg":
                case "EventCount":
                    return s.AdvanceCondition == AdvanceCondition.WaitForEvent;
                case "DelaySeconds":
                    return s.AdvanceCondition == AdvanceCondition.WaitForDelay;
                case "PointerDropTargetId":
                    return s.Pointer == PointerMode.Drag;
                case "AllowedActions":
                    return s.RestrictGameActions;
                case "Title":
                case "Body":
                case "Portrait":
                case "Placement":
                    return s.PopupStyle != PopupStyle.None;
                default:
                    return true;
            }
        }

        private void DrawPreview()
        {
            if (_SelectedStep.PopupStyle == PopupStyle.None)
                return;
            var    style = new GUIStyle(EditorStyles.helpBox) { fontSize = 12, wordWrap = true, richText = true, padding = new RectOffset(12, 12, 10, 10) };
            string title = string.IsNullOrEmpty(_SelectedStep.Title.Text) ? "" : $"<b>{_SelectedStep.Title.Text}</b>\n";
            string body  = string.IsNullOrEmpty(_SelectedStep.Body.Text) ? "<i>(empty text)</i>" : _SelectedStep.Body.Text;

            EditorGUILayout.BeginHorizontal();

            if (_SelectedStep.Portrait != null) {
                var tex = AssetPreview.GetAssetPreview(_SelectedStep.Portrait) ?? AssetPreview.GetMiniThumbnail(_SelectedStep.Portrait);

                GUILayout.Label(tex, GUILayout.Width(48), GUILayout.Height(48));
            }

            EditorGUILayout.LabelField(title + body, style, GUILayout.MinHeight(48));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Warns about typos in the target IDs, based on the open scene.
        /// Targets set at runtime (objects created on the fly) cannot be checked here.
        /// </summary>
        private void DrawMissingTargets()
        {
            if (_SelectedStep.TargetIds.Count == 0)
                return;
            #if UNITY_2023_1_OR_NEWER
                var sceneTargets = FindObjectsByType<TutorialTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            #else
                var sceneTargets = FindObjectsOfType<TutorialTarget>(true);
            #endif

            var ids     = sceneTargets.Select(t => t.TargetId).ToHashSet();
            var missing = _SelectedStep.TargetIds.Where(id => !ids.Contains(id) && !id.Contains(":")).ToList();

            if (missing.Count > 0)
                EditorGUILayout.HelpBox(
                    $"Target(s) missing from the open scene: {string.Join(", ", missing)}. Check the spelling or open the game scene. " +
                    "(IDs containing \":\", such as \"hand:sword\" or \"go:Name\", are set or looked up at runtime and are not checked.)",
                    MessageType.Warning);
        }

        #endregion
    }
}
