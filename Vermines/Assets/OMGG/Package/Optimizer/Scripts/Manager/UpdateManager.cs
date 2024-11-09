using System.Collections.Generic;
using UnityEngine;
using OMGG.DesignPattern;

namespace OMGG.Optimizer {

    /*
     * @brief Update Manager class, that enhance the optimization of the Update method.
     * It simulate every update method of our game that is subscribed to the UpdateManager.
     * 
     * Thanks to that every slowing method of unity will be ignored, and will improve the performance of the game.
     */
    public class UpdateManager : MonoBehaviourSingleton<UpdateManager> {

        #region Attributes

        /*
         * @brief List of the observers that are subscribed to the UpdateManager.
         */
        private readonly List<IUpdateObserver> _Observers = new();

        /*
         * @brief List of the observers that are pending to be unsubscribed to the UpdateManager.
         */
        private readonly List<IUpdateObserver> _PendingObservers = new();

        #endregion

        #region Monobeaviour Methods

        private void OnDestroy()
        {
            _Observers.Clear();
            _PendingObservers.Clear();
        }

        public void Update()
        {
            // -- Loop through all the observers and call the ObservedUpdate method.
            for (int currentIndex = _Observers.Count - 1; currentIndex >= 0; currentIndex--) {
                MonoBehaviour script = _Observers[currentIndex] as MonoBehaviour;

                // -- Check if the observer is active and enabled.
                // If it is, call the ObservedUpdate method.
                // If not, let's it in hold until it is active and enabled.
                if (script && script.isActiveAndEnabled)
                    _Observers[currentIndex].ObservedUpdate();
            }

            // -- Loop through all the pending observers and remove them from the observers list.
            for (int i = 0; i < _PendingObservers.Count; i++)
                _Observers.Remove(_PendingObservers[i]);
        }

        #endregion

        #region Methods /* that need to be called by objects scripts */

        public void RegisterObserver(IUpdateObserver observer)
        {
            _Observers.Add(observer);
        }

        public void UnregisterObserver(IUpdateObserver observer)
        {
            _PendingObservers.Add(observer);
        }

        #endregion

        #region Getters

        /*
         * @brief Getters that return if the UpdateManager has observers or not.
         */
        public bool HasObservers => _Observers.Count > 0;

        #endregion
    }
}
