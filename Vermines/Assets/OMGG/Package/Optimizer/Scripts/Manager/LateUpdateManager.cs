using System.Collections.Generic;
using UnityEngine;

/*
 * @brief Late Update Manager class, that enhance the optimization of the LateUpdateManager method.
 * It simulate every late update method of our game that is subscribed to the LateUpdateManager.
 * 
 * Thanks to that every slowing method of unity will be ignored, and will improve the performance of the game.
 */
public class LateUpdateManager : MonoBehaviour {

    #region Singleton

    /*
     * @brief Singleton instance of the LateUpdate.
     * Allow the Manager to be get everywhere in the code.
     * And if not exist, it will be instantiated, by the creation of a game object and the addition of the component.
     */
    public static Singleton<LateUpdateManager> Instance { get; private set; }

    #endregion

    #region Attributes

    /*
     * @brief List of the observers that are subscribed to the LateUpdate.
     */
    private readonly List<ILateUpdateObserver> _Observers = new();

    /*
     * @brief List of the observers that are pending to be unsubscribed to the LateUpdate.
     */
    private readonly List<ILateUpdateObserver> _PendingObservers = new();

    #endregion

    #region Monobeaviour Methods

    private void OnDestroy()
    {
        _Observers.Clear();
        _PendingObservers.Clear();
    }

    private void LateUpdate()
    {
        // -- Loop through all the observers and call the ObservedLateUpdate method.
        for (int currentIndex = _Observers.Count - 1; currentIndex >= 0; currentIndex--)
            _Observers[currentIndex].ObservedLateUpdate();

        // -- Loop through all the pending observers and remove them from the observers list.
        for (int i = 0; i < _PendingObservers.Count; i++)
            _Observers.Remove(_PendingObservers[i]);
    }

    #endregion

    #region Static Methods /* that need to be called by objects scripts */

    public void RegisterObserver(ILateUpdateObserver observer)
    {
        _Observers.Add(observer);
    }

    public void UnregisterObserver(ILateUpdateObserver observer)
    {
        _PendingObservers.Add(observer);
    }

    #endregion

    #region Getters

    /*
     * @brief Getters that return if the LateUpdateManager has observers or not.
     */
    public bool HasObservers => _Observers.Count > 0;

    #endregion
}