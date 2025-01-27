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

        public int Initialize(int seed, int startingEloquence)
        {
            GameDataStorage storage = GameDataStorage.Instance;
            int      numberOfPlayer = storage.PlayerData.Count;

            // TODO: Create a Game config with a number of player required
            // For now just check if the number of player is 2 or more.
            if (numberOfPlayer < 2)
                return -1;
            List<CardFamily> families = FamilyUtils.GenerateFamilies(seed, numberOfPlayer);

            // -- Player Initialization
            int orderIndex = 0;

            foreach (var player in storage.PlayerData) {
                PlayerData data = player.Value;

                data.Family    = families[orderIndex];
                data.Eloquence = GiveEloquence(orderIndex, startingEloquence);

                storage.PlayerData.Set(player.Key, data);

                orderIndex++;
            }

            RPC_InitializeGame(seed, FamilyUtils.FamiliesListToIds(families));

            return 0;
        }

        public void DeckDistribution(System.Random rand)
        {
            GameDataStorage     storage = GameDataStorage.Instance;
            List<ICard>    starterCards = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.IsStartingCard == true);
            int       starterDeckLength = starterCards.Count / storage.PlayerData.Count;
            string serializedPlayerDeck = string.Empty;

            Dictionary<PlayerRef, PlayerDeck> decks = new();

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

        public void StartingDraw(int numberOfCardToDraw)
        {
            RPC_StartingDraw(numberOfCardToDraw);
        }

        private int GiveEloquence(int index, int startingEloquence)
        {
            return startingEloquence + Mathf.Min(index, 2);
        }

        private void SetGameSeed(int seed)
        {
            GameManager.Instance.Config.Seed = seed;
        }

        #endregion

        #region Commands

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_InitializeGame(int seed, int[] familiesIds)
        {
            SetGameSeed(seed);

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
