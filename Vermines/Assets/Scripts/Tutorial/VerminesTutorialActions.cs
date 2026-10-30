namespace Vermines.Tutorial.Integration {

    using CelticEdge.Tutorial.Authoring;

    /// <summary>
    /// IDs of the player's actions, checked by the game with TutorialInputGate.IsAllowed before it runs them.
    /// The subject of an action is the card ID (see TutorialCardIds), or the phase name for NextPhase.
    /// The scripted opponents use the same IDs (ScriptedAction.Action).
    /// </summary>
    public static class VerminesTutorialActions {

        #region Constants

        public const string PlayCard          = "play_card";
        public const string BuyCard           = "buy_card";
        public const string RecycleCard       = "recycle_card";
        public const string DiscardCard       = "discard_card";
        public const string ActivateEffect    = "activate_effect";
        public const string SacrificePartisan = "sacrifice_partisan";
        public const string EndTurn           = "end_turn";
        public const string Other             = "other";

        /// <summary>
        /// The phase button. Subject = name of the current phase ("Gain", "Action"...), empty = any phase.
        /// </summary>
        public const string NextPhase = "next_phase";

        /// <summary>
        /// One of the effects of a card that offers several (the dagger).
        /// Subject = "card#index" (index of the effect in the card, 0 = the first one), or just "card" for any of its effects.
        /// </summary>
        public const string ChooseEffect = "choose_effect";

        #endregion

        #region Methods

        /// <summary>
        /// The French names written in the "actions_autorisees" column of the scenario table.
        /// Called before the table is read.
        /// </summary>
        public static void RegisterTableAliases()
        {
            TutorialTable.RegisterActionAlias("jouer"         , PlayCard);
            TutorialTable.RegisterActionAlias("acheter"       , BuyCard);
            TutorialTable.RegisterActionAlias("recycler"      , RecycleCard);
            TutorialTable.RegisterActionAlias("defausser"     , DiscardCard);
            TutorialTable.RegisterActionAlias("activer"       , ActivateEffect);
            TutorialTable.RegisterActionAlias("sacrifier"     , SacrificePartisan);
            TutorialTable.RegisterActionAlias("fin_tour"      , EndTurn);
            TutorialTable.RegisterActionAlias("fin_de_tour"   , EndTurn);
            TutorialTable.RegisterActionAlias("autre"         , Other);
            TutorialTable.RegisterActionAlias("phase_suivante", NextPhase);
            TutorialTable.RegisterActionAlias("phase"         , NextPhase);
            TutorialTable.RegisterActionAlias("choisir"       , ChooseEffect);
            TutorialTable.RegisterActionAlias("choisir_effet" , ChooseEffect);
        }

        #endregion
    }
}
