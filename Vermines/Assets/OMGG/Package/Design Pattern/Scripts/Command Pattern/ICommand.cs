namespace OMGG.DesignPattern {

    /*
     * @brief Command interface, for all the command objects.
     */
    public interface ICommand {

        /*
         * @brief Execute the command.
         */
        bool Execute();

        /*
         * @brief Function to undo the command.
         */
        void Undo();
    }
}
