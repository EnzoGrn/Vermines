using System.Collections.Generic;
using System;
using OMGG.DesignPattern;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Commands.Internal {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Data;
    using Vermines.Player;

    public class InitializeGameCommand : ICommand {

        private readonly List<CardFamily> _Families;

        public InitializeGameCommand(List<CardFamily> families)
        {
            _Families = families;
        }

        public void Execute()
        {
            CardSetDatabase.Instance.Initialize(_Families);
        }

        public void Undo()
        {
            CardSetDatabase.Instance.Clear();
        }
    }

    public class InitializeDeckCommand : ICommand {

        private readonly string[] _PlayersData;

        private Dictionary<PlayerRef, PlayerDeck> _OldDecks;

        /// <summary>
        /// Pass every data needed to initialize the deck of every player.
        /// </summary>
        /// <data>
        /// [Player:1]:Deck[1,3,5,4,2];Hand[];Discard[];Graveyard[]|[Player:2]:Deck[1,3,5,4,2];Hand[];Discard[];Graveyard[]
        /// </data>
        /// <param name="data">The data to parse for initialize every deck</param>
        public InitializeDeckCommand(string data)
        {
            _PlayersData = data.Split('|', StringSplitOptions.RemoveEmptyEntries);
        }

        public void Execute()
        {
            _OldDecks = GameDataStorage.Instance.PlayerDeck;

            foreach (string playerData in _PlayersData) {
                string[] playerDeck = playerData.Split(':', StringSplitOptions.RemoveEmptyEntries);

                if (playerDeck.Length != 2)
                    continue;
                try {
                    InitializeDeck(playerDeck[0], playerDeck[1]);
                } catch (Exception) {
                    Debug.LogWarning($"[SERVER]: Error during deck initialization of {playerDeck[0]} - {playerDeck[1]}");

                    continue;
                }
            }
        }

        public void Undo()
        {
            GameDataStorage.Instance.PlayerDeck = _OldDecks;
        }

        private void InitializeDeck(string playerData, string deckData)
        {
            PlayerRef playerRef = PlayerRef.FromEncoded(int.Parse(playerData));
            PlayerDeck     deck = PlayerDeck.Deserialize(deckData);

            GameDataStorage.Instance.PlayerDeck[playerRef] = deck;
        }
    }
}
