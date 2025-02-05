using Fusion;
using System.Diagnostics;
using UnityEngine.Events;
namespace Vermines.Player
{
    public struct WaitingRoomPlayerData : INetworkStruct
    {
        [Networked, Capacity(24)]
        public string Nickname
        {
            get => default;
            set { }
        }

        [Networked]
        public bool IsReady
        {
            get => default;
            set {
                //OnIsReadyUpdated.Invoke();
            }
        }

        // Expose this through a property, if you want
        //public bool IsReady
        //{
        //    get => IsReadyNetwork;
        //    set
        //    {
        //        if (IsReady != value)
        //        {
        //            IsReadyNetwork = value;
        //            Log.DebugWarn($"IsReady Just changed right now (nickname: {Nickname})");
        //        }
        //    }
        //}

        //public UnityEvent OnIsReadyUpdated;

        public PlayerRef PlayerRef;
        public bool IsHost;
    }
}