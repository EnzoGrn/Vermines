using Fusion;
using UnityEngine;

namespace Vermines {

    /// <summary>
    /// Runtime data structure to hold player profile for in-game use.
    /// </summary>
    public struct PlayerProfile : INetworkStruct {

        [Networked, Capacity(24)]
        [Tooltip("The player's nickname.\nCan be up to 24 characters long.")]
        public string Nickname
        {
            get => default;
            set {}
        }

        public PlayerRef PlayerRef;

        public bool IsConnected;
    }

    /// <summary>
    /// Runtime data structure to hold player information for in-game use.
    /// </summary>
    public struct PlayerDataStruct : INetworkStruct {

        // -- Player profile --

        public PlayerProfile Profile;

        // -- Player's data --

        public int Eloquence;

        public int Souls;

        public CardType Family;
    }

    /// <summary>
    /// Data structure to hold every player's data.
    /// It's synchronized across the network automatically thanks to '[Networked]' attribute.
    /// </summary>
    public class PlayerData : NetworkBehaviour {

        // -- Player's data --

        [Networked]
        public PlayerDataStruct Data { get; set; }

        // -- Player's Deck --

        [Networked, Capacity(60)] // TODO: Maybe in future add more cards possibility to the deck
        public NetworkArray<int> Deck { get; }

        [Networked, Capacity(60)] // TODO: Maybe in future add more cards possibility to the hand
        public NetworkArray<int> Hand { get; }

        [Networked, Capacity(60)] // TODO: Maybe in future add more cards possibility to the sacrifice pile
        public NetworkArray<int> Sacrifice { get; }

        [Networked, Capacity(60)] // TODO: Maybe in future add more cards possibility to the discard pile
        public NetworkArray<int> Discard { get; }

        [Networked, Capacity(4)] // TODO: Maybe in future add more cards possibility to the partisan board
        public NetworkArray<int> PartisanBoard { get; }

        [Networked, Capacity(3)] // TODO: Maybe in future add more cards possibility to the equipment board
        public NetworkArray<int> EquipmentBoard { get; }
    }
}
