/*
 * @brief Observers interface for the FixedUpdateManager objects.
 * Inherit this class in your Monobehaviour class that needs to be fixed update by the FixedUpdateManager.
 */
public interface IFixedUpdateObserver {

    /*
     * @brief Observed fixed update is egal to the MonoBehaviour FixedUpdate method,
     * but it is called by the FixedUpdateManager, that enhance the optimization.
     */
    void ObservedFixedUpdate();
}
