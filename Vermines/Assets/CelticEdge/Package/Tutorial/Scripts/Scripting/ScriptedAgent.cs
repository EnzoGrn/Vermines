using System.Collections.Generic;
using System;
using UnityEngine;

namespace CelticEdge.Tutorial {

    /// <summary>
    /// An action forced upon a scripted character (enemy, ally, NPC, etc.).
    /// </summary>
    [Serializable]
    public struct ScriptedAction {

        [Tooltip("Action ID, interpreted by ITutorialGameBridge.PerformScriptedAction.")]
        public string Action;

        [Tooltip("What the action is about (card played, target space, etc.).")]
        public string Subject;

        [Tooltip("A pause before the action, so the player has time to catch up.")]
        public float DelayBefore;

        public ScriptedAction(string action, string subject = null, float delayBefore = 0.6f)
        {
            Action      = action;
            Subject     = subject;
            DelayBefore = delayBefore;
        }

        public override string ToString() => string.IsNullOrEmpty(Subject) ? Action : $"{Action}:{Subject}";
    }

    [Serializable]
    public class ScriptedTurn {

        [Tooltip("This agent's turn number (1 = their first turn).")]
        public int TurnNumber = 1;

        public List<ScriptedAction> Actions = new();
    }

    /// <summary>
    /// An agent's turns, written in advance (turn-based games).
    /// A missing turn gives control to the game's AI, or is simply an UnscriptedTurnAction.
    /// </summary>
    [Serializable]
    public class ScriptedAgent {

        [Tooltip("Index of the player controlled by this agent (0 = usually the human player).")]
        public int AgentIndex = 1;

        public List<ScriptedTurn> Turns = new();

        [Tooltip("True = An unscripted turn is played by the game's standard AI.")]
        public bool UseGameAIForUnscriptedTurns;

        [Tooltip("Without AI: the action taken during an unscripted turn (e.g., \"end_turn\"). Blank = the game decides.")]
        public string UnscriptedTurnAction = "end_turn";

        public bool TryGetTurn(int turnNumber, out IReadOnlyList<ScriptedAction> actions)
        {
            foreach (var t in Turns) {
                if (t != null && t.TurnNumber == turnNumber) {
                    actions = t.Actions;

                    return true;
                }
            }

            actions = null;

            return false;
        }
    }
}
