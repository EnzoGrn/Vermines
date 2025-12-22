namespace OMGG.DesignPattern {

    /*
     * @brief Command interface, for all the command objects.
     */
    public interface ICommand {

        /*
         * @brief Execute the command.
         */
        CommandResponse Execute();

        /*
         * @brief Function to undo the command.
         */
        void Undo();
    }

    public abstract class ACommand : ICommand {

        public abstract CommandResponse Execute();

        public virtual void Undo() { }
    }
}
