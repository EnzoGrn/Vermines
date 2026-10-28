namespace Vermines.Tutorial.Integration {

    using CelticEdge.Tutorial.Authoring;

    /// <summary>
    /// Commands of the scenario table run by the game (VerminesTutorialBridge.ExecuteCommand), written Name(key=value, ...):
    ///     AddEloquence(amount=5, player=0), GiveCardToHand(card=operarius), SetCameraFocus(target=court), Custom(arg=open_table)...
    /// The tutorial itself runs Wait, HideElement, ShowElement, PublishEvent, PlayInterlude, PauseAgents and ResumeAgents.
    /// </summary>
    public static class VerminesTutorialCommands {

        #region Constants

        public const string SetEloquence        = "SetEloquence";
        public const string AddEloquence        = "AddEloquence";
        public const string AddSouls            = "AddSouls";
        public const string DrawCards           = "DrawCards";
        public const string GiveCardToHand      = "GiveCardToHand";
        public const string PutCardOnTopOfDeck  = "PutCardOnTopOfDeck";
        public const string AddCardToCourt      = "AddCardToCourt";
        public const string RemoveCardFromCourt = "RemoveCardFromCourt";
        public const string EndCurrentTurn      = "EndCurrentTurn";
        public const string SetCameraFocus      = "SetCameraFocus";

        /// <summary>
        /// Custom(arg=...): skip_to_action, open_table, open_courtyard, open_market.
        /// </summary>
        public const string Custom = "Custom";

        #endregion

        #region Argument keys

        public static readonly string[] PlayerKeys = { "player", "joueur" };
        public static readonly string[] CardKeys   = { "card", "carte" };
        public static readonly string[] TargetKeys = { "target", "cible" };
        public static readonly string[] AmountKeys = { "amount", "nombre", "quantite" };
        public static readonly string[] ArgKeys    = { "arg", "texte", "event", "evenement" };

        #endregion

        #region Methods

        /// <summary>
        /// Declares the commands to the table, so that a typo is reported when the table is read.
        /// </summary>
        public static void RegisterInTable()
        {
            TutorialTable.RegisterCommands(SetEloquence, AddEloquence, AddSouls, DrawCards, GiveCardToHand, PutCardOnTopOfDeck, AddCardToCourt, RemoveCardFromCourt, EndCurrentTurn, SetCameraFocus, Custom);
        }

        #endregion
    }
}
