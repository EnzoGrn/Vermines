using Fusion;
using OMGG.Network.Fusion;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using Vermines.Network.Utilities;

namespace Vermines
{
    public class ResolutionPhase : NetworkBehaviour
    {
        public PhaseType PhaseType;

        // Singleton
        public static ResolutionPhase Instance => NetworkSingleton<ResolutionPhase>.Instance;
        public override void Spawned()
        {
            name = NetworkNameTools.GiveNetworkingObjectName(Object.InputAuthority, HasInputAuthority, HasStateAuthority);
        }

        public ResolutionPhase()
        {
            PhaseType = PhaseType.Resolution;
            //OnEndPhase = new UnityEvent();
        }

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
        //    // For effect based on the end of the turn maybe do something?

        //    // Discard all the cards

        //    // Draw cards

        //}
    }
}
