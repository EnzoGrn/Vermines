using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CelticEdge.Tutorial.Tests {

    using CelticEdge.Tutorial.Authoring;

    public class TableTests {

        private const string Sample =
            "lecon;id_etape;style;titre;texte;cibles;condition;evenement;argument;actions_autorisees;commandes_debut\r\n" +
            "lecon_01;intro;narrateur;Bienvenue;\"Bonjour ; prêt ?\";;bouton;;;aucune;HideElement(target=market) | HideElement(target=shop)\r\n" +
            ";jouer;bulle;;Jouez l'épée.;hand:sword|board;;card_played;sword;play:sword;\r\n" +
            ";fin;Bulle;;Terminez votre tour.;end_turn_button;clic_cible;;;end_turn;AddGold(amount=2, player=0)\r\n" +
            "lecon_01/trop_cher;trop_cher_1;fenetre;Trop cher;Il vous manque de l'or.;;bouton;;;aucune;\r\n"
        ;

        [Test]
        public void ParsesFrenchTable()
        {
            var warnings = new List<string>();
            var steps    = TutorialTable.Parse(Sample, warnings);

            CollectionAssert.IsEmpty(warnings, string.Join("\n", warnings));

            Assert.AreEqual(4, steps.Count);

            var intro = steps[0];

            Assert.AreEqual("lecon_01"                    , intro.LessonId);
            Assert.AreEqual(PopupStyle.Narrator           , intro.PopupStyle);
            Assert.AreEqual("Bonjour ; prêt ?"            , intro.Body);
            Assert.AreEqual(AdvanceCondition.WaitForButton, intro.AdvanceCondition);

            Assert.IsTrue(intro.RestrictGameActions);

            Assert.AreEqual(0                           , intro.AllowedActions.Count);
            Assert.AreEqual(2                           , intro.OnEnter.Count);
            Assert.AreEqual(TutorialCommands.HideElement, intro.OnEnter[1].Name);
            Assert.AreEqual("shop"                      , intro.OnEnter[1].Get("target"));

            var play = steps[1];

            Assert.AreEqual("lecon_01", play.LessonId, "the lesson extends to the following lines");

            Assert.AreEqual(new[] {
                "hand:sword", "board"
            }, play.TargetIds.ToArray());

            Assert.AreEqual(AdvanceCondition.WaitForEvent, play.AdvanceCondition, "an unconditional event involves WaitForEvent");
            Assert.AreEqual("sword", play.EventArg);
            Assert.AreEqual("play" , play.AllowedActions.Single().Action);
            Assert.AreEqual("sword", play.AllowedActions.Single().Subject);

            var end = steps[2];

            Assert.AreEqual(AdvanceCondition.WaitForTargetClick, end.AdvanceCondition);
            Assert.AreEqual("end_turn"                         , end.AllowedActions.Single().Action);
            Assert.AreEqual(2                                  , end.OnEnter.Single().GetInt("amount"));
        }

        [Test]
        public void ParsesEnglishHeaders()
        {
            var warnings = new List<string>();
            var steps    = TutorialTable.Parse("lesson,step_id,text,allowed_actions,on_enter\nl1,a,Hello,all,Wait(seconds=1)\n", warnings);

            CollectionAssert.IsEmpty(warnings, string.Join("\n", warnings));

            Assert.AreEqual("l1"   , steps[0].LessonId);
            Assert.AreEqual("Hello", steps[0].Body);

            Assert.IsFalse(steps[0].RestrictGameActions);

            Assert.AreEqual(1f, steps[0].OnEnter.Single().GetFloat("seconds"));
        }

        [Test]
        public void ActionAliasesAreApplied()
        {
            TutorialTable.RegisterActionAlias("acheter", "buy");

            var actions = TutorialTable.ParseActions("Acheter:sword | jump", out bool restrict);

            Assert.IsTrue(restrict);

            Assert.AreEqual("buy"  , actions[0].Action);
            Assert.AreEqual("sword", actions[0].Subject);
            Assert.AreEqual("jump" , actions[1].Action);
        }

        [Test]
        public void UnknownValuesProduceWarnings()
        {
            var warnings = new List<string>();

            TutorialTable.Parse("id_etape;style;condition\nx;bizarre;jamais\n", warnings);

            Assert.AreEqual(2, warnings.Count, string.Join("\n", warnings));
        }

        [Test]
        public void AllActionsKeyword()
        {
            TutorialTable.ParseActions("all", out bool restrict);

            Assert.IsFalse(restrict);
        }

        [Test]
        public void RoundTripThroughCsv()
        {
            var original = TutorialTable.Parse(Sample, new List<string>());
            string   csv = TutorialTable.ToCsv(original);
            var    again = TutorialTable.Parse(csv, new List<string>());

            Assert.AreEqual(original.Count, again.Count);

            for (int i = 0; i < original.Count; i++) {
                Assert.AreEqual(original[i].LessonId            , again[i].LessonId);
                Assert.AreEqual(original[i].StepId              , again[i].StepId);
                Assert.AreEqual(original[i].Body                , again[i].Body);
                Assert.AreEqual(original[i].AdvanceCondition    , again[i].AdvanceCondition);
                Assert.AreEqual(original[i].TargetIds           , again[i].TargetIds);
                Assert.AreEqual(original[i].OnEnter.Count       , again[i].OnEnter.Count);
                Assert.AreEqual(original[i].AllowedActions.Count, again[i].AllowedActions.Count);
            }
        }

        [Test]
        public void CommaSeparatedFileIsDetected()
        {
            var steps = TutorialTable.Parse("id_etape,texte\na,\"Bonjour, toi\"\n", new List<string>());

            Assert.AreEqual("Bonjour, toi", steps.Single().Body);
        }

        [Test]
        public void LoaderSplitsStepsAndInterludes()
        {
            var rows     = TutorialTable.Parse(Sample, new List<string>());
            var campaign = TutorialTableLoader.CreateCampaign(rows);

            try {
                Assert.AreEqual(1, campaign.Lessons.Count);

                var lesson = campaign.Lessons[0];

                Assert.AreEqual(3            , lesson.Steps.Count);
                Assert.AreEqual("trop_cher_1", lesson.FindInterlude("trop_cher").Steps.Single().StepId);
            } finally {
                foreach (var lesson in campaign.Lessons) {
                    foreach (var step in lesson.Steps.Concat(lesson.Interludes.SelectMany(i => i.Steps)))
                        Object.DestroyImmediate(step);
                    Object.DestroyImmediate(lesson);
                }

                Object.DestroyImmediate(campaign);
            }
        }
    }
}
