using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vermines.Tutorial
{
    /// Chef d'orchestre du tutoriel : lance une leçon (micro-partie scriptée), déroule
    /// ses étapes, et enregistre la progression dans le profil du joueur.
    ///
    /// Utilisation côté jeu :
    ///   TutorialManager.Instance.Initialize(new JsonFileTutorialProgressStore(), profile.Id);
    ///   TutorialManager.Instance.StartNextLesson();          // bouton "Didacticiel"
    ///   TutorialManager.Instance.StartLesson(lesson);         // menu de sélection des leçons
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [SerializeField] private TutorialCampaignData campaign;
        [SerializeField] private TutorialUIController ui;

        [Tooltip("Le MonoBehaviour du jeu qui implémente ITutorialGameBridge. Peut aussi être fourni par SetBridge().")]
        [SerializeField] private MonoBehaviour bridgeBehaviour;

        [Tooltip("Garder le tutoriel entre les scènes (menu → scène de partie).")]
        [SerializeField] private bool persistAcrossScenes = true;

        [Tooltip("Vrai si, dans le code du jeu, l'index 0 d'une pile est la prochaine carte piochée.")]
        [SerializeField] private bool topOfPileIsIndexZero = true;

        [Tooltip("Temps max d'attente qu'une cible apparaisse (carte en cours d'animation...).")]
        [SerializeField] private float targetWaitTimeout = 3f;

        [SerializeField] private bool verboseLogs;

        private ITutorialGameBridge _bridge;
        private TutorialProgressService _progress;
        private Coroutine _lessonRoutine;
        private IShuffleService _previousRandom;
        private TutorialLessonData _lesson;
        private TutorialStepData _step;
        private bool _nextPressed;
        private bool _skipRequested;
        private bool _matchEnded;

        public TutorialCampaignData Campaign => campaign;
        public TutorialUIController UI => ui;
        public TutorialProgressService Progress => _progress;
        public TutorialLessonData CurrentLesson => _lesson;
        public TutorialStepData CurrentStep => _step;
        public bool IsRunning => _lesson != null;

        public event Action<TutorialLessonData> LessonStarted;
        /// (leçon, passée par le joueur)
        public event Action<TutorialLessonData, bool> LessonCompleted;
        public event Action<TutorialLessonData> LessonAborted;
        /// (étape, index dans la leçon)
        public event Action<TutorialStepData, int> StepStarted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (persistAcrossScenes)
                DontDestroyOnLoad(gameObject);

            if (bridgeBehaviour != null)
            {
                _bridge = bridgeBehaviour as ITutorialGameBridge;
                if (_bridge == null)
                    Debug.LogError($"[Tutoriel] {bridgeBehaviour.name} n'implémente pas ITutorialGameBridge.", this);
            }

            WireUI();
        }

        private void WireUI()
        {
            if (ui == null)
                return;
            ui.NextPressed -= OnNextPressed;
            ui.NextPressed += OnNextPressed;
            ui.SkipPressed -= SkipCurrentLesson;
            ui.SkipPressed += SkipCurrentLesson;
        }

        private void OnNextPressed() => _nextPressed = true;

        private void OnDestroy()
        {
            if (Instance != this)
                return;
            if (IsRunning)
                FinishLesson(false, false);
            Instance = null;
        }

        // ------------------------------------------------------------- Configuration

        /// À appeler quand le profil du joueur est connu (après connexion / choix du
        /// profil). Sans appel, la progression va dans les PlayerPrefs du profil "default".
        public void Initialize(ITutorialProgressStore store, string profileId)
        {
            _progress = new TutorialProgressService(store, profileId);
        }

        public void SetBridge(ITutorialGameBridge bridge) => _bridge = bridge;

        public void SetCampaign(TutorialCampaignData newCampaign) => campaign = newCampaign;

        public void SetUI(TutorialUIController controller)
        {
            ui = controller;
            WireUI();
        }

        public bool VerboseLogs
        {
            get => verboseLogs;
            set => verboseLogs = value;
        }

        private TutorialProgressService EnsureProgress()
        {
            if (_progress == null)
                _progress = new TutorialProgressService(new PlayerPrefsTutorialProgressStore(), "default");
            return _progress;
        }

        // -------------------------------------------------------------- API publique

        /// Vrai si le tutoriel doit se lancer tout seul (premier démarrage).
        public bool ShouldAutoStart() =>
            campaign != null && campaign.autoStartOnFirstLaunch && !campaign.IsFinished(EnsureProgress());

        public bool IsUnlocked(TutorialLessonData lesson) => campaign == null || campaign.IsUnlocked(lesson, EnsureProgress());

        /// Lance la première leçon non terminée de la campagne. Renvoie faux si tout est fini.
        public bool StartNextLesson()
        {
            if (campaign == null)
                return false;
            var next = campaign.GetNextLesson(EnsureProgress());
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
            _lessonRoutine = StartCoroutine(RunLesson(lesson));
        }

        /// Le joueur passe la leçon en cours (bouton "Passer").
        public void SkipCurrentLesson()
        {
            if (IsRunning && _lesson.allowSkip)
                _skipRequested = true;
        }

        /// Quitte la leçon sans la valider (retour au menu). Elle reste "en cours".
        public void AbortLesson()
        {
            if (!IsRunning)
                return;
            if (_lessonRoutine != null)
                StopCoroutine(_lessonRoutine);
            _lessonRoutine = null;
            var lesson = _lesson;
            FinishLesson(false, false);
            LessonAborted?.Invoke(lesson);
        }

        /// Le joueur choisit de ne pas faire le tutoriel du tout.
        public void SkipWholeTutorial()
        {
            AbortLesson();
            EnsureProgress().SkipTutorial();
        }

        // ------------------------------------------------------------ Déroulé leçon

        private IEnumerator RunLesson(TutorialLessonData lesson)
        {
            _lesson = lesson;
            _skipRequested = false;
            _matchEnded = false;
            EnsureProgress().MarkStarted(lesson.lessonId);
            Log($"Leçon \"{lesson.lessonId}\" : début.");

            if (!string.IsNullOrEmpty(lesson.sceneName) && SceneManager.GetActiveScene().name != lesson.sceneName)
            {
                var load = SceneManager.LoadSceneAsync(lesson.sceneName);
                while (load != null && !load.isDone)
                    yield return null;
            }

            if (_bridge == null || (_bridge is UnityEngine.Object o && o == null))
                _bridge = FindBridgeInScene();
            if (_bridge == null)
            {
                Debug.LogError("[Tutoriel] Aucun ITutorialGameBridge trouvé : impossible de préparer la partie scriptée.", this);
                FinishLesson(false, false);
                yield break;
            }

            // Hasard remplacé par l'ordre scripté de la leçon.
            _previousRandom = GameRandom.Current;
            var scripted = ScriptedShuffleService.FromSetup(lesson.matchSetup, topOfPileIsIndexZero);
            scripted.OnWarning += msg => Debug.LogWarning(msg, this);
            GameRandom.Current = scripted;
            TutorialSession.Begin(lesson.lessonId, lesson.matchSetup, lesson.opponent);
            TutorialEventBus.OnEvent += OnGameEvent;

            // Pendant la mise en place, le joueur ne peut rien faire.
            TutorialInputGate.Restrict(null);
            yield return _bridge.StartScriptedMatch(lesson.matchSetup);
            TutorialInputGate.Release();

            LessonStarted?.Invoke(lesson);

            for (int i = 0; i < lesson.steps.Count && !_skipRequested; i++)
            {
                var step = lesson.steps[i];
                if (step == null)
                    continue;
                yield return RunStep(step, i);
            }

            if (!_skipRequested && lesson.endCondition == LessonEndCondition.OnMatchEnd)
            {
                if (ui != null) ui.HideAll();
                while (!_matchEnded && !_skipRequested)
                    yield return null;
            }

            bool skipped = _skipRequested;
            FinishLesson(!skipped || lesson.skipCountsAsCompleted, skipped);
            LessonCompleted?.Invoke(lesson, skipped);
        }

        private IEnumerator RunStep(TutorialStepData step, int index)
        {
            _step = step;
            Log($"Étape {index + 1} \"{step.stepId}\".");
            StepStarted?.Invoke(step, index);

            foreach (var cmd in step.onEnter)
                yield return ExecuteCommand(cmd);
            if (_skipRequested)
                yield break;

            if (step.restrictGameActions)
                TutorialInputGate.Restrict(step.allowedActions);
            else
                TutorialInputGate.Release();

            // Les cibles peuvent apparaître avec un peu de retard (carte en train d'être piochée).
            var targets = new List<TutorialTarget>();
            float waited = 0f;
            while (true)
            {
                targets.Clear();
                bool allFound = true;
                foreach (var id in step.targetIds)
                {
                    var t = TutorialTargetRegistry.Get(id);
                    targets.Add(t);
                    allFound &= t != null && t.IsVisible;
                }
                if (allFound || waited >= targetWaitTimeout)
                    break;
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
            for (int i = 0; i < targets.Count; i++)
                if (targets[i] == null)
                    Debug.LogWarning($"[Tutoriel] Étape \"{step.stepId}\" : cible \"{step.targetIds[i]}\" introuvable dans la scène.", this);

            var drop = string.IsNullOrEmpty(step.pointerDropTargetId) ? null : TutorialTargetRegistry.Get(step.pointerDropTargetId);

            _nextPressed = false;
            _pendingEvents = 0;
            _awaitedEventId = null;
            _awaitedEventArg = null;
            if (step.advanceCondition == AdvanceCondition.WaitForEvent)
            {
                _awaitedEventId = step.eventId;
                _awaitedEventArg = step.eventArg;
            }
            else if (step.advanceCondition == AdvanceCondition.WaitForTargetClick)
            {
                _awaitedEventId = TutorialEvents.TargetClicked;
                _awaitedEventArg = step.MainTargetId;
            }

            if (ui != null) ui.ShowStep(step, targets, drop, _lesson.allowSkip);

            switch (step.advanceCondition)
            {
                case AdvanceCondition.Immediate:
                    break;
                case AdvanceCondition.WaitForDelay:
                    for (float t = 0; t < step.delaySeconds && !_skipRequested; t += Time.unscaledDeltaTime)
                        yield return null;
                    break;
                case AdvanceCondition.WaitForButton:
                    while (!_nextPressed && !_skipRequested)
                        yield return null;
                    TutorialEventBus.Publish(TutorialEvents.PopupClosed, step.stepId);
                    break;
                case AdvanceCondition.WaitForEvent:
                case AdvanceCondition.WaitForTargetClick:
                    int needed = Mathf.Max(1, step.advanceCondition == AdvanceCondition.WaitForEvent ? step.eventCount : 1);
                    while (_pendingEvents < needed && !_skipRequested)
                        yield return null;
                    break;
            }

            _awaitedEventId = null;
            if (ui != null) ui.HideStep();
            TutorialInputGate.Release();

            if (_skipRequested)
                yield break;
            foreach (var cmd in step.onExit)
                yield return ExecuteCommand(cmd);
            _step = null;
        }

        private string _awaitedEventId;
        private string _awaitedEventArg;
        private int _pendingEvents;

        private void OnGameEvent(TutorialEvent e)
        {
            if (verboseLogs)
                Debug.Log($"[Tutoriel] Événement {e}");
            if (e.Id == TutorialEvents.MatchEnded)
                _matchEnded = true;
            if (_awaitedEventId != null && e.Matches(_awaitedEventId, _awaitedEventArg))
                _pendingEvents++;
        }

        private IEnumerator ExecuteCommand(TutorialCommand cmd)
        {
            if (cmd == null)
                yield break;
            Log($"Commande {cmd}");
            switch (cmd.type)
            {
                case TutorialCommandType.Wait:
                    for (float t = 0; t < cmd.seconds && !_skipRequested; t += Time.unscaledDeltaTime)
                        yield return null;
                    yield break;
                case TutorialCommandType.HideElement:
                    TutorialTargetRegistry.SetHidden(cmd.target, true);
                    yield break;
                case TutorialCommandType.ShowElement:
                    TutorialTargetRegistry.SetHidden(cmd.target, false);
                    yield break;
                case TutorialCommandType.PublishEvent:
                    TutorialEventBus.Publish(cmd.arg, cmd.card, cmd.amount);
                    yield break;
                case TutorialCommandType.PauseOpponent:
                    TutorialSession.OpponentPaused = true;
                    break;
                case TutorialCommandType.ResumeOpponent:
                    TutorialSession.OpponentPaused = false;
                    break;
            }
            // Les actions de plateau ne sont pas filtrées par le TutorialInputGate.
            bool wasRestricted = TutorialInputGate.IsRestricted;
            var allowed = new List<AllowedAction>(TutorialInputGate.Allowed);
            TutorialInputGate.Release();
            yield return _bridge.ExecuteCommand(cmd);
            if (wasRestricted)
                TutorialInputGate.Restrict(allowed);
        }

        private void FinishLesson(bool completed, bool skipped)
        {
            var lesson = _lesson;
            TutorialEventBus.OnEvent -= OnGameEvent;
            if (ui != null) ui.HideAll();
            TutorialInputGate.Release();
            TutorialTargetRegistry.RevealAll();
            if (_previousRandom != null)
                GameRandom.Current = _previousRandom;
            _previousRandom = null;
            TutorialSession.End();

            if (lesson != null && completed)
                EnsureProgress().MarkCompleted(lesson.lessonId);

            _lesson = null;
            _step = null;
            _awaitedEventId = null;
            _lessonRoutine = null;

            if (_bridge != null && !(_bridge is UnityEngine.Object o && o == null))
                _bridge.EndScriptedMatch(completed);
            Log($"Leçon \"{lesson?.lessonId}\" : fin (terminée={completed}, passée={skipped}).");
        }

        private static ITutorialGameBridge FindBridgeInScene()
        {
#if UNITY_2023_1_OR_NEWER
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
#else
            var behaviours = FindObjectsOfType<MonoBehaviour>();
#endif
            foreach (var b in behaviours)
                if (b is ITutorialGameBridge bridge)
                    return bridge;
            return null;
        }

        private void Log(string message)
        {
            if (verboseLogs)
                Debug.Log($"[Tutoriel] {message}", this);
        }
    }
}
