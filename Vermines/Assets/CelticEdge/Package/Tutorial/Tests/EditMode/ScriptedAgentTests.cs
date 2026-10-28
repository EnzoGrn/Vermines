using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace CelticEdge.Tutorial.Tests {

    public class ScriptedAgentTests {

        [Test]
        public void UnscriptedTurnEndsTurnUnlessAIAllowed()
        {
            var lesson = ScriptableObject.CreateInstance<TutorialLessonData>();
            var agent  = new ScriptedAgent {
                AgentIndex = 1
            };

            agent.Turns.Add(new ScriptedTurn {
                TurnNumber = 1,
                Actions    = {
                    new ScriptedAction("buy", "dague")
                }
            });

            lesson.ScriptedAgents.Add(agent);

            TutorialSession.Begin(lesson, null);

            try {
                Assert.IsTrue(TutorialSession.TryGetScriptedTurn(1, 1, out var t1));
                Assert.AreEqual("buy", t1[0].Action);

                Assert.IsTrue(TutorialSession.TryGetScriptedTurn(1, 2, out var t2));
                Assert.AreEqual("end_turn", t2.Single().Action);

                Assert.IsFalse(TutorialSession.TryGetScriptedTurn(0, 1, out _), "the human player is not scripted");

                agent.UseGameAIForUnscriptedTurns = true;

                Assert.IsFalse(TutorialSession.TryGetScriptedTurn(1, 2, out _));
            } finally {
                TutorialSession.End();

                Object.DestroyImmediate(lesson);
            }
        }
    }
}
