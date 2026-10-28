using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// Tutorial manager: starts a lesson, runs through its steps and related sequences, displays hints, and tracks the player's progress.
    ///
    /// Game-side usage:
    /// <code>
    ///   TutorialManager.Instance.Initialize(new JsonFileTutorialProgressStore(), profile.Id);
    ///   TutorialManager.Instance.StartNextLesson();   // "Tutorial" button
    ///   TutorialManager.Instance.StartLesson(lesson); // lesson selection menu
    /// </code>
    /// </summary>
    public class TutorialManager : MonoBehaviour {

        public static TutorialManager Instance { get; private set; }

        [SerializeField]
        private TutorialCampaignData _Campaign;

        [SerializeField]
        private TutorialUIController _Ui;

        [SerializeField][Tooltip("The game's MonoBehaviour that implements ITutorialGameBridge. It can also be provided by SetBridge().")]
        private MonoBehaviour _BridgeBehaviour;

        [SerializeField][Tooltip("Keep the tutorial open between scenes (Menu > Game Scene).")]
        private bool _PersistAcrossScenes = true;

        [SerializeField][Tooltip("True if, in the game's code, index 0 of a shuffled deck is the next card to be drawn.")]
        private bool _TopOfPileIsIndexZero = true;

        [SerializeField][Tooltip("Maximum wait time for a target to appear (object currently animating...).")]
        private float _TargetWaitTimeout = 3f;

        [SerializeField]
        private bool _VerboseLogs;

        private ITutorialGameBridge     _Bridge;
        private TutorialProgressService _Progress;
        private Coroutine               _LessonRoutine;
        private IGameRandom             _PreviousRandom;
        private TutorialHints           _Hints;
        private TutorialTargetScanner   _Scanner;
        private TutorialLessonData      _Lesson;
        private TutorialStepData        _Step;

        private int  _NextPresses;
        private bool _SkipRequested;
        private bool _Failed;
        private bool _CompleteRequested;
        private bool _GameEnded;

        /// <summary>Side sequences requested by the game, played as soon as the current step is waiting for the player.</summary>
        private readonly List<string> _InterludeQueue = new();

        private string _RunningInterlude;

        public TutorialCampaignData Campaign    => _Campaign;
        public TutorialUIController UI          => _Ui;
        public TutorialProgressService Progress => EnsureProgress();

        /// <summary>Hints for the current lesson (TutorialLessonData.Hints).</summary>
        public TutorialHints Hints => _Hints ??= new TutorialHints(this);

        /// <summary>Set the targets for the objects created by the game (see TutorialTargetScanner.AddProvider).</summary>
        public TutorialTargetScanner Scanner    => EnsureScanner();
        public TutorialLessonData CurrentLesson => _Lesson;
        public TutorialStepData CurrentStep     => _Step;
        public bool IsRunning                   => _Lesson != null;

        /// <summary>ID of the side sequence being played; null otherwise.</summary>
        public string RunningInterlude => _RunningInterlude;

        public event Action<TutorialLessonData> LessonStarted;

        /// <summary>(lesson, skipped by the player)</summary>
        public event Action<TutorialLessonData, bool> LessonCompleted;
        public event Action<TutorialLessonData>       LessonAborted;

        /// <summary>(step, index within the lesson or within its side sequence)</summary>
        public event Action<TutorialStepData, int> StepStarted;

        /// <summary>A step comes back after a side sequence (its targets may need to be looked up again).</summary>
        public event Action<TutorialStepData> StepResumed;

        /// <summary>(side sequence ID): its steps are complete.</summary>
        public event Action<string> InterludeFinished;

        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);

                return;
            }

            Instance = this;

            if (_PersistAcrossScenes)
                DontDestroyOnLoad(gameObject);
            _Hints ??= new TutorialHints(this);

            if (_BridgeBehaviour != null) {
                _Bridge = _BridgeBehaviour as ITutorialGameBridge;

                if (_Bridge == null)
                    Debug.LogError($"[Tutorial] {_BridgeBehaviour.name} does not implement ITutorialGameBridge.", this);
            }

            WireUI();
        }

        private void WireUI()
        {
            if (_Ui == null)
                return;
            _Ui.NextPressed -= OnNextPressed;
            _Ui.NextPressed += OnNextPressed;
            _Ui.SkipPressed -= SkipCurrentLesson;
            _Ui.SkipPressed += SkipCurrentLesson;
        }

        private void OnNextPressed() => _NextPresses++;

        private void OnDestroy()
        {
            if (Instance != this)
                return;
            if (IsRunning)
                FinishLesson(false, false);
            Instance = null;
        }

        #region Configuration

        /// <summary>
        /// Call this when the player's profile is known (after logging in or selecting a profile).
        /// If not called, the progress will be saved in the "default" profile's PlayerPrefs.
        /// </summary>
        public void Initialize(ITutorialProgressStore store, string profileId)
        {
            _Progress = new TutorialProgressService(store, profileId);
        }

        public void SetBridge(ITutorialGameBridge bridge) => _Bridge = bridge;

        public void SetCampaign(TutorialCampaignData newCampaign) => _Campaign = newCampaign;

        public void SetUI(TutorialUIController controller)
        {
            _Ui = controller;

            WireUI();
        }

        public bool VerboseLogs
        {
            get => _VerboseLogs;
            set => _VerboseLogs = value;
        }

        private TutorialProgressService EnsureProgress()
        {
            if (_Progress == null)
                _Progress = new TutorialProgressService(new PlayerPrefsTutorialProgressStore(), "default");
            return _Progress;
        }

        #endregion

        #region Public API

        /// <summary>True if the tutorial is supposed to start on its own (first launch).</summary>
        public bool ShouldAutoStart() => _Campaign != null && _Campaign.AutoStartOnFirstLaunch && !_Campaign.IsFinished(EnsureProgress());

        public bool IsUnlocked(TutorialLessonData lesson) => _Campaign == null || _Campaign.IsUnlocked(lesson, EnsureProgress());

        /// <summary>Launch the first unfinished lesson in the campaign. Return "false" if everything is complete.</summary>
        public bool StartNextLesson()
        {
            if (_Campaign == null)
                return false;
            var next = _Campaign.GetNextLesson(EnsureProgress());

            if (next == null)
                return false;
            StartLesson(next);

            return true;
        }

        public void StartLesson(TutorialLessonData lesson)
        {
            if (lesson == null)
                return;
            if (IsRunning)
                AbortLesson();
            _LessonRoutine = StartCoroutine(RunLesson(lesson));
        }

        /// <summary>The player skips the current lesson (using the "Skip" button).</summary>
        public void SkipCurrentLesson()
        {
            if (IsRunning && _Lesson.AllowSkip)
                _SkipRequested = true;
        }

        /// <summary>Exit the lesson without confirming it (return to the menu). It remains "in progress."</summary>
        public void AbortLesson()
        {
            if (!IsRunning)
                return;
            if (_LessonRoutine != null)
                StopCoroutine(_LessonRoutine);
            _LessonRoutine = null;

            var lesson = _Lesson;

            FinishLesson(false, false);

            LessonAborted?.Invoke(lesson);
        }

        /// <summary>The player chooses not to do the tutorial at all.</summary>
        public void SkipWholeTutorial()
        {
            AbortLesson();
            EnsureProgress().SkipTutorial();
        }

        /// <summary>
        /// The lesson is considered successful immediately (objective achieved in free play, game won...), without waiting for all the steps to be completed.
        /// </summary>
        public void CompleteCurrentLesson()
        {
            if (!IsRunning)
                return;
            _CompleteRequested = true;
            _SkipRequested     = true;
        }

        /// <summary>
        /// The lesson has failed (game over...): it ends without being completed, as if the player had quit.
        /// The game may first play a side sequence explaining this.
        /// </summary>
        public void FailCurrentLesson()
        {
            if (!IsRunning)
                return;
            _Failed        = true;
            _SkipRequested = true;
        }

        /// <summary>
        /// Requests a side sequence of the current lesson (help after a forbidden move,
        /// a tip...). It plays over the current step as soon as that step is waiting for
        /// the player, then the step resumes. Returns false if the lesson has no such sequence,
        /// or if it is already running or pending.
        /// </summary>
        public bool PlayInterlude(string interludeId)
        {
            if (!IsRunning || string.IsNullOrEmpty(interludeId))
                return false;
            if (_Lesson.FindInterlude(interludeId) == null) {
                Debug.LogWarning($"[Tutorial] Lesson \"{_Lesson.LessonId}\": No related sequence \"{interludeId}\".", this);

                return false;
            }

            if (_RunningInterlude == interludeId || _InterludeQueue.Contains(interludeId))
                return false;
            _InterludeQueue.Add(interludeId);

            Log($"Request for additional sequence \"{interludeId}\".");

            return true;
        }

        #endregion

        #region Lesson Outline

        private IEnumerator RunLesson(TutorialLessonData lesson)
        {
            _Lesson            = lesson;
            _SkipRequested     = false;
            _Failed            = false;
            _CompleteRequested = false;
            _GameEnded         = false;

            _InterludeQueue.Clear();

            _RunningInterlude = null;

            EnsureProgress().MarkStarted(lesson.LessonId);

            Log($"Lesson \"{lesson.LessonId}\": start.");

            if (!string.IsNullOrEmpty(lesson.SceneName) && SceneManager.GetActiveScene().name != lesson.SceneName) {
                var load = SceneManager.LoadSceneAsync(lesson.SceneName);

                while (load != null && !load.isDone)
                    yield return null;
            }

            if (_Bridge == null || (_Bridge is UnityEngine.Object o && o == null))
                _Bridge = FindBridgeInScene();
            if (_Bridge == null) {
                Debug.LogError("[Tutorial] No ITutorialGameBridge found: unable to prepare the lesson.", this);

                FinishLesson(false, false);

                yield break;
            }

            // Random order replaced with the scripted order of the lesson.
            _PreviousRandom = GameRandom.Current;

            var scripted = ScriptedGameRandom.FromSetup(lesson.Random, _TopOfPileIsIndexZero);

            scripted.OnWarning += msg => Debug.LogWarning(msg, this);
            GameRandom.Current  = scripted;

            TutorialSession.Begin(lesson, scripted);

            TutorialEventBus.OnEvent += OnGameEvent;
            EnsureScanner().Scanning  = true;

            // During setup, the player cannot do anything.
            TutorialInputGate.Restrict(null);

            yield return _Bridge.PrepareLesson(lesson);

            TutorialInputGate.Release();

            Hints.Begin(lesson.Hints);
            LessonStarted?.Invoke(lesson);

            for (int i = 0; i < lesson.Steps.Count && !_SkipRequested; i++) {
                var step = lesson.Steps[i];

                if (step == null)
                    continue;
                if (_InterludeQueue.Count > 0)
                    yield return RunInterludes();
                yield return RunStep(step, i);
            }

            if (!_SkipRequested && lesson.EndCondition == LessonEndCondition.OnGameEnd) {
                if (_Ui != null)
                    _Ui.HideAll();
                while (!_GameEnded && !_SkipRequested) {
                    if (_InterludeQueue.Count > 0) {
                        yield return RunInterludes();

                        if (_Ui != null)
                            _Ui.HideAll();
                    }

                    yield return null;
                }
            }

            if (_Failed) {
                Log($"Lesson \"{lesson.LessonId}\": failed.");

                FinishLesson(false, false);
                LessonAborted?.Invoke(lesson);

                yield break;
            }

            bool skipped = _SkipRequested && !_CompleteRequested;

            FinishLesson(!skipped || lesson.SkipCountsAsCompleted, skipped);

            LessonCompleted?.Invoke(lesson, skipped);
        }

        private IEnumerator RunStep(TutorialStepData step, int index)
        {
            _Step = step;

            Log($"Step {index + 1} \"{step.StepId}\".");

            StepStarted?.Invoke(step, index);

            foreach (var cmd in step.OnEnter)
                yield return ExecuteCommand(cmd);
            if (_SkipRequested)
                yield break;
            ApplyGate(step);

            if (_Scanner != null)
                _Scanner.ScanNow();
            // Targets may appear with a slight delay (object is currently animating).
            var  targets = new List<TutorialTarget>();
            float waited = 0f;

            while (true) {
                CollectTargets(step, targets);

                bool allFound = true;

                foreach (var t in targets)
                    allFound &= t != null && t.IsVisible;
                if (allFound || waited >= _TargetWaitTimeout)
                    break;
                waited += Time.unscaledDeltaTime;

                yield return null;
            }

            for (int i = 0; i < targets.Count; i++) {
                if (targets[i] == null)
                    Debug.LogWarning($"[Tutorial] Step \"{step.StepId}\": target \"{step.TargetIds[i]}\" not found in the scene.", this);
            }

            var wait = new EventWait();

            if (step.AdvanceCondition == AdvanceCondition.WaitForEvent) {
                wait.Id  = step.EventId;
                wait.Arg = step.EventArg;
            } else if (step.AdvanceCondition == AdvanceCondition.WaitForTargetClick) {
                wait.Id  = TutorialEvents.TargetClicked;
                wait.Arg = step.MainTargetId;
            }

            if (wait.Id != null)
                _Waits.Add(wait);
            int presses = _NextPresses;

            if (_Ui != null)
                _Ui.ShowStep(step, targets, DropTarget(step), _Lesson.AllowSkip);
            // A sub-sequence does not interrupt itself: the subsequent ones wait their turn.
            bool interruptible = _RunningInterlude == null;
            float      elapsed = 0f;

            while (!_SkipRequested && !IsStepDone(step, wait, presses, elapsed)) {
                if (interruptible && _InterludeQueue.Count > 0) {
                    if (_Ui != null)
                        _Ui.HideStep();
                    yield return RunInterludes();

                    if (_SkipRequested)
                        break;
                    // The step resumes where it left off: same rules, same pop-up.
                    _Step = step;

                    ApplyGate(step);

                    presses = _NextPresses;

                    StepResumed?.Invoke(step);

                    if (_Scanner != null)
                        _Scanner.ScanNow();
                    CollectTargets(step, targets);

                    if (_Ui != null)
                        _Ui.ShowStep(step, targets, DropTarget(step), _Lesson.AllowSkip);
                    continue;
                }

                yield return null;

                elapsed += Time.unscaledDeltaTime;
            }

            _Waits.Remove(wait);

            if (step.AdvanceCondition == AdvanceCondition.WaitForButton && !_SkipRequested)
                TutorialEventBus.Publish(TutorialEvents.PopupClosed, step.StepId);
            if (_Ui != null)
                _Ui.HideStep();
            TutorialInputGate.Release();

            if (_SkipRequested)
                yield break;
            foreach (var cmd in step.OnExit)
                yield return ExecuteCommand(cmd);
            _Step = null;
        }

        private bool IsStepDone(TutorialStepData step, EventWait wait, int presses, float elapsed)
        {
            switch (step.AdvanceCondition) {
                case AdvanceCondition.WaitForDelay:
                    return elapsed >= step.DelaySeconds;
                case AdvanceCondition.WaitForButton:
                    return _NextPresses > presses;
                case AdvanceCondition.WaitForEvent:
                    return wait.Count >= Mathf.Max(1, step.EventCount);
                case AdvanceCondition.WaitForTargetClick:
                    return wait.Count >= 1;
                default:
                    return true;
            }
        }

        private static void ApplyGate(TutorialStepData step)
        {
            if (step.RestrictGameActions)
                TutorialInputGate.Restrict(step.AllowedActions);
            else
                TutorialInputGate.Release();
        }

        private static void CollectTargets(TutorialStepData step, List<TutorialTarget> targets)
        {
            targets.Clear();

            foreach (var id in step.TargetIds)
                targets.Add(TutorialTargetRegistry.GetOrResolve(id));
        }

        private static TutorialTarget DropTarget(TutorialStepData step) => string.IsNullOrEmpty(step.PointerDropTargetId) ? null : TutorialTargetRegistry.GetOrResolve(step.PointerDropTargetId);

        /// <summary>Play the pending sub-sequences one after another.</summary>
        private IEnumerator RunInterludes()
        {
            while (_InterludeQueue.Count > 0 && !_SkipRequested) {
                string id = _InterludeQueue[0];

                _InterludeQueue.RemoveAt(0);

                var interlude = _Lesson.FindInterlude(id);

                if (interlude == null)
                    continue;
                _RunningInterlude = id;

                Log($"Supplementary Sequence \"{id}\": start.");

                for (int i = 0; i < interlude.Steps.Count && !_SkipRequested; i++) {
                    if (interlude.Steps[i] != null)
                        yield return RunStep(interlude.Steps[i], i);
                }

                _RunningInterlude = null;

                InterludeFinished?.Invoke(id);
            }
        }

        #endregion

        /// <summary>
        /// What a step expects: an event (and the number of occurrences received).
        /// A step interrupted by a side sequence retains its own count and continues to count.
        /// </summary>
        private sealed class EventWait {

            public string  Id;
            public string Arg;
            public int  Count;
        }

        private readonly List<EventWait> _Waits = new();

        private void OnGameEvent(TutorialEvent e)
        {
            if (_VerboseLogs)
                Debug.Log($"[Tutorial] Event {e}");
            if (e.Id == TutorialEvents.GameEnded)
                _GameEnded = true;
            foreach (var wait in _Waits) {
                if (e.Matches(wait.Id, wait.Arg))
                    wait.Count++;
            }
        }

        private IEnumerator ExecuteCommand(TutorialCommand cmd)
        {
            if (cmd == null || string.IsNullOrEmpty(cmd.Name))
                yield break;
            Log($"Command {cmd}");

            if (cmd.Is(TutorialCommands.Wait)) {
                float seconds = cmd.GetFloat("seconds", "secondes", "duree");

                for (float t = 0; t < seconds && !_SkipRequested; t += Time.unscaledDeltaTime)
                    yield return null;
                yield break;
            }

            if (cmd.Is(TutorialCommands.HideElement) || cmd.Is(TutorialCommands.ShowElement)) {
                TutorialTargetRegistry.SetHidden(cmd.Get("target", "cible"), cmd.Is(TutorialCommands.HideElement));

                yield break;
            }

            if (cmd.Is(TutorialCommands.PublishEvent)) {
                TutorialEventBus.Publish(cmd.Get("event", "evenement", "id"), cmd.Get("arg", "argument"), cmd.GetInt("value", "valeur", "amount", "nombre"));

                yield break;
            }

            if (cmd.Is(TutorialCommands.PlayInterlude)) {
                PlayInterlude(cmd.Get("id", "interlude", "sequence"));

                yield break;
            }

            if (cmd.Is(TutorialCommands.PauseAgents) || cmd.Is(TutorialCommands.ResumeAgents)) {
                TutorialSession.AgentsPaused = cmd.Is(TutorialCommands.PauseAgents);

                yield break;
            }

            // Game commands are not filtered by the TutorialInputGate.
            bool wasRestricted = TutorialInputGate.IsRestricted;
            var allowed        = new List<AllowedAction>(TutorialInputGate.Allowed);

            TutorialInputGate.Release();

            yield return _Bridge.ExecuteCommand(cmd);

            if (wasRestricted)
                TutorialInputGate.Restrict(allowed);
        }

        private void FinishLesson(bool completed, bool skipped)
        {
            var lesson = _Lesson;

            TutorialEventBus.OnEvent -= OnGameEvent;
            if (_Hints != null)
                _Hints.End();
            if (_Scanner != null)
                _Scanner.Scanning = false;
            if (_Ui != null)
                _Ui.HideAll();
            TutorialInputGate.Release();
            TutorialTargetRegistry.RevealAll();

            if (_PreviousRandom != null)
                GameRandom.Current = _PreviousRandom;
            _PreviousRandom = null;

            TutorialSession.End();

            if (lesson != null && completed)
                EnsureProgress().MarkCompleted(lesson.LessonId);
            _Lesson = null;
            _Step   = null;

            _Waits.Clear();
            _InterludeQueue.Clear();

            _RunningInterlude = null;
            _LessonRoutine    = null;

            if (lesson != null && _Bridge != null && !(_Bridge is UnityEngine.Object o && o == null))
                _Bridge.CleanupLesson(lesson, completed);
            Log($"Lesson \"{lesson?.LessonId}\": end (completed={completed}, skipped={skipped}).");
        }

        private TutorialTargetScanner EnsureScanner()
        {
            if (_Scanner == null && !TryGetComponent(out _Scanner))
                _Scanner = gameObject.AddComponent<TutorialTargetScanner>();
            return _Scanner;
        }

        private static ITutorialGameBridge FindBridgeInScene()
        {
            #if UNITY_2023_1_OR_NEWER
                var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            #else
                var behaviours = FindObjectsOfType<MonoBehaviour>();
            #endif

            foreach (var b in behaviours) {
                if (b is ITutorialGameBridge bridge)
                    return bridge;
            }

            return null;
        }

        private void Log(string message)
        {
            if (_VerboseLogs)
                Debug.Log($"[Tutorial] {message}", this);
        }
    }
}
