using System.Collections.Generic;

/*
 * @brief Class that is responsible for executing and undoing commands.
 * It store every command that have been executed, to allow the undo function.
 */
public class CommandInvoker {

    /*
     * @brief Stack of all the commands that have been executed, for the undo function.
     * 
     * @note The stack is static, so it is shared between all the commands.
     * 
     * @future The stack can may be public in future, to allow the developper to access it,
     * and then create a command historique.
     */
    private static readonly Stack<ICommand> _UndoStack = new();

    /*
     * @brief Function that execute a command, that is passed as parameter.
     * The command is then pushed in the undo stack.
     */
    public static void ExecuteCommand(ICommand command)
    {
        command.Execute();

        _UndoStack.Push(command);
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
