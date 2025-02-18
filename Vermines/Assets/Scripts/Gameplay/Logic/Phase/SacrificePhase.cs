using Fusion;
using OMGG.Network.Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Vermines.CardSystem.Elements;
using Vermines.Network.Utilities;

namespace Vermines
{
    public class SacrificePhase : NetworkBehaviour
    {
        private int _MaxNumberOfCardsToSacrifice;
        private int _NumberOfCardSacrified;

        public PhaseType PhaseType;

        // Singleton
        public static SacrificePhase Instance => NetworkSingleton<SacrificePhase>.Instance;

        public override void Spawned()
        {
            name = NetworkNameTools.GiveNetworkingObjectName(Object.InputAuthority, HasInputAuthority, HasStateAuthority);
        }

        public SacrificePhase()
        {
            PhaseType = PhaseType.Sacrifice;
        }
        //{
        //    PhaseType = PhaseType.Sacrifice;
        //    //OnEndPhase = new UnityEvent();
        //}

        public void RunPhase(PlayerRef playerRef)
        {
            Debug.Log($"Phase {PhaseType} is now running");

            _MaxNumberOfCardsToSacrifice = GameManager.Instance.Config.MaxSacrificesPerTurn.Value;
            _NumberOfCardSacrified = 0;

            // Get PlayedCards Deck
            List<ICard> PlayedCards = GameDataStorage.Instance.PlayerDeck[playerRef].PlayedCards;

            // Check if the player has some cards in the PlayedCards that can go to the graveyard
            if (PlayedCards.Count > 0)
            {
                // Invite the player to pick one played card and sacrifice it
                // TODO: Use the HUDManager.

                Debug.Log($"You can pick a card to sacrifice, scrifice left ({_MaxNumberOfCardsToSacrifice - _NumberOfCardSacrified}).");
            }
            else
            {
                EndPhase();
            }
        }

        public void EndPhase()
        {
            // Notify the PhaseManager that the phase is completed
            //OnEndPhase.Invoke();
            GameEvents.OnAttemptNextPhase.Invoke();

            //GameEvents.AttemptNextPhase();

            Debug.Log($"End of the {PhaseType.ToString()} OnEndPhase.");
        }

        #region Events
        public void OnCardSacrified()
        {
            _NumberOfCardSacrified++;

            if (_NumberOfCardSacrified < _MaxNumberOfCardsToSacrifice)
            {
                // Invite the player to pick one played card and sacrifice it
                // TODO: Use the HUDManager.

                Debug.Log($"You can pick a card to sacrifice, scrifice left ({_MaxNumberOfCardsToSacrifice - _NumberOfCardSacrified}).");
            }
            else
            {
                EndPhase();

            }
        }
        #endregion
    }
}
