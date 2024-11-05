/*
 * @brief Observers interface for the UpdateManager objects.
 * Inherit this class in your Monobehaviour class that needs to be updated by the UpdateManager.
 */
public interface IUpdateObserver {

    /*
     * @brief Observed update is egal to the MonoBehaviour Update method,
     * but it is called by the UpdateManager, that enhance the optimization.
     */
    void ObservedUpdate();
}
