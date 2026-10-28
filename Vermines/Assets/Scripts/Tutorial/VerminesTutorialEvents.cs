namespace Vermines.Tutorial.Integration {

    using CelticEdge.Tutorial;

    /// <summary>
    /// IDs of the events Vermines publishes on the TutorialEventBus (TutorialGameEventRelay, game hooks),
    /// the vocabulary of the "evenement" column of the scenario table.
    /// turn_started and turn_ended come from the package (TutorialEvents).
    /// The same actions of a scripted opponent are published with the "opponent_" prefix.
    /// </summary>
    public static class VerminesTutorialEvents {

        #region Cards (Arg = card ID)

        public const string CardDrawn          = "card_drawn";
        public const string CardPlayed         = "card_played";
        public const string CardBought         = "card_bought";
        public const string CardRecycled       = "card_recycled";
        public const string CardDiscarded      = "card_discarded";
        public const string PartisanPlaced     = "partisan_placed";
        public const string PartisanSacrificed = "partisan_sacrificed";

        /// <summary>Arg = card ID, Value = slot index.</summary>
        public const string CardClicked = "card_clicked";

        public const string DeckReshuffled = "deck_reshuffled";

        #endregion

        #region Resources (Value = new total)

        public const string EloquenceChanged = "eloquence_changed";
        public const string SoulsChanged     = "souls_changed";

        /// <summary>Arg = threshold reached ("25"), published for every multiple of 5 the player's souls go past.</summary>
        public const string SoulsReached = "souls_reached";

        #endregion

        #region Match

        public const string MatchStarted = "match_started";

        /// <summary>Arg = "win" or "lose", Value = index of the winner.</summary>
        public const string MatchEnded = "match_ended";

        /// <summary>Arg = phase name ("Gain", "Action"...), Value = index of the current player.</summary>
        public const string PhaseChanged = "phase_changed";

        /// <summary>Arg = kind of the effect prompt.</summary>
        public const string EffectPrompt = "effect_prompt";

        /// <summary>Arg = key of the game's error (Shop_Buy_NotEnoughEloquence...), or of a refusal the tutorial reports itself.</summary>
        public const string ActionRefused = "action_refused";

        #endregion

        #region Interface

        /// <summary>Arg = main, courtyard, market or sacrifice: the view the camera just reached.</summary>
        public const string CameraLocation = "camera_location";

        /// <summary>Arg = courtyard or market.</summary>
        public const string ShopOpened = "shop_opened";
        public const string ShopClosed = "shop_closed";

        #endregion

        public const string OpponentPrefix = "opponent_";

        #region Methods

        /// <summary>
        /// Events that happen without an action of the player: a step waiting for them can forbid every action.
        /// </summary>
        public static void RegisterPassiveEvents()
        {
            TutorialEvents.RegisterPassiveEvent(MatchStarted);
            TutorialEvents.RegisterPassiveEvent(MatchEnded);
            TutorialEvents.RegisterPassiveEvent(DeckReshuffled);
            TutorialEvents.RegisterPassiveEvent(EffectPrompt);
            TutorialEvents.RegisterPassiveEvent(CameraLocation);
        }

        #endregion
    }
}
