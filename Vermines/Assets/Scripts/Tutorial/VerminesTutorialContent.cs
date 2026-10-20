using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Tutorial.Integration {

    using Vermines.Characters;
    using Vermines.Core;
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
            campaign.lessons.Add(Lesson("lecon_04", "Premiers pas", "Une vraie partie : construire son deck et atteindre 25 âmes.", LessonFourSetup(), LessonFourOpponent(), rows, "lecon_03"));
            campaign.lessons.Add(Lesson("lecon_05", "La partie", "Une partie entière contre deux cultistes, avec ton dieu : à toi de gagner !", LessonFiveSetup(), LessonFiveOpponent(), rows, "lecon_04"));

            ShowGodPortrait(campaign.lessons[campaign.lessons.Count - 1], "l05_dieu", LessonFivePlayerGod);

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
            // Rows of "lecon_04" are the steps, rows of "lecon_04/trop_cher" the interlude "trop_cher" (played on the game's request).
            string interludePrefix = id + "/";

            foreach (ParsedStep row in rows) {
                if (row.lessonId != id && (row.lessonId == null || !row.lessonId.StartsWith(interludePrefix)))
                    continue;
                TutorialStepData step = ScriptableObject.CreateInstance<TutorialStepData>();

                step.name = row.stepId;

                row.ApplyTo(step);

                foreach (string problem in step.Validate())
                    Debug.LogWarning($"[Tutorial] Step {row.stepId} (line {row.sourceLine}) : {problem}");
                if (row.lessonId == id) {
                    lesson.steps.Add(step);

                    continue;
                }

                string             interludeId = row.lessonId.Substring(interludePrefix.Length);
                TutorialInterlude    interlude = lesson.FindInterlude(interludeId);

                if (interlude == null) {
                    interlude = new TutorialInterlude {
                        interludeId = interludeId
                    };

                    lesson.interludes.Add(interlude);
                }

                interlude.steps.Add(step);
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

        #region Lesson 4

        /// <summary>
        /// A normal start: starting deck (2 Operarius, 1 Vicanus, 2 Pugionem Sacrificialem), shuffled, 3 cards drawn after the first player's 2,
        /// no starting eloquence, a full court. The free-play step waits for 25 souls (souls_reached 25).
        /// No godId: both players get a god without power.
        /// </summary>
        private static ScriptedMatchSetup LessonFourSetup()
        {
            ScriptedMatchSetup setup = new() {
                firstPlayerIndex = 0,
                fallbackSeed     = 17
            };

            setup.players.Add(new ScriptedPlayerSetup {
                isHuman     = true,
                displayName = "Vous"
            });

            setup.players.Add(new ScriptedPlayerSetup {
                isHuman     = false,
                displayName = "Cultiste rival"
            });

            // On top of the court's first pile: the two cards the lesson advises (tips latro and armiger).
            setup.courtPileI.AddRange(new[] {
                "latro",
                "armiger"
            });

            return setup;
        }

        /// <summary>
        /// The opponent plays by itself (VerminesTutorialBridge.PlayAITurn), without ever going above 40 souls.
        /// </summary>
        private static OpponentScript LessonFourOpponent()
        {
            OpponentScript script = new() {
                playerIndex             = 1,
                useAIForUnscriptedTurns = true,
                aiMaxSouls              = 40
            };

            return script;
        }

        #endregion

        #region Lesson 5

        /// <summary>
        /// The player's god in lesson 5: Mantislith (ID 3), "your first sacrifice each turn draws 1 card". The step l05_dieu presents it.
        /// </summary>
        private const string LessonFivePlayerGod = "3";

        /// <summary>
        /// A whole match from the start, against two opponents who play by themselves without soul cap: the first to 100 souls ends the match.
        /// The player always gets the same god, the opponents a god drawn with the lesson seed.
        /// </summary>
        private static ScriptedMatchSetup LessonFiveSetup()
        {
            ScriptedMatchSetup setup = new() {
                firstPlayerIndex = 0,
                fallbackSeed     = 19
            };

            setup.players.Add(new ScriptedPlayerSetup {
                isHuman     = true,
                displayName = "Vous",
                godId       = LessonFivePlayerGod
            });

            setup.players.Add(new ScriptedPlayerSetup {
                isHuman     = false,
                displayName = "Cultiste rival",
                godId       = "aleatoire"
            });

            setup.players.Add(new ScriptedPlayerSetup {
                isHuman     = false,
                displayName = "Second cultiste",
                godId       = "aleatoire"
            });

            return setup;
        }

        /// <summary>
        /// Opponent 1 is the scripted one (AI for every turn), opponent 2 gets the AI as any unscripted player.
        /// </summary>
        private static OpponentScript LessonFiveOpponent()
        {
            OpponentScript script = new() {
                playerIndex             = 1,
                useAIForUnscriptedTurns = true
            };

            return script;
        }

        /// <summary>
        /// The god's image on the popup of the step that presents it (the game shows the gods nowhere else during a match).
        /// </summary>
        private static void ShowGodPortrait(TutorialLessonData lesson, string stepId, string godId)
        {
            TutorialStepData step = lesson.FindStep(stepId);
            God[]            gods = Global.Settings != null && Global.Settings.Gods != null ? Global.Settings.Gods.GetAllGods() : null;

            if (step == null || gods == null || !int.TryParse(godId, out int id))
                return;
            foreach (God god in gods) {
                if (god != null && god.ID == id)
                    step.portrait = god.DivinitySprite;
            }
        }

        #endregion

        #region Free play

        /// <summary>
        /// The hints and offers of a free-play lesson (TutorialFreePlayCoach), null for a guided lesson.
        /// The interludes named here are written in the table with the lesson "lecon_04/&lt;interlude&gt;".
        /// </summary>
        public static TutorialFreePlayRules FreePlayRules(string lessonId)
        {
            switch (lessonId) {
                case "lecon_04":
                    return LessonFourRules();
                case "lecon_05":
                    return LessonFiveRules();
                default:
                    return null;
            }
        }

        /// <summary>
        /// The hints of both free-play lessons (interludes of the same names in lecon_04/... and lecon_05/...).
        /// </summary>
        private static void AddHints(TutorialFreePlayRules rules)
        {
            rules.hints.Add(Hint("Shop_Buy_NotEnoughEloquence", "trop_cher"));
            rules.hints.Add(Hint("Table_Discard_CardNotDiscardable", "pas_defaussable"));
            rules.hints.Add(Hint("Table_Play_ToolOnTable", "objet_sur_table"));
            rules.hints.Add(Hint("Table_Play_TableFull", "table_pleine"));
            rules.hints.Add(Hint(TutorialFreePlayCoach.CourtIncompleteTrigger, "cour_vide", once: true));
            rules.hints.Add(Hint("Shop_SlotEmpty", "cour_vide", once: true));
            rules.hints.Add(Hint(TutorialFreePlayCoach.EloquenceMaxTrigger, "eloquence_max", once: true));
            rules.hints.Add(Hint(TutorialFreePlayCoach.TableFilledTrigger, "table_remplie", once: true));
            rules.hints.Add(Hint("Recycle_CardNotRecyclable", "dague_recyclage"));
            rules.hints.Add(Hint("Shop_Buy_WrongPhase", "mauvaise_phase"));
            rules.hints.Add(Hint("Table_Play_WrongPhase", "mauvaise_phase"));
            rules.hints.Add(Hint("Table_Discard_WrongPhase", "mauvaise_phase"));
        }

        private static TutorialFreePlayRules LessonFourRules()
        {
            TutorialFreePlayRules rules = new() {
                turnsBetweenOffers = 2
            };

            AddHints(rules);

            // Advice only, the first time the card is seen in the court.
            rules.tips.Add(new TutorialCardTip {
                cardId      = "latro",
                interludeId = "conseil_latro"
            });

            rules.tips.Add(new TutorialCardTip {
                cardId      = "armiger",
                interludeId = "conseil_armiger"
            });

            // Nuntius Regius (7 eloquence): 3 souls and a card at the start of each turn, the way to the 25 souls.
            rules.offers.Add(new TutorialCourtOffer {
                cardId            = "nuntius_regius",
                offerInterludeId  = "offre_nuntius_regius",
                placedInterludeId = "pose_nuntius_regius",
                effectInterludeId = "effet_nuntius_regius",
                effectMoment      = TutorialOfferEffect.NextTurnStart
            });

            // Cruce Signatus (10 eloquence): 5 souls for each card added to the court, with the Epistula Regia, the Armiger or the Militia.
            rules.offers.Add(new TutorialCourtOffer {
                cardId            = "cruce_signatus",
                offerInterludeId  = "offre_cruce_signatus",
                placedInterludeId = "pose_cruce_signatus",
                effectInterludeId = "effet_cruce_signatus",
                effectMoment      = TutorialOfferEffect.CourtCardAdded
            });

            return rules;
        }

        /// <summary>
        /// A whole match to win: the hints stay, no offer, a loss fails the lesson after the interlude "defaite".
        /// </summary>
        private static TutorialFreePlayRules LessonFiveRules()
        {
            TutorialFreePlayRules rules = new() {
                requireWin        = true,
                defeatInterludeId = "defaite"
            };

            AddHints(rules);

            rules.hints.Add(Hint(TutorialFreePlayCoach.EndGamePlayerTrigger, "fin_partie_joueur", once: true));
            rules.hints.Add(Hint(TutorialFreePlayCoach.EndGameRivalTrigger, "fin_partie_rival", once: true));

            return rules;
        }

        private static TutorialHint Hint(string trigger, string interludeId, int cooldownTurns = 1, bool once = false)
        {
            return new TutorialHint {
                trigger       = trigger,
                interludeId   = interludeId,
                cooldownTurns = cooldownTurns,
                once          = once
            };
        }

        #endregion
    }
}
