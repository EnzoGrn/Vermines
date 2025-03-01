using System.Collections.Generic;

namespace OMGG.DesignPattern {

    /// <summary>
    /// Class that is responsible for executing and undoing commands.
    /// It store every command that have been executed, to allow the undo function.
    /// </summary>
    public class CommandInvoker {

        /// <summary>
        /// @brief Stack of all the commands that have been executed, for the undo function.
        /// </summary>
        /// <note>
        /// The stack is static, so it is shared between all the commands.
        /// </note>
        private static readonly Stack<ICommand> _UndoStack = new();

        /// <summary>
        /// The state of the last command that have been executed
        /// </summary>
        public static CommandResponse State = new(CommandStatus.Success, "No command executed yet.");

        /// <summary>
        /// Function that execute a command, that is passed as parameter.
        /// The command is then pushed in the undo stack.
        /// </summary>
        public static CommandResponse ExecuteCommand(ICommand command)
        {
            State = command.Execute();

            _UndoStack.Push(command);

            return State;
        }

        public static void UndoCommand()
        {
            if (_UndoStack.Count > 0) {
                // -- Get the last command that have been executed, for the undo function.
                ICommand activeCommand = _UndoStack.Pop();

                activeCommand.Undo();
            }
        }
    }
}
