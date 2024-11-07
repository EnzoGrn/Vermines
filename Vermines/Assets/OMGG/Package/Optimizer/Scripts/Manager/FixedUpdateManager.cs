using System.Collections.Generic;
using UnityEngine;

/*
 * @brief Fixed Update Manager class, that enhance the optimization of the FixedUpdate method.
 * It simulate every fixed update method of our game that is subscribed to the FixedUpdateManager.
 * 
 * Thanks to that every slowing method of unity will be ignored, and will improve the performance of the game.
 */
public class FixedUpdateManager : Singleton<FixedUpdateManager> {

    #region Attributes

    /*
     * @brief List of the observers that are subscribed to the FixedUpdate.
     */
    private readonly List<IFixedUpdateObserver> _Observers = new();

    /*
     * @brief List of the observers that are pending to be unsubscribed to the FixedUpdate.
     */
    private readonly List<IFixedUpdateObserver> _PendingObservers = new();

    #endregion

    #region Monobeaviour Methods

    private void OnDestroy()
    {
        _Observers.Clear();
        _PendingObservers.Clear();
    }

    private void FixedUpdate()
    {
        // -- Loop through all the observers and call the ObservedFixedUpdate method.
        for (int currentIndex = _Observers.Count - 1; currentIndex >= 0; currentIndex--) {
            MonoBehaviour script = _Observers[currentIndex] as MonoBehaviour;

            // -- Check if the observer is active and enabled.
            // If it is, call the ObservedUpdate method.
            // If not, let's it in hold until it is active and enabled.
            if (script && script.isActiveAndEnabled)
                _Observers[currentIndex].ObservedFixedUpdate();
        }

        // -- Loop through all the pending observers and remove them from the observers list.
        for (int i = 0; i < _PendingObservers.Count; i++)
            _Observers.Remove(_PendingObservers[i]);
    }

    #endregion

    #region Methods /* that need to be called by objects scripts */

    public void RegisterObserver(IFixedUpdateObserver observer)
    {
        _Observers.Add(observer);
    }

    public void UnregisterObserver(IFixedUpdateObserver observer)
    {
        _PendingObservers.Add(observer);
    }

    #endregion

    #region Getters

    /*
     * @brief Getters that return if the FixedUpdateManager has observers or not.
     */
    public bool HasObservers => _Observers.Count > 0;

    #endregion
}
