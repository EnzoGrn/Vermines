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

        public CommandResponse Execute()
        {
            CardSetDatabase.Instance.Initialize(_Families);

            if (CardSetDatabase.Instance.Size == 0)
                return new CommandResponse(CommandStatus.Failure, $"Failed to initialize the game with the families: {string.Join(", ", _Families)}");
            return new CommandResponse(CommandStatus.Success, $"Game initialized with the families: {string.Join(", ", _Families)}");
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

        public CommandResponse Execute()
        {
            _OldDecks = GameDataStorage.Instance.PlayerDeck;

            foreach (string playerData in _PlayersData) {
                string[] playerDeck = playerData.Split(':', StringSplitOptions.RemoveEmptyEntries);

                if (playerDeck.Length != 2)
                    return new CommandResponse(CommandStatus.Invalid, $"Malformed data during deck initialization.\n{_PlayersData}");
                try {
                    InitializeDeck(playerDeck[0], playerDeck[1]);
                } catch (Exception) {
                    Debug.LogWarning($"[SERVER]: Error during deck initialization of {playerDeck[0]} - {playerDeck[1]}");

                    return new CommandResponse(CommandStatus.Invalid, $"Malformed data during deck initialization.\n{_PlayersData}");
                }
            }

            return new CommandResponse(CommandStatus.Success, "Decks initialized.");
        }

        public void Undo()
        {
            GameDataStorage.Instance.PlayerDeck = _OldDecks;
        }

        private void InitializeDeck(string playerData, string deckData)
        {
            PlayerRef  playerRef = PlayerRef.FromEncoded(int.Parse(playerData));
            PlayerDeck deck      = PlayerDeck.Deserialize(deckData);

            deck.Deck.ForEach(card        => card.Owner = playerRef);
            deck.Hand.ForEach(card        => card.Owner = playerRef);
            deck.Discard.ForEach(card     => card.Owner = playerRef);
            deck.Graveyard.ForEach(card   => card.Owner = playerRef);
            deck.PlayedCards.ForEach(card => card.Owner = playerRef);
            deck.Equipments.ForEach(card  => card.Owner = playerRef);

            GameDataStorage.Instance.PlayerDeck[playerRef] = deck;
        }
    }
}
