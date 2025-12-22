namespace OMGG.DesignPattern {

    /// <summary>
    /// Represent the status of a command after its execution.
    /// </summary>
    public enum CommandStatus {

        /// <summary>
        /// Command run successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The command failed due to a non-fatal condition.
        /// Examples:
        /// - An invalid action (ex: playing a card the player doesn't own).
        /// - A logical issue (ex: trying to buy an item without enough money).
        /// - A game constraint that prevents the execution (ex: playing a card when it's not your turn).
        /// 
        /// ⚠ To not be confused with `Invalid`, which concerns invalid inputs.
        /// </summary>
        Failure,

        /// <summary>
        /// The command could not be executed because the input was invalid.
        /// Examples:
        /// - Null or invalid parameter (ex: `null` passed as an argument when a card is required).
        /// - ID of a non-existent object (ex: trying to interact with a card that doesn't exist).
        /// - Data format issue (ex: a malformed JSON for a network command).
        /// 
        /// ⚠ To not be confused with `Failure`, which concerns impossible but valid actions.
        /// </summary>
        Invalid,

        /// <summary>
        /// The command encountered a critical error that could affect the proper functioning of the game.
        /// Examples:
        /// - An unhandled exception (ex: `NullReferenceException`).
        /// - A potential crash (ex: a memory error with native pointers).
        /// - A network synchronization issue that corrupts the game state.
        /// - An error in the rendering / physics engine (ex: a `RigidBody` that becomes `NaN`).
        /// 
        /// 💥 This type of error should generally be logged and handled specifically.
        /// </summary>
        CriticalError
    }

    public readonly struct CommandResponse {

        public CommandStatus Status { get; }

        public string Message { get; }

        public string[] Args { get; }

        public CommandResponse(CommandStatus status, string message = "", params string[] args) : this()
        {
            Status  = status;
            Message = message;
            Args    = args;
        }
    }
}
