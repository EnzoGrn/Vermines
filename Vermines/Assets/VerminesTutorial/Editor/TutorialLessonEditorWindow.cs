using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Vermines.Tutorial.EditorTools
{
    /// Éditeur de leçons : liste réordonnable des étapes, édition de l'étape choisie
    /// avec aperçu du popup et contrôle des erreurs de saisie, test direct en Play Mode.
    /// Reprend la fenêtre d'édition de séquences du prototype TutorialSystem.
    public class TutorialLessonEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Vermines/Tutoriel/Éditeur de leçons")]
        public static void Open()
        {
            var window = GetWindow<TutorialLessonEditorWindow>("Leçons du tuto");
            window.minSize = new Vector2(760, 440);
        }

        [MenuItem("Tools/Vermines/Tutoriel/Réinitialiser la progression (PlayerPrefs, profil default)")]
        public static void ResetProgress()
        {
            new PlayerPrefsTutorialProgressStore().Delete("default");
            if (Application.isPlaying && TutorialManager.Instance != null && TutorialManager.Instance.Progress != null)
                TutorialManager.Instance.Progress.ResetAll();
            Debug.Log("[Tutoriel] Progression du profil \"default\" réinitialisée.");
        }

        private TutorialLessonData _lesson;
        private SerializedObject _lessonSO;
        private ReorderableList _list;
        private TutorialStepData _selectedStep;
        private SerializedObject _stepSO;
        private Vector2 _scrollSteps;
        private Vector2 _scrollDetail;
        private bool _showSetup;

        private void OnEnable() => RebuildList();

        private void OnSelectionChange()
        {
            if (Selection.activeObject is TutorialLessonData lesson && lesson != _lesson)
            {
                _lesson = lesson;
                _selectedStep = null;
                RebuildList();
                Repaint();
            }
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (_lesson == null)
            {
                EditorGUILayout.HelpBox("Sélectionnez ou créez une leçon (Create > Vermines > Tutoriel > Leçon).", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(340));
            DrawLessonHeader();
            _scrollSteps = EditorGUILayout.BeginScrollView(_scrollSteps);
            _lessonSO.Update();
            _list.DoLayoutList();
            _lessonSO.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
            DrawStepButtons();
            EditorGUILayout.EndVertical();

            GUILayout.Box(GUIContent.none, GUILayout.Width(1), GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginVertical();
            _scrollDetail = EditorGUILayout.BeginScrollView(_scrollDetail);
            if (_showSetup)
                DrawLessonSetup();
            else
                DrawStepDetail();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        // ------------------------------------------------------------------ Barre

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();
            var newLesson = (TutorialLessonData)EditorGUILayout.ObjectField(_lesson, typeof(TutorialLessonData), false, GUILayout.Width(300));
            if (EditorGUI.EndChangeCheck())
            {
                _lesson = newLesson;
                _selectedStep = null;
                RebuildList();
            }

            if (GUILayout.Button("Nouvelle leçon", EditorStyles.toolbarButton, GUILayout.Width(110)))
                CreateNewLesson();

            using (new EditorGUI.DisabledScope(_lesson == null))
            {
                if (GUILayout.Button("Importer CSV…", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    TutorialCsvImportWindow.Open(_lesson);
                if (GUILayout.Button("Exporter CSV…", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    TutorialCsvImportWindow.Export(_lesson);
            }

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(!Application.isPlaying || _lesson == null))
            {
                if (GUILayout.Button("▶ Tester en Play Mode", EditorStyles.toolbarButton, GUILayout.Width(150)))
                {
                    if (TutorialManager.Instance == null)
                        Debug.LogWarning("[Tutoriel] Aucun TutorialManager dans la scène en cours.");
                    else
                        TutorialManager.Instance.StartLesson(_lesson);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CreateNewLesson()
        {
            string path = EditorUtility.SaveFilePanelInProject("Nouvelle leçon", "Lecon_01", "asset", "Où enregistrer la leçon ?");
            if (string.IsNullOrEmpty(path))
                return;
            var lesson = CreateInstance<TutorialLessonData>();
            lesson.lessonId = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            lesson.matchSetup.players.Add(new ScriptedPlayerSetup { isHuman = true, displayName = "Joueur" });
            lesson.matchSetup.players.Add(new ScriptedPlayerSetup { isHuman = false, displayName = "Adversaire" });
            AssetDatabase.CreateAsset(lesson, path);
            AssetDatabase.SaveAssets();
            _lesson = lesson;
            _selectedStep = null;
            RebuildList();
        }

        // ----------------------------------------------------------- Liste d'étapes

        private void DrawLessonHeader()
        {
            EditorGUILayout.Space(4);
            _lessonSO.Update();
            EditorGUILayout.PropertyField(_lessonSO.FindProperty("lessonId"), new GUIContent("ID de leçon"));
            _lessonSO.ApplyModifiedProperties();
            _showSetup = GUILayout.Toggle(_showSetup, _showSetup ? "◀ Retour aux étapes" : "Mise en place de la partie…", "Button");
            EditorGUILayout.Space(4);
        }

        private void RebuildList()
        {
            if (_lesson == null)
            {
                _lessonSO = null;
                _list = null;
                return;
            }

            _lessonSO = new SerializedObject(_lesson);
            var stepsProp = _lessonSO.FindProperty("steps");
            _list = new ReorderableList(_lessonSO, stepsProp, true, true, false, false) { elementHeight = 40 };

            _list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, $"Étapes ({stepsProp.arraySize})");
            _list.drawElementCallback = (rect, index, active, focused) =>
            {
                var step = stepsProp.GetArrayElementAtIndex(index).objectReferenceValue as TutorialStepData;
                rect.y += 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 22, rect.height - 4), (index + 1).ToString());
                var body = new Rect(rect.x + 24, rect.y, rect.width - 24, rect.height - 4);
                if (step == null)
                {
                    EditorGUI.LabelField(body, "(étape vide)", EditorStyles.miniLabel);
                    return;
                }
                bool hasProblem = step.Validate().Any();
                string text = step.body.text;
                string preview = string.IsNullOrEmpty(text) ? $"[{step.stepId}]" : text.Replace('\n', ' ');
                if (preview.Length > 50) preview = preview.Substring(0, 50) + "…";
                EditorGUI.LabelField(new Rect(body.x, body.y, body.width, 16), (hasProblem ? "⚠ " : "") + preview, EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(body.x, body.y + 16, body.width, 16), Describe(step), EditorStyles.miniLabel);
            };
            _list.onSelectCallback = l =>
            {
                _selectedStep = stepsProp.GetArrayElementAtIndex(l.index).objectReferenceValue as TutorialStepData;
                _showSetup = false;
            };
        }

        private static string Describe(TutorialStepData step)
        {
            string advance;
            switch (step.advanceCondition)
            {
                case AdvanceCondition.Immediate: advance = "immédiat"; break;
                case AdvanceCondition.WaitForDelay: advance = $"attend {step.delaySeconds}s"; break;
                case AdvanceCondition.WaitForButton: advance = "attend \"Suivant\""; break;
                case AdvanceCondition.WaitForEvent: advance = $"attend {step.eventId}{(string.IsNullOrEmpty(step.eventArg) ? "" : ":" + step.eventArg)}"; break;
                case AdvanceCondition.WaitForTargetClick: advance = "attend un clic sur la cible"; break;
                default: advance = step.advanceCondition.ToString(); break;
            }
            string targets = step.targetIds.Count > 0 ? $" · éclaire {string.Join(", ", step.targetIds)}" : "";
            return advance + targets;
        }

        private void DrawStepButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Nouvelle étape"))
                AddNewStep();
            using (new EditorGUI.DisabledScope(_selectedStep == null))
            {
                if (GUILayout.Button("Dupliquer"))
                    DuplicateSelectedStep();
                if (GUILayout.Button("Supprimer"))
                    RemoveSelectedStep();
            }
            EditorGUILayout.EndHorizontal();
        }

        private string StepsFolder()
        {
            string lessonPath = AssetDatabase.GetAssetPath(_lesson);
            string parent = Path.GetDirectoryName(lessonPath).Replace('\\', '/');
            string folder = $"{parent}/{_lesson.name}_Steps";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(parent, _lesson.name + "_Steps");
            return folder;
        }

        private void AddNewStep()
        {
            int number = _lesson.steps.Count + 1;
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{StepsFolder()}/{_lesson.name}_{number:00}.asset");
            var step = CreateInstance<TutorialStepData>();
            step.stepId = $"{_lesson.lessonId}_{number:00}";
            AssetDatabase.CreateAsset(step, assetPath);
            AssetDatabase.SaveAssets();

            int insertAt = _selectedStep != null ? _lesson.steps.IndexOf(_selectedStep) + 1 : _lesson.steps.Count;
            Undo.RecordObject(_lesson, "Ajouter une étape");
            _lesson.steps.Insert(insertAt, step);
            EditorUtility.SetDirty(_lesson);
            RebuildList();
            _selectedStep = step;
            _list.index = insertAt;
        }

        private void DuplicateSelectedStep()
        {
            string path = AssetDatabase.GetAssetPath(_selectedStep);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CopyAsset(path, newPath);
            var copy = AssetDatabase.LoadAssetAtPath<TutorialStepData>(newPath);
            copy.stepId = _selectedStep.stepId + "_copie";
            EditorUtility.SetDirty(copy);

            int index = _lesson.steps.IndexOf(_selectedStep);
            Undo.RecordObject(_lesson, "Dupliquer l'étape");
            _lesson.steps.Insert(index + 1, copy);
            EditorUtility.SetDirty(_lesson);
            RebuildList();
            _selectedStep = copy;
            _list.index = index + 1;
        }

        private void RemoveSelectedStep()
        {
            int choice = EditorUtility.DisplayDialogComplex(
                "Supprimer l'étape",
                $"Retirer \"{_selectedStep.name}\" de la leçon. Supprimer aussi le fichier .asset ?",
                "Retirer et supprimer le fichier", "Annuler", "Retirer seulement");
            if (choice == 1)
                return;

            Undo.RecordObject(_lesson, "Supprimer une étape");
            _lesson.steps.Remove(_selectedStep);
            EditorUtility.SetDirty(_lesson);
            if (choice == 0)
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_selectedStep));
            _selectedStep = null;
            RebuildList();
        }

        // ------------------------------------------------------- Mise en place

        private void DrawLessonSetup()
        {
            _lessonSO.Update();
            EditorGUILayout.LabelField("Leçon", EditorStyles.boldLabel);
            foreach (var name in new[] { "displayName", "description", "icon", "sceneName", "requiredLessonIds", "endCondition", "allowSkip", "skipCountsAsCompleted" })
                EditorGUILayout.PropertyField(_lessonSO.FindProperty(name), true);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Partie scriptée (remplace la seed)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Ordres de cartes du DESSUS au DESSOUS de la pile. Les 3 premières cartes du deck forment la main de départ.", MessageType.None);
            EditorGUILayout.PropertyField(_lessonSO.FindProperty("matchSetup"), true);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Adversaire scripté", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_lessonSO.FindProperty("opponent"), true);
            _lessonSO.ApplyModifiedProperties();
        }

        // ------------------------------------------------------- Détail d'une étape

        private void DrawStepDetail()
        {
            if (_selectedStep == null)
            {
                EditorGUILayout.HelpBox("Sélectionnez une étape dans la liste pour l'éditer.", MessageType.None);
                return;
            }
            if (_stepSO == null || _stepSO.targetObject != _selectedStep)
                _stepSO = new SerializedObject(_selectedStep);
            _stepSO.Update();

            EditorGUILayout.LabelField(_selectedStep.name, EditorStyles.boldLabel);
            DrawPreview();

            foreach (var problem in _selectedStep.Validate())
                EditorGUILayout.HelpBox(problem, MessageType.Warning);
            DrawMissingTargets();

            EditorGUILayout.Space(6);
            var prop = _stepSO.GetIterator();
            prop.NextVisible(true); // m_Script
            while (prop.NextVisible(false))
            {
                if (!IsRelevant(prop.name))
                    continue;
                EditorGUILayout.PropertyField(prop, true);
            }
            _stepSO.ApplyModifiedProperties();
        }

        /// Masque les champs sans effet pour la condition / le style choisis.
        private bool IsRelevant(string field)
        {
            var s = _selectedStep;
            switch (field)
            {
                case "eventId":
                case "eventArg":
                case "eventCount":
                    return s.advanceCondition == AdvanceCondition.WaitForEvent;
                case "delaySeconds":
                    return s.advanceCondition == AdvanceCondition.WaitForDelay;
                case "pointerDropTargetId":
                    return s.pointer == PointerMode.Drag;
                case "allowedActions":
                    return s.restrictGameActions;
                case "title":
                case "body":
                case "portrait":
                case "placement":
                    return s.popupStyle != PopupStyle.None;
                default:
                    return true;
            }
        }

        private void DrawPreview()
        {
            if (_selectedStep.popupStyle == PopupStyle.None)
                return;
            var style = new GUIStyle(EditorStyles.helpBox) { fontSize = 12, wordWrap = true, richText = true, padding = new RectOffset(12, 12, 10, 10) };
            string title = string.IsNullOrEmpty(_selectedStep.title.text) ? "" : $"<b>{_selectedStep.title.text}</b>\n";
            string body = string.IsNullOrEmpty(_selectedStep.body.text) ? "<i>(texte vide)</i>" : _selectedStep.body.text;

            EditorGUILayout.BeginHorizontal();
            if (_selectedStep.portrait != null)
            {
                var tex = AssetPreview.GetAssetPreview(_selectedStep.portrait) ?? AssetPreview.GetMiniThumbnail(_selectedStep.portrait);
                GUILayout.Label(tex, GUILayout.Width(48), GUILayout.Height(48));
            }
            EditorGUILayout.LabelField(title + body, style, GUILayout.MinHeight(48));
            EditorGUILayout.EndHorizontal();
        }

        /// Prévient des fautes de frappe dans les IDs de cibles, d'après la scène ouverte.
        /// Les cibles créées en jeu (cartes en main) ne peuvent pas être vérifiées ici.
        private void DrawMissingTargets()
        {
            if (_selectedStep.targetIds.Count == 0)
                return;
#if UNITY_2023_1_OR_NEWER
            var sceneTargets = FindObjectsByType<TutorialTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var sceneTargets = FindObjectsOfType<TutorialTarget>(true);
#endif
            var ids = sceneTargets.Select(t => t.TargetId).ToHashSet();
            var missing = _selectedStep.targetIds.Where(id => !ids.Contains(id) && !id.Contains(":")).ToList();
            if (missing.Count > 0)
                EditorGUILayout.HelpBox(
                    $"Cible(s) absente(s) de la scène ouverte : {string.Join(", ", missing)}. Vérifiez l'orthographe ou ouvrez la scène de partie. " +
                    "(Les IDs avec \":\", comme \"hand:ouvriere\", sont des cartes créées en jeu et ne sont pas vérifiés.)",
                    MessageType.Warning);
        }
    }
}
