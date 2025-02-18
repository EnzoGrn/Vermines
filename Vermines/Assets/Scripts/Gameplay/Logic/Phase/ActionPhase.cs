using Fusion;
using OMGG.Network.Fusion;
using UnityEngine;
using UnityEngine.Events;
using Vermines.Network.Utilities;

namespace Vermines
{
    //public class ActionPhase : APhaseBase
    public class ActionPhase : NetworkBehaviour
    {
        public PhaseType PhaseType;

        // Singleton
        public static ActionPhase Instance => NetworkSingleton<ActionPhase>.Instance;
        public override void Spawned()
        {
            name = NetworkNameTools.GiveNetworkingObjectName(Object.InputAuthority, HasInputAuthority, HasStateAuthority);
        }

        public ActionPhase()
        {
            PhaseType = PhaseType.Action;
        }
        //{
            //OnEndPhase = new UnityEvent();
        //}

        public void RunPhase(PlayerRef playerRef)
        {
            Debug.Log($"Phase {PhaseType} is now running");
            return;
        }

        public void EndPhase()
        {
            // Notify the PhaseManager that the phase is completed
            //OnEndPhase.Invoke();
            GameEvents.OnAttemptNextPhase.Invoke();

            //GameEvents.AttemptNextPhase();

            Debug.Log($"End of the {PhaseType.ToString()} OnEndPhase.");
        }

        //public void EndPhase()
        //{
        //    // Refill all the shops
        //}
    }
}
