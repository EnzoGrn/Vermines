using System.Numerics;

namespace OMGG.Network {

    /*
     * @brief ISyncObject is an interface that represents the management of synchronized objects (instantiation, destruction).
     * It contains the object's unique identifier, as well as the ability to synchronize the object across the network.
     */
    public interface ISyncObject {

        /*
         * @brief The unique identifier of the object.
         */
        string ObjectId { get; }

        /*
         * @brief Methods for spawning the object.
         * 
         * @note The object to be instantiated must be a prefab.
         *
         * @important The initiliazeData parameter is used to pass data to the object when it is instantiated.
         */
        void Spawn(string prefabId, Vector3 position, Quaternion rotation, object initiliazeData = null);

        /*
         * @brief Method for destroying the object.
         */
        void Destroy();

        /*
         * @brief Method for send more information about the object.
         */
        void SyncState(object state);
    }
}
