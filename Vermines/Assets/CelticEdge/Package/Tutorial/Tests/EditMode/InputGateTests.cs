using NUnit.Framework;
using System;

namespace CelticEdge.Tutorial.Tests {

    public class InputGateTests {

        [TearDown]
        public void TearDown() => TutorialInputGate.Release();

        [Test]
        public void EverythingAllowedWhenNotRestricted()
        {
            Assert.IsTrue(TutorialInputGate.IsAllowed("buy", "sword"));
        }

        [Test]
        public void OnlyListedActionsAreAllowed()
        {
            int refused = 0;

            Action<string, string> handler = (a, c) => refused++;

            TutorialInputGate.OnActionRefused += handler;

            try {
                TutorialInputGate.Restrict(new[] {
                    new AllowedAction("play", "worker"),
                    new AllowedAction("end_turn")
                });

                Assert.IsTrue(TutorialInputGate.IsAllowed("Play", "worker"));
                Assert.IsFalse(TutorialInputGate.IsAllowed("play", "dagger"));
                Assert.IsTrue(TutorialInputGate.IsAllowed("end_turn"));
                Assert.IsFalse(TutorialInputGate.WouldAllow("buy", "dagger"));
                Assert.AreEqual(1, refused);
            } finally {
                TutorialInputGate.OnActionRefused -= handler;
            }
        }

        [Test]
        public void WildcardAllowsAnyAction()
        {
            TutorialInputGate.Restrict(new[] {
                new AllowedAction(AllowedAction.AnyAction, "sword")
            });

            Assert.IsTrue(TutorialInputGate.IsAllowed("buy", "sword"));
            Assert.IsFalse(TutorialInputGate.IsAllowed("buy", "shield"));
        }

        [Test]
        public void EmptyRestrictionBlocksEverything()
        {
            TutorialInputGate.Restrict(null);

            Assert.IsFalse(TutorialInputGate.IsAllowed("end_turn"));
        }
    }
}
