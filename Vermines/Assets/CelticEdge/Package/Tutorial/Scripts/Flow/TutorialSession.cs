using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// What is true during a lesson.
    /// The game checks this to determine whether it is running a lesson (e.g., to disable chat, achievements, and leaderboards),
    /// read the game-specific setup, and run the scripted agents:
    ///
    /// <code>
    ///   if (TutorialSession.TryGetScriptedTurn(p, turn, out var actions))
    ///       StartCoroutine(TutorialSession.PlayScriptedTurn(bridge, p, actions));
    ///   else
    ///       ai.PlayTurn();
    /// </code>
    /// </summary>
    public static class TutorialSession {

        public static bool IsActive { get; private set; }

        public static TutorialLessonData Lesson { get; private set; }

        public static string LessonId => Lesson != null ? Lesson.LessonId : null;

        /// <summary>
        /// The lesson's scripted randomness: the game can add calculated commands (EnqueueOrder) in PrepareLesson.
        /// </summary>
        public static ScriptedGameRandom Random { get; private set; }

        /// <summary>True when a step has paused the agents (PauseAgents command).</summary>
        public static bool AgentsPaused { get; set; }

        /// <summary>The game-specific setup (TutorialLessonData.setup), or null.</summary>
        public static T GetSetup<T>() where T : class, ITutorialLessonSetup => Lesson != null ? Lesson.Setup as T : null;

        internal static void Begin(TutorialLessonData lesson, ScriptedGameRandom random)
        {
            IsActive     = true;
            Lesson       = lesson;
            Random       = random;
            AgentsPaused = false;
        }

        internal static void End()
        {
            IsActive     = false;
            Lesson       = null;
            Random       = null;
            AgentsPaused = false;
        }

        public static ScriptedAgent FindAgent(int agentIndex)
        {
            if (!IsActive || Lesson == null)
                return null;
            foreach (var agent in Lesson.ScriptedAgents) {
                if (agent != null && agent.AgentIndex == agentIndex)
                    return agent;
            }

            return null;
        }

        /// <summary>
        /// True if this agent's turn is scripted in the lesson (or should be reduced to unscriptedTurnAction).
        /// False: the game plays it normally.
        /// </summary>
        public static bool TryGetScriptedTurn(int agentIndex, int turnNumber, out IReadOnlyList<ScriptedAction> actions)
        {
            actions = null;

            var agent = FindAgent(agentIndex);

            if (agent == null)
                return false;
            if (agent.TryGetTurn(turnNumber, out actions))
                return true;
            if (agent.UseGameAIForUnscriptedTurns || string.IsNullOrEmpty(agent.UnscriptedTurnAction))
                return false;
            actions = new[] {
                new ScriptedAction(agent.UnscriptedTurnAction)
            };

            return true;
        }

        /// <summary>
        /// Plays a list of scripted actions, observing the pauses specified by the tutorial (AgentsPaused) and the delays between actions.
        /// </summary>
        public static IEnumerator PlayScriptedTurn(ITutorialGameBridge bridge, int agentIndex, IReadOnlyList<ScriptedAction> actions)
        {
            foreach (var action in actions) {
                if (action.DelayBefore > 0)
                    yield return new WaitForSeconds(action.DelayBefore);

                // Verified after the deadline: Any break requested during that time is honored.
                while (AgentsPaused)
                    yield return null;
                yield return bridge.PerformScriptedAction(agentIndex, action);
            }
        }
    }
}
