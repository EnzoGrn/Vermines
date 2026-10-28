namespace Vermines.Gameplay.Core {

    using Vermines.Tutorial.Integration;

    public partial class StandartGameplay {

        /// <summary>
        /// A tutorial match has one connected player and scripted opponents without a client:
        ///     the turn order comes from the lesson, not from Runner.ActivePlayers.
        /// </summary>
        public override int TurnPlayerCount => TutorialMatch.IsActive && TutorialMatch.PlayerCount > 0 ? TutorialMatch.PlayerCount : base.TurnPlayerCount;

        private GameModeInitializer CreateInitializer()
        {
            return TutorialMatch.IsActive ? new TutorialGameInitializer(this) : new StandartGameInitializer(this);
        }
    }
}
