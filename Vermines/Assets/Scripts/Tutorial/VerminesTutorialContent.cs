using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Tutorial.Integration {

    using Vermines.Tutorial.Authoring;

    /// <summary>
    /// The lessons of the tutorial.
    /// - If an asset Resources/Tutorial/VerminesCampaign (TutorialCampaignData) exists, it is used as is.
    /// - Otherwise the campaign is built here: the steps come from the table
    ///   Resources/Tutorial/vermines_scenario.csv (same columns as the scenario table, editable in Excel / Google Sheets), the scripted setups (decks, court, opponent's turns) are below.
    /// Card IDs are the card names in lowercase with '_' ("Pugionem Sacrificialem" = pugionem_sacrificialem).
    /// </summary>
    public static class VerminesTutorialContent {

        public const string CampaignResource = "Tutorial/VerminesCampaign";
        public const string ScenarioResource = "Tutorial/vermines_scenario";

        public static TutorialCampaignData Build()
        {
            TutorialCampaignData asset = Resources.Load<TutorialCampaignData>(CampaignResource);

            if (asset != null)
                return asset;
            TutorialCampaignData campaign = ScriptableObject.CreateInstance<TutorialCampaignData>();

            campaign.name                   = "VerminesCampaign";
            campaign.campaignId             = "vermines";
            campaign.autoStartOnFirstLaunch = false;

            TextAsset    scenario = Resources.Load<TextAsset>(ScenarioResource);
            List<string> warnings = new();
            List<ParsedStep> rows = scenario != null ? TutorialTable.Parse(scenario.text, warnings) : new List<ParsedStep>();

            if (scenario == null)
                Debug.LogError($"[Tutorial] Table not found : Resources/{ScenarioResource}.csv");
            foreach (string warning in warnings)
                Debug.LogWarning("[Tutorial] Table : " + warning);
            campaign.lessons.Add(Lesson("lecon_01", "Découverte", "L'éloquence, acheter à la cour, défausser un objet, poser un partisan.", LessonOneSetup(), QuickOpponent(), rows, null));
            campaign.lessons.Add(Lesson("lecon_02", "Le sacrifice", "Gagner des âmes en sacrifiant des partisans, le bonus de famille.", LessonTwoSetup(), QuickOpponent(), rows, "lecon_01"));
            campaign.lessons.Add(Lesson("lecon_03", "Les boutiques", "Le marché, le recyclage et la cour.", LessonThreeSetup(), QuickOpponent(), rows, "lecon_02"));

            return campaign;
        }

        private static TutorialLessonData Lesson(string id, string displayName, string description, ScriptedMatchSetup setup, OpponentScript opponent, List<ParsedStep> rows, string requiredLessonId)
        {
            TutorialLessonData lesson = ScriptableObject.CreateInstance<TutorialLessonData>();

            lesson.name        = id;
            lesson.lessonId    = id;
            lesson.displayName = new TutorialText(displayName);
            lesson.description = new TutorialText(description);
            lesson.matchSetup  = setup;
            lesson.opponent    = opponent;

            if (!string.IsNullOrEmpty(requiredLessonId))
                lesson.requiredLessonIds.Add(requiredLessonId);
            foreach (ParsedStep row in rows) {
                if (row.lessonId != id)
                    continue;
                TutorialStepData step = ScriptableObject.CreateInstance<TutorialStepData>();

                step.name = row.stepId;

                row.ApplyTo(step);

                foreach (string problem in step.Validate())
                    Debug.LogWarning($"[Tutorial] Step {row.stepId} (line {row.sourceLine}) : {problem}");
                lesson.steps.Add(step);
            }

            if (lesson.steps.Count == 0)
                Debug.LogWarning($"[Tutorial] No steps for lesson {id} in the table.");
            return lesson;
        }

        /// <summary>
        /// The opponent only ends its turns, without the pauses made to follow its actions.
        /// </summary>
        private static OpponentScript QuickOpponent()
        {
            OpponentScript script = new() {
                playerIndex             = 1,
                useAIForUnscriptedTurns = false
            };

            return script;
        }

        #region Lesson 1

        /// <summary>
        /// Eloquence 3 + 2 at the start of the turn = 5, the price of the priest (Antistes) revealed in the court.
        /// The hand is only Scriptum Insanum and the deck is empty: discarding it gives 2 eloquence and draws the priest,
        /// since the empty deck is rebuilt from the discard where the purchase went.
        /// </summary>
        private static ScriptedMatchSetup LessonOneSetup()
        {
            ScriptedMatchSetup setup = new() {
                firstPlayerIndex = 0,
                fallbackSeed     = 7
            };

            setup.players.Add(new ScriptedPlayerSetup {
                isHuman           = true,
                displayName       = "Vous",
                startingEloquence = 3,
                exactDeck         = true,
                startingHand      = {
                    "scriptum_insanum"
                }
            });

            setup.players.Add(new ScriptedPlayerSetup {
                isHuman     = false,
                displayName = "Cultiste rival"
            });

            setup.courtPileI.Add("antistes");

            return setup;
        }

        #endregion

        #region Lesson 2

        /// <summary>
        /// Starts in the Action phase with the table open (Custom skip_to_action and open_table in the table).
        /// The priest on the table belongs to the opponent's family, so it gives its souls without the family bonus;
        /// the villager (Vicanus) in hand belongs to the player's family and shows the bonus.
        /// </summary>
        private static ScriptedMatchSetup LessonTwoSetup()
        {
            ScriptedMatchSetup setup = new() {
                firstPlayerIndex = 0,
                fallbackSeed     = 11
            };

            setup.players.Add(new ScriptedPlayerSetup {
                isHuman           = true,
                displayName       = "Vous",
                exactDeck         = true,
                startingPartisans = {
                    "antistes@opponent"
                },
                startingHand      = {
                    "pugionem_sacrificialem",
                    "pugionem_sacrificialem",
                    "vicanus"
                }
            });

            setup.players.Add(new ScriptedPlayerSetup {
                isHuman     = false,
                displayName = "Cultiste rival"
            });

            return setup;
        }

        #endregion

        #region Lesson 3

        /// <summary>
        /// Eloquence 5 + 2 = 7, the price of the last Idolum Maledictum of the market (a pile of 1 copy).
        /// The idol in hand is then recycled (+6) and goes back to the market.
        /// The other cards of the hand are the ones that add cards to the court.
        /// </summary>
        private static ScriptedMatchSetup LessonThreeSetup()
        {
            ScriptedMatchSetup setup = new() {
                firstPlayerIndex = 0,
                fallbackSeed     = 13
            };

            setup.players.Add(new ScriptedPlayerSetup {
                isHuman           = true,
                displayName       = "Vous",
                startingEloquence = 5,
                exactDeck         = true,
                startingHand      = {
                    "idolum_maledictum",
                    "epistula_regia",
                    "armiger",
                    "militia"
                }
            });

            setup.players.Add(new ScriptedPlayerSetup {
                isHuman     = false,
                displayName = "Cultiste rival"
            });

            setup.marketPiles.AddRange(new[] {
                "idolum_maledictum*1",
                "*"
            });

            // A single card in the court: the end of the lesson refills it by discarding the Epistula Regia or the Armiger.
            setup.courtOverride.Add("latro");

            return setup;
        }

        #endregion
    }
}
