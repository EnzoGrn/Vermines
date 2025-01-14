using System.Collections.Generic;
using OMGG.DesignPattern;
using Fusion;
using UnityEngine;

namespace Vermines {

    using Vermines.Gameplay.Commands.Internal;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Deck;
    using Vermines.CardSystem.Utilities;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.Player;

    public class GameInitializer : NetworkBehaviour {

        #region Methods

        public int Initialize()
        {
            GameDataStorage storage = GameDataStorage.Instance;
            int      numberOfPlayer = storage.PlayerData.Count;

            // TODO: Create a Game config with a number of player required
            // For now just check if the number of player is 2 or more.
            if (numberOfPlayer < 2)
                return -1;
            List<CardFamily> families = FamilyUtils.GenerateFamilies(numberOfPlayer);

            // -- Player Initialization
            int orderIndex = 0;

            foreach (var player in storage.PlayerData) {
                PlayerData data = player.Value;

                data.Family = families[orderIndex];

                if (orderIndex == 0)
                    data.Eloquence = 0;
                else if (orderIndex == 1)
                    data.Eloquence = 1;
                else
                    data.Eloquence = 2;
                orderIndex++;

                storage.PlayerData.Set(player.Key, data);
            }

            RPC_InitializeGame(FamilyUtils.FamiliesListToIds(families));

            return 0;
        }

        public void DeckDistribution()
        {
            GameDataStorage     storage = GameDataStorage.Instance;
            List<ICard>    starterCards = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.IsStartingCard == true);
            int       starterDeckLength = starterCards.Count / storage.PlayerData.Count;
            string serializedPlayerDeck = string.Empty;

            Dictionary<PlayerRef, PlayerDeck> decks = new();
            System.Random rand = new();

            foreach (var player in storage.PlayerData) {
                PlayerDeck deck = new(starterDeckLength);

                for (int i = 0; i < starterDeckLength; i++) {
                    ICard card = starterCards[rand.Next(starterDeckLength - deck.Deck.Count)];

                    deck.Deck.Add(card);
                    starterCards.Remove(card);
                }

                decks.Add(player.Key, deck);
            }

            // -- RPC Command for sync initialization
            RPC_InitializeDeck(storage.SerializeDeck());
        }

        public void StartingDraw()
        {
            int numberOfCardToDraw = 3; // TODO: Add a GameManager config files for the specific data

            RPC_StartingDraw(numberOfCardToDraw);
        }

        #endregion

        #region Commands

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_InitializeGame(int[] familiesIds)
        {
            List<CardFamily> familiesList = FamilyUtils.FamiliesIdsToList(familiesIds);
            ICommand    initializeCommand = new InitializeGameCommand(familiesList);

            CommandInvoker.ExecuteCommand(initializeCommand);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_InitializeDeck(string data)
        {
            ICommand initializeCommand = new InitializeDeckCommand(data);

            CommandInvoker.ExecuteCommand(initializeCommand);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_StartingDraw(int numberOfCardToDraw)
        {
            for (int i = 0; i < numberOfCardToDraw; i++) {
                foreach (var player in GameDataStorage.Instance.PlayerDeck) {
                    ICommand drawCommand = new DrawCommand(player.Key);

                    CommandInvoker.ExecuteCommand(drawCommand);
                }
            }
        }

        #endregion
    }
}
