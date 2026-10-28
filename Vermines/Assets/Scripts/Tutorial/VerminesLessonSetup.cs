using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vermines.Tutorial.Integration {

    using CelticEdge.Tutorial;

    /// <summary>
    /// The board of a tutorial lesson (TutorialLessonData.Setup), built by TutorialGameInitializer instead of a random match:
    /// players and their families, gods, decks and hands, court and market.
    /// Card IDs are the card names in lowercase with '_' (see TutorialCardIds).
    /// </summary>
    [Serializable]
    public class VerminesLessonSetup : ITutorialLessonSetup {

        #region Attributes

        [Tooltip("Index 0 = the human player, 1+ = the scripted opponents.")]
        public List<VerminesPlayerSetup> Players = new();

        public int FirstPlayerIndex;

        [Tooltip("Order of the court's level I pile, from top to bottom. The first 3 cards are revealed in the court.")]
        public List<string> CourtPileI = new();

        [Tooltip("Order of the court's level II pile. The first 2 cards are revealed in the court.")]
        public List<string> CourtPileII = new();

        [Tooltip("If set, the starting court is exactly this list (instead of 3 level I cards + 2 level II cards).")]
        public List<string> CourtOverride = new();

        [Tooltip("Market piles, in order. \"card*N\" = only N copies, \"*\" = every other tool. Empty = normal market.")]
        public List<string> MarketPiles = new();

        [Tooltip("Seed of the match: the lesson stays the same from one session to the next.")]
        public int FallbackSeed = 1;

        [Tooltip("Souls that trigger the end of the match (100 in the base game). 0 = normal rule.")]
        public int VictorySoulsOverride;

        #endregion
    }

    /// <summary>
    /// One player of a lesson (VerminesLessonSetup.Players).
    /// </summary>
    [Serializable]
    public class VerminesPlayerSetup {

        #region Attributes

        public string DisplayName;

        [Tooltip("Cultist ID, cultist name or family name. Empty = the family chosen by the player (human), or a free one.")]
        public string CultistId;

        [Tooltip("ID or name of the god. Empty = a god without power; \"aleatoire\" = drawn as in a normal match.")]
        public string GodId;

        public int StartingEloquence;
        public int StartingSouls;

        [Tooltip("Deck order, from top to bottom. Empty = normal starting deck, shuffled.")]
        public List<string> DeckTopToBottom = new();

        [Tooltip("True = the deck is exactly DeckTopToBottom, even empty (no normal starting deck).")]
        public bool ExactDeck;

        [Tooltip("If set, the starting hand is exactly these cards (no initial draw).")]
        public List<string> StartingHand = new();

        [Tooltip("Partisans already on the table when the lesson starts. \"antistes@opponent\" = the copy of the opponent's family.")]
        public List<string> StartingPartisans = new();

        [Tooltip("Cards already in the discard pile when the lesson starts.")]
        public List<string> StartingDiscard = new();

        [Tooltip("Partisans already sacrificed when the lesson starts (for the family bonus).")]
        public List<string> StartingSacrificed = new();

        [Tooltip("Opponent played by the AI: it never goes above this number of souls, so it does not win the lesson. 0 = no cap.")]
        public int AIMaxSouls;

        #endregion
    }
}
