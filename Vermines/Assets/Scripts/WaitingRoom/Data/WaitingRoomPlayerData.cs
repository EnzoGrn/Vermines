using Fusion;
using System.Diagnostics;
using UnityEditor;
using UnityEngine.Events;
using Vermines.CardSystem.Enumerations;
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

        [Networked, 
            //OnChangedRender(nameof(TestFunction))
        ]
        public bool IsReady
        {
            get => default;
            set {}
        }

        //public void TestFunction()
        //{
        //    if (OnIsReadyUpdated == null)
        //    {
        //        Log.DebugWarn("Ca Update mais c'est nul !");
        //    }

        //    Log.DebugWarn("OUIIIIIIIIII ca UPDATE");
        //    OnIsReadyUpdated.Invoke();
        //}

        //public UnityEvent OnIsReadyUpdated;
        public PlayerRef PlayerRef;
        public bool IsHost;

        //private UnityEvent _OnIsReadyUpdated
        //{
        //    get {  return _OnIsReadyUpdated; }
        //    set { _OnIsReadyUpdated = value; }
        //}

        //public UnityEvent OnIsReadyUpdated
        //{
        //    get => _OnIsReadyUpdated;
        //    set
        //    {
        //        _OnIsReadyUpdated = value;
        //        //OnIsReadyUpdated = value;
        //    }
        //}

        public WaitingRoomPlayerData(PlayerRef player, bool isHost)
        {
            //OnIsReadyUpdated = new UnityEvent();
            PlayerRef = player;
            IsHost = isHost;

            //_OnIsReadyUpdated = new UnityEvent();
            Nickname = "Player " + player.PlayerId;
            IsReady = false;
        }
    }
}