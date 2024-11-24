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
         */
        void Spawn(string prefabId, Vector3 position, Quaternion rotation);

        /*
         * @brief Method for destroying the object.
         */
        void Destroy();
    }
}
