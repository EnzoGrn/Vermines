using Fusion;
using OMGG.Network.Fusion;
using UnityEngine;
using UnityEngine.Events;
using Vermines.Network.Utilities;

namespace Vermines
{
    /// <summary>
    /// The Gain phase corresponds to the phase that distribute the eloquence of a new turn
    /// </summary>
    public class GainPhase : NetworkBehaviour
    {
        public PhaseType PhaseType;

        // Singleton
        public static GainPhase Instance => NetworkSingleton<GainPhase>.Instance;


        public override void Spawned()
        {
            name = NetworkNameTools.GiveNetworkingObjectName(Object.InputAuthority, HasInputAuthority, HasStateAuthority);
        }

        public GainPhase()
        {
            PhaseType = PhaseType.Gain;
        }
        //{
        //    PhaseType = PhaseType.Gain;
        //    //OnEndPhase = new UnityEvent();
        //}

        public void RunPhase(PlayerRef playerRef)
        {
            Debug.Log($"Phase {PhaseType} is now running");

            int eloquence = GameDataStorage.Instance.PlayerData[playerRef].Eloquence;

            eloquence += GameManager.Instance.Config.NumberOfEloquencesToStartTheTurnWith.Value;

            Debug.Log($"New Value of Eloquence to gain {eloquence}");

            RPC_SetEloquence(playerRef, eloquence);
        }

        public void EndPhase()
        {
            // Notify the PhaseManager that the phase is completed
            //OnEndPhase.Invoke();
            GameEvents.OnAttemptNextPhase.Invoke();

            //GameEvents.AttemptNextPhase();

            Debug.Log($"End of the {PhaseType.ToString()} OnEndPhase.");
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        public void RPC_SetEloquence(PlayerRef playerRef, int eloquence)
        {
            Debug.LogWarning($"RPC_SetEloquence Received is runner.IsServer {Runner.IsServer} by {playerRef}");
            GameDataStorage.Instance.SetEloquence(Runner.LocalPlayer, eloquence);
            EndPhase();
        }
    }
}
