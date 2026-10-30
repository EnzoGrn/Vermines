using NUnit.Framework;

namespace CelticEdge.Tutorial.Tests {

    public class ProgressTests {

        [Test]
        public void CompletionIsSavedPerProfile()
        {
            var store = new InMemoryTutorialProgressStore();
            var alice = new TutorialProgressService(store, "alice");

            alice.MarkStarted("lecon_01");

            Assert.AreEqual("lecon_01", alice.Data.InProgressLessonId);

            alice.MarkCompleted("lecon_01");

            var aliceAgain = new TutorialProgressService(store, "alice");

            Assert.IsTrue(aliceAgain.IsCompleted("lecon_01"));
            Assert.IsNull(aliceAgain.Data.InProgressLessonId);

            var bob = new TutorialProgressService(store, "bob");

            Assert.IsFalse(bob.IsCompleted("lecon_01"));
        }

        [Test]
        public void CompletingTwiceDoesNotDuplicate()
        {
            var p = new TutorialProgressService(new InMemoryTutorialProgressStore(), "p");

            p.MarkCompleted("x");
            p.MarkCompleted("x");

            Assert.AreEqual(1, p.Data.CompletedLessonIds.Count);
        }

        [Test]
        public void ResetClearsEverything()
        {
            var store = new InMemoryTutorialProgressStore();
            var p     = new TutorialProgressService(store, "p");

            p.MarkCompleted("x");
            p.SkipTutorial();
            p.ResetAll();

            Assert.IsFalse(p.IsCompleted("x"));
            Assert.IsFalse(p.IsSkipped);
            Assert.IsNull(store.Load("p"));
        }
    }
}
