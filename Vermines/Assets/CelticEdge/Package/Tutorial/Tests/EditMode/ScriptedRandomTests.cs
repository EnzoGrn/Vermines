using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CelticEdge.Tutorial.Tests {

    public class ScriptedRandomTests {

        private class Card {

            public string Id;

            public Card(string id)
            {
                Id = id;
            }
        }

        private static List<Card> Cards(params string[] ids) => ids.Select(i => new Card(i)).ToList();

        private static string[] Ids(IEnumerable<Card> cards) => cards.Select(c => c.Id).ToArray();

        [Test]
        public void ScriptedOrderIsAppliedTopFirst()
        {
            var service = new ScriptedGameRandom(new SeededGameRandom(1));

            service.EnqueueOrder("deck:0", new[] {
                "dagger", "worker", "missive"
            });

            var deck = Cards("worker", "missive", "dagger");

            service.Shuffle(deck, "deck:0", c => c.Id);

            Assert.AreEqual(new[] {
                "dagger", "worker", "missive"
            }, Ids(deck));
        }

        [Test]
        public void TopAtEndConventionReversesOrder()
        {
            var service = new ScriptedGameRandom(new SeededGameRandom(1), topIsFirstIndex: false);

            service.EnqueueOrder("deck:0", new[] {
                "a", "b", "c"
            });

            var deck = Cards("c", "a", "b");

            service.Shuffle(deck, "deck:0", c => c.Id);

            Assert.AreEqual(new[] {
                "c", "b", "a"
            }, Ids(deck));
        }

        [Test]
        public void DuplicatesAreMatchedOneByOne()
        {
            var service = new ScriptedGameRandom(new SeededGameRandom(1));

            service.EnqueueOrder("deck:0", new[] {
                "worker", "dagger", "worker"
            });

            var deck = Cards("dagger", "worker", "worker");

            service.Shuffle(deck, "deck:0", c => c.Id);

            Assert.AreEqual(new[] {
                "worker", "dagger", "worker"
            }, Ids(deck));
        }

        [Test]
        public void UnlistedItemsGoUnderneathAndMissingItemsWarn()
        {
            var service = new ScriptedGameRandom(new SeededGameRandom(1));
            var warnings = new List<string>();

            service.OnWarning += warnings.Add;

            service.EnqueueOrder("deck:0", new[] {
                "queen", "dagger"
            });

            var deck = Cards("worker", "dagger", "missive");

            service.Shuffle(deck, "deck:0", c => c.Id);

            Assert.AreEqual("dagger", deck[0].Id);

            CollectionAssert.AreEquivalent(new[] {
                "dagger", "worker", "missive"
            }, Ids(deck));

            Assert.AreEqual(2, warnings.Count); // queen missing + items not listed
        }

        [Test]
        public void SuccessiveShufflesUseSuccessiveOrdersThenFallback()
        {
            var setup = new ScriptedRandomSetup();

            setup.Orders.Add(new ScriptedOrder("deck:0", "a", "b"));
            setup.Orders.Add(new ScriptedOrder("deck:0", "b", "a"));

            var service = ScriptedGameRandom.FromSetup(setup);
            var deck = Cards("b", "a");

            service.Shuffle(deck, "deck:0", c => c.Id);

            Assert.AreEqual(new[] {
                "a", "b"
            }, Ids(deck));

            service.Shuffle(deck, "deck:0", c => c.Id);

            Assert.AreEqual(new[] {
                "b", "a"
            }, Ids(deck));

            Assert.AreEqual(0, service.PendingOrders("deck:0"));

            service.Shuffle(deck, "deck:0", c => c.Id); // Random shuffle, must not crash

            Assert.AreEqual(2, deck.Count);
        }

        [Test]
        public void PilesAreIndependent()
        {
            var setup = new ScriptedRandomSetup();

            setup.Orders.Add(new ScriptedOrder("court", "villagers", "priest"));
            setup.Orders.Add(new ScriptedOrder("deck:0", "dagger", "worker"));

            var service = ScriptedGameRandom.FromSetup(setup);
            var court = Cards("priest", "villagers");

            service.Shuffle(court, "court", c => c.Id);

            Assert.AreEqual(new[] {
                "villagers", "priest"
            }, Ids(court));
            Assert.AreEqual(1, service.PendingOrders("deck:0"));
        }

        [Test]
        public void SeededShuffleIsReproducible()
        {
            var a = Cards("1", "2", "3", "4", "5", "6", "7", "8");
            var b = Cards("1", "2", "3", "4", "5", "6", "7", "8");

            new SeededGameRandom(42).Shuffle(a, "deck:0", c => c.Id);
            new SeededGameRandom(42).Shuffle(b, "deck:0", c => c.Id);

            Assert.AreEqual(Ids(a), Ids(b));
        }

        [Test]
        public void ScriptedRollsAreUsedThenFallback()
        {
            var service = new ScriptedGameRandom(new SeededGameRandom(1));

            service.EnqueueRoll("first_player", 1);

            Assert.AreEqual(1, service.Range(0, 2, "first_player"));

            int next = service.Range(0, 2, "first_player");

            Assert.IsTrue(next == 0 || next == 1);
        }
    }
}
