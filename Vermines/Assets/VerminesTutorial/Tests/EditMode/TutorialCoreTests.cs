using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vermines.Tutorial.Authoring;

namespace Vermines.Tutorial.Tests
{
    public class ScriptedShuffleTests
    {
        private class Card
        {
            public string Id;
            public Card(string id) { Id = id; }
        }

        private static List<Card> Cards(params string[] ids) => ids.Select(i => new Card(i)).ToList();
        private static string[] Ids(IEnumerable<Card> cards) => cards.Select(c => c.Id).ToArray();

        [Test]
        public void ScriptedOrderIsAppliedTopFirst()
        {
            var service = new ScriptedShuffleService(new SeededShuffleService(1));
            service.EnqueueOrder(PileKey.Deck(0), new[] { "dague", "ouvriere", "missive" });
            var deck = Cards("ouvriere", "missive", "dague");

            service.Shuffle(deck, PileKey.Deck(0), c => c.Id);

            Assert.AreEqual(new[] { "dague", "ouvriere", "missive" }, Ids(deck));
        }

        [Test]
        public void TopAtEndConventionReversesOrder()
        {
            var service = new ScriptedShuffleService(new SeededShuffleService(1), topIsFirstIndex: false);
            service.EnqueueOrder(PileKey.Deck(0), new[] { "a", "b", "c" });
            var deck = Cards("c", "a", "b");

            service.Shuffle(deck, PileKey.Deck(0), c => c.Id);

            Assert.AreEqual(new[] { "c", "b", "a" }, Ids(deck));
        }

        [Test]
        public void DuplicatesAreMatchedOneByOne()
        {
            var service = new ScriptedShuffleService(new SeededShuffleService(1));
            service.EnqueueOrder(PileKey.Deck(0), new[] { "ouvriere", "dague", "ouvriere" });
            var deck = Cards("dague", "ouvriere", "ouvriere");

            service.Shuffle(deck, PileKey.Deck(0), c => c.Id);

            Assert.AreEqual(new[] { "ouvriere", "dague", "ouvriere" }, Ids(deck));
        }

        [Test]
        public void UnlistedCardsGoUnderneathAndMissingCardsWarn()
        {
            var service = new ScriptedShuffleService(new SeededShuffleService(1));
            var warnings = new List<string>();
            service.OnWarning += warnings.Add;
            service.EnqueueOrder(PileKey.Deck(0), new[] { "reine", "dague" });
            var deck = Cards("ouvriere", "dague", "missive");

            service.Shuffle(deck, PileKey.Deck(0), c => c.Id);

            Assert.AreEqual("dague", deck[0].Id);
            CollectionAssert.AreEquivalent(new[] { "dague", "ouvriere", "missive" }, Ids(deck));
            Assert.AreEqual(2, warnings.Count); // reine absente + cartes non prévues
        }

        [Test]
        public void SuccessiveShufflesUseSuccessiveOrdersThenFallback()
        {
            var setup = new ScriptedMatchSetup();
            setup.players.Add(new ScriptedPlayerSetup
            {
                deckTopToBottom = new List<string> { "a", "b" },
                reshuffleOrders = new List<CardOrder> { new CardOrder("b", "a") }
            });
            var service = ScriptedShuffleService.FromSetup(setup);

            var deck = Cards("b", "a");
            service.Shuffle(deck, PileKey.Deck(0), c => c.Id);
            Assert.AreEqual(new[] { "a", "b" }, Ids(deck));

            service.Shuffle(deck, PileKey.Deck(0), c => c.Id);
            Assert.AreEqual(new[] { "b", "a" }, Ids(deck));

            Assert.AreEqual(0, service.PendingOrders(PileKey.Deck(0)));
            service.Shuffle(deck, PileKey.Deck(0), c => c.Id); // repli aléatoire, ne doit pas planter
            Assert.AreEqual(2, deck.Count);
        }

        [Test]
        public void PilesAreIndependent()
        {
            var setup = new ScriptedMatchSetup { courtPileI = new List<string> { "villageois", "pretre" } };
            setup.players.Add(new ScriptedPlayerSetup { deckTopToBottom = new List<string> { "dague", "ouvriere" } });
            var service = ScriptedShuffleService.FromSetup(setup);

            var court = Cards("pretre", "villageois");
            service.Shuffle(court, PileKey.CourtI, c => c.Id);
            Assert.AreEqual(new[] { "villageois", "pretre" }, Ids(court));
            Assert.AreEqual(1, service.PendingOrders(PileKey.Deck(0)));
        }

        [Test]
        public void SeededShuffleIsReproducible()
        {
            var a = Cards("1", "2", "3", "4", "5", "6", "7", "8");
            var b = Cards("1", "2", "3", "4", "5", "6", "7", "8");
            new SeededShuffleService(42).Shuffle(a, PileKey.Deck(0), c => c.Id);
            new SeededShuffleService(42).Shuffle(b, PileKey.Deck(0), c => c.Id);
            Assert.AreEqual(Ids(a), Ids(b));
        }

        [Test]
        public void ScriptedRollsAreUsedThenFallback()
        {
            var service = new ScriptedShuffleService(new SeededShuffleService(1));
            service.EnqueueRoll("first_player", 1);
            Assert.AreEqual(1, service.Range(0, 2, "first_player"));
            int next = service.Range(0, 2, "first_player");
            Assert.IsTrue(next == 0 || next == 1);
        }
    }

    public class ProgressTests
    {
        [Test]
        public void CompletionIsSavedPerProfile()
        {
            var store = new InMemoryTutorialProgressStore();
            var alice = new TutorialProgressService(store, "alice");
            alice.MarkStarted("lecon_01");
            Assert.AreEqual("lecon_01", alice.Data.inProgressLessonId);
            alice.MarkCompleted("lecon_01");

            var aliceAgain = new TutorialProgressService(store, "alice");
            Assert.IsTrue(aliceAgain.IsCompleted("lecon_01"));
            Assert.IsNull(aliceAgain.Data.inProgressLessonId);

            var bob = new TutorialProgressService(store, "bob");
            Assert.IsFalse(bob.IsCompleted("lecon_01"));
        }

        [Test]
        public void CompletingTwiceDoesNotDuplicate()
        {
            var p = new TutorialProgressService(new InMemoryTutorialProgressStore(), "p");
            p.MarkCompleted("x");
            p.MarkCompleted("x");
            Assert.AreEqual(1, p.Data.completedLessonIds.Count);
        }

        [Test]
        public void ResetClearsEverything()
        {
            var store = new InMemoryTutorialProgressStore();
            var p = new TutorialProgressService(store, "p");
            p.MarkCompleted("x");
            p.SkipTutorial();
            p.ResetAll();
            Assert.IsFalse(p.IsCompleted("x"));
            Assert.IsFalse(p.IsSkipped);
            Assert.IsNull(store.Load("p"));
        }
    }

    public class InputGateTests
    {
        [TearDown]
        public void TearDown() => TutorialInputGate.Release();

        [Test]
        public void EverythingAllowedWhenNotRestricted()
        {
            Assert.IsTrue(TutorialInputGate.IsAllowed(GameActionType.BuyCard, "reine"));
        }

        [Test]
        public void OnlyListedActionsAreAllowed()
        {
            int refused = 0;
            System.Action<GameActionType, string> handler = (a, c) => refused++;
            TutorialInputGate.OnActionRefused += handler;
            try
            {
                TutorialInputGate.Restrict(new[] { new AllowedAction(GameActionType.PlayCard, "ouvriere"), new AllowedAction(GameActionType.EndTurn) });
                Assert.IsTrue(TutorialInputGate.IsAllowed(GameActionType.PlayCard, "Ouvriere"));
                Assert.IsFalse(TutorialInputGate.IsAllowed(GameActionType.PlayCard, "dague"));
                Assert.IsTrue(TutorialInputGate.IsAllowed(GameActionType.EndTurn));
                Assert.IsFalse(TutorialInputGate.WouldAllow(GameActionType.BuyCard, "dague"));
                Assert.AreEqual(1, refused);
            }
            finally
            {
                TutorialInputGate.OnActionRefused -= handler;
            }
        }

        [Test]
        public void EmptyRestrictionBlocksEverything()
        {
            TutorialInputGate.Restrict(null);
            Assert.IsFalse(TutorialInputGate.IsAllowed(GameActionType.EndTurn));
        }
    }

    public class OpponentScriptTests
    {
        [Test]
        public void UnscriptedTurnEndsTurnUnlessAIAllowed()
        {
            var script = new OpponentScript { playerIndex = 1 };
            script.turns.Add(new ScriptedTurn { turnNumber = 1, actions = { new ScriptedGameAction(GameActionType.BuyCard, "dague") } });
            TutorialSession.Begin("test", new ScriptedMatchSetup(), script);
            try
            {
                Assert.IsTrue(TutorialSession.TryGetOpponentTurn(1, 1, out var t1));
                Assert.AreEqual(GameActionType.BuyCard, t1[0].action);

                Assert.IsTrue(TutorialSession.TryGetOpponentTurn(1, 2, out var t2));
                Assert.AreEqual(GameActionType.EndTurn, t2.Single().action);

                Assert.IsFalse(TutorialSession.TryGetOpponentTurn(0, 1, out _), "le joueur humain n'est jamais scripté");

                script.useAIForUnscriptedTurns = true;
                Assert.IsFalse(TutorialSession.TryGetOpponentTurn(1, 2, out _));
            }
            finally
            {
                TutorialSession.End();
            }
        }
    }

    public class TableTests
    {
        private const string Sample =
            "lecon;id_etape;style;titre;texte;cibles;condition;evenement;argument;actions_autorisees;commandes_debut\r\n" +
            "lecon_01;intro;narrateur;Bienvenue;\"Bonjour, cultiste ; prêt ?\";;bouton;;;aucune;HideElement(target=market) | HideElement(target=court)\r\n" +
            ";jouer_ouvriere;bulle;;Jouez l'Ouvrière.;hand:ouvriere|partisan_zone;;card_played;ouvriere;jouer:ouvriere;\r\n" +
            ";fin;Bulle;;Terminez votre tour.;end_turn_button;clic_cible;;;fin_tour;AddEloquence(amount=2, player=0)\r\n";

        [Test]
        public void ParsesFrenchTable()
        {
            var warnings = new List<string>();
            var steps = TutorialTable.Parse(Sample, warnings);

            CollectionAssert.IsEmpty(warnings, string.Join("\n", warnings));
            Assert.AreEqual(3, steps.Count);

            var intro = steps[0];
            Assert.AreEqual("lecon_01", intro.lessonId);
            Assert.AreEqual(PopupStyle.Narrator, intro.popupStyle);
            Assert.AreEqual("Bonjour, cultiste ; prêt ?", intro.body);
            Assert.AreEqual(AdvanceCondition.WaitForButton, intro.advanceCondition);
            Assert.IsTrue(intro.restrictGameActions);
            Assert.AreEqual(0, intro.allowedActions.Count);
            Assert.AreEqual(2, intro.onEnter.Count);
            Assert.AreEqual(TutorialCommandType.HideElement, intro.onEnter[1].type);
            Assert.AreEqual("court", intro.onEnter[1].target);

            var play = steps[1];
            Assert.AreEqual("lecon_01", play.lessonId, "la leçon se propage aux lignes suivantes");
            Assert.AreEqual(new[] { "hand:ouvriere", "partisan_zone" }, play.targetIds.ToArray());
            Assert.AreEqual(AdvanceCondition.WaitForEvent, play.advanceCondition, "un événement sans condition implique WaitForEvent");
            Assert.AreEqual("ouvriere", play.eventArg);
            Assert.AreEqual(GameActionType.PlayCard, play.allowedActions.Single().action);
            Assert.AreEqual("ouvriere", play.allowedActions.Single().cardId);

            var end = steps[2];
            Assert.AreEqual(AdvanceCondition.WaitForTargetClick, end.advanceCondition);
            Assert.AreEqual(GameActionType.EndTurn, end.allowedActions.Single().action);
            Assert.AreEqual(2, end.onEnter.Single().amount);
        }

        [Test]
        public void UnknownValuesProduceWarnings()
        {
            var warnings = new List<string>();
            TutorialTable.Parse("id_etape;style;actions_autorisees;commandes_debut\nx;bizarre;voler;Teleport()\n", warnings);
            Assert.AreEqual(3, warnings.Count, string.Join("\n", warnings));
        }

        [Test]
        public void AllActionsKeyword()
        {
            TutorialTable.ParseActions("toutes", out bool restrict);
            Assert.IsFalse(restrict);
        }

        [Test]
        public void RoundTripThroughCsv()
        {
            var original = TutorialTable.Parse(Sample, new List<string>());
            string csv = TutorialTable.ToCsv(original);
            var again = TutorialTable.Parse(csv, new List<string>());

            Assert.AreEqual(original.Count, again.Count);
            for (int i = 0; i < original.Count; i++)
            {
                Assert.AreEqual(original[i].stepId, again[i].stepId);
                Assert.AreEqual(original[i].body, again[i].body);
                Assert.AreEqual(original[i].advanceCondition, again[i].advanceCondition);
                Assert.AreEqual(original[i].targetIds, again[i].targetIds);
                Assert.AreEqual(original[i].onEnter.Count, again[i].onEnter.Count);
                Assert.AreEqual(original[i].allowedActions.Count, again[i].allowedActions.Count);
            }
        }

        [Test]
        public void CommaSeparatedFileIsDetected()
        {
            var steps = TutorialTable.Parse("id_etape,texte\na,\"Bonjour, toi\"\n", new List<string>());
            Assert.AreEqual("Bonjour, toi", steps.Single().body);
        }
    }
}
