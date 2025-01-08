using System;

namespace OMGG.Network {

    /*
     * @brief INetworkAuthority is an interface that represents the management of authority transfer.
     * It is used to transfer authority between players.
     */
    public interface INetworkAuthority {

        /*
         * @brief Check if the entity has the authority to perform actions on the object.
         */
        bool HasAutority(string objectId);

        /*
         * @brief Force the transfer of authority to another player.
         */
        void TransferAuthority(string objectId, string newOwner);

        /*
         * @brief Event that is triggered when the authority is transferred.
         */
        event Action<string /* objectId */, string /* newOwner */> OnAuthorityTransfered;
    }
}
