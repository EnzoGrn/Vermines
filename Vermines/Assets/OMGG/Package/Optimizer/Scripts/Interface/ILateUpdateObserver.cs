namespace OMGG.Optimizer {

    /*
     * @brief Observers interface for the LateUpdateManager objects.
     * Inherit this class in your Monobehaviour class that needs to be late updated by the LateUpdateManager.
     */
    public interface ILateUpdateObserver {

        /*
         * @brief Observed late update is egal to the MonoBehaviour LateUpdate method,
         * but it is called by the LateUpdateManager, that enhance the optimization.
         */
        void ObservedLateUpdate();
    }
}