using System.Collections.Generic;
using System.Linq;
using OMGG.Network.Fusion;
using OMGG.DesignPattern;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vermines {
    using Vermines.Gameplay.Commands.Internal;
    using Vermines.Gameplay.Commands.Deck;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.ShopSystem.Commands.Internal;
    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem.Data;
    using Vermines.Player;
    using Vermines.ShopSystem.Commands;
    using Vermines.Gameplay.Phases;
    using Vermines.HUD;
    using Vermines.HUD.Card;
    using Mono.Cecil.Cil;

    public class GameInitializer : NetworkBehaviour {

        private readonly NetworkQueue _Queue = new();

        #region Methods

        public override void Spawned()
        {
            // Load UI Scene
            if (Runner.IsServer)
            {
                Debug.Log("LoadYourAsyncScene.");
                Runner.LoadScene("EnvironmentDay", LoadSceneMode.Additive);
                Runner.LoadScene("GameplayCameraTravelling", LoadSceneMode.Additive);
                Runner.LoadScene("UI", LoadSceneMode.Additive);
            }
        }

        public int InitializePlayers(int seed, int startingEloquence)
        {
            GameDataStorage storage = GameDataStorage.Instance;
            int numberOfPlayer = storage.PlayerData.Count;

            // TODO: Create a Game config with a number of player required
            // For now just check if the number of player is 2 or more.
            if (numberOfPlayer < 2)
                return -1;
            InitializePlayers(seed, numberOfPlayer, startingEloquence);

            return 0;
        }

        private void InitializePlayers(int seed, int numberOfPlayer, int startingEloquence)
        {
            List<CardFamily> families = FamilyUtils.GenerateFamilies(seed, numberOfPlayer);
            int orderIndex = 0;

            foreach (var player in GameDataStorage.Instance.PlayerData)
            {
                Vermines.Player.PlayerData data = player.Value;

                data.Family = families[orderIndex];
                data.Eloquence = GiveEloquence(orderIndex, startingEloquence);

                GameDataStorage.Instance.PlayerData.Set(player.Key, data);

                orderIndex++;
            }

            RPC_InitializeGame(seed, FamilyUtils.FamiliesListToIds(families));
        }

        public int InitalizePhase()
        {
            RPC_InitializePhase();

            return 0;
        }

        public int InitializeShop(int seed)
        {
            List<ICard> everyBuyableCard = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.IsStartingCard == false);

            if (everyBuyableCard == null || everyBuyableCard.Count == 0)
                return -1;
            List<ICard> objectCards = everyBuyableCard.Where(card => card.Data.Type == CardType.Equipment || card.Data.Type == CardType.Tools).ToList();

            if (objectCards == null || objectCards.Count == 0)
                return -1;
            objectCards.Shuffle(seed);

            List<ICard> partisanCards = everyBuyableCard.Where(card => card.Data.Type == CardType.Partisan).ToList();

            if (partisanCards == null || partisanCards.Count == 0)
                return -1;

            List<ICard> partisan1Cards = partisanCards.Where(card => card.Data.Level == 1).ToList();
            List<ICard> partisan2Cards = partisanCards.Where(card => card.Data.Level == 2).ToList();

            if (partisan1Cards == null || partisan2Cards == null || partisan1Cards.Count == 0 || partisan2Cards.Count == 0)
                return -1;
            partisan1Cards.Shuffle(seed);
            partisan2Cards.Shuffle(seed);

            partisan1Cards.Merge(partisan2Cards);

            partisanCards = partisan1Cards;

            ShopData shop = ScriptableObject.CreateInstance<ShopData>();

            shop.Initialize(ShopType.Market, GameManager.Instance.Config.MaxMarketCards.Value);
            shop.FillShop(ShopType.Market, objectCards);

            shop.Initialize(ShopType.Courtyard, GameManager.Instance.Config.MaxCourtyardCards.Value);
            shop.FillShop(ShopType.Courtyard, partisanCards);

            GameDataStorage.Instance.Shop = shop;

            // -- RPC Command for sync initialization
            RPC_InitializeShop(GameDataStorage.Instance.Shop.Serialize());

            return 0;
        }

        public int DeckDistribution(System.Random rand)
        {
            GameDataStorage storage = GameDataStorage.Instance;
            List<ICard> starterCards = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.IsStartingCard == true);

            if (starterCards == null || starterCards.Count == 0)
                return -1;
            int starterDeckLength = starterCards.Count / storage.PlayerData.Count;
            string serializedPlayerDeck = string.Empty;

            Dictionary<PlayerRef, PlayerDeck> decks = new();

            foreach (var player in storage.PlayerData)
            {
                PlayerDeck deck = new();

                deck.Initialize();

                for (int i = 0; i < starterDeckLength; i++)
                {
                    ICard card = starterCards[rand.Next(starterDeckLength - deck.Deck.Count)];

                    deck.Deck.Add(card);
                    starterCards.Remove(card);
                }

                decks.Add(player.Key, deck);
            }

            GameDataStorage.Instance.PlayerDeck = decks;

            // -- RPC Command for sync initialization
            RPC_InitializeDeck(storage.SerializeDeck());

            return 0;
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
            _Queue.EnqueueRPC(() => {
                SetGameSeed(seed);

                List<CardFamily> familiesList = FamilyUtils.FamiliesIdsToList(familiesIds);

                ICommand initializeCommand = new InitializeGameCommand(familiesList);

                CommandResponse response = CommandInvoker.ExecuteCommand(initializeCommand);

                if (response.Status == CommandStatus.Success)
                    Debug.Log($"[SERVER]: {CardSetDatabase.Instance.Size} cards instantiated!");
                else
                    Debug.LogWarning($"[SERVER]: {response.Message}");
            });
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_InitializeDeck(string data)
        {
            _Queue.EnqueueRPC(() => {
                Debug.Log($"[SERVER]: Deck initialization:");
                Debug.Log($"[SERVER]: {data}");

                ICommand initializeCommand = new InitializeDeckCommand(data);

                CommandInvoker.ExecuteCommand(initializeCommand);
            });
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_InitializeShop(string data)
        {
            _Queue.EnqueueRPC(() => {
                Debug.Log($"[SERVER]: Shop initialization:");
                Debug.Log($"[SERVER]: {data}");

                // Ignore the host because it's shop is already initialized (it's just synchronising the shop with others)
                if (HasStateAuthority == false)
                {
                    GameDataStorage.Instance.Shop = ScriptableObject.CreateInstance<ShopData>();

                    ICommand initializeCommand = new SyncShopCommand(GameDataStorage.Instance.Shop, data, GameManager.Instance.Config);

                    CommandResponse syncStatus = CommandInvoker.ExecuteCommand(initializeCommand);

                    if (syncStatus.Status != CommandStatus.Success)
                        Debug.LogWarning($"[SERVER]: {syncStatus.Message}");
                }
                ICommand fillCommand = new FillShopCommand(GameDataStorage.Instance.Shop);

                CommandResponse response = CommandInvoker.ExecuteCommand(fillCommand);
            });
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_StartingDraw(int numberOfCardToDraw)
        {
            _Queue.EnqueueRPC(() => {
                for (int i = 0; i < numberOfCardToDraw; i++)
                {
                    var deckCopy = GameDataStorage.Instance.PlayerDeck.ToList();

                    foreach (var player in deckCopy)
                    {
                        ICommand drawCommand = new DrawCommand(player.Key);

                        CommandResponse command = CommandInvoker.ExecuteCommand(drawCommand);

                        if (command.Status == CommandStatus.Success && PlayerController.Local.PlayerRef == player.Key) {
                            PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[player.Key];

                            GameEvents.InvokeOnDrawCard(deck.Hand.Last());
                        }
                    }
                }

                Debug.Log($"[SERVER]: Deck after everyone draw their cards:");
                Debug.Log($"[SERVER]: {GameDataStorage.Instance.SerializeDeck()}");
            });
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_InitializePhase()
        {
            _Queue.EnqueueRPC(() => {
                PhaseManager.Instance.Initialize();
            });
        }

        #endregion
    }
}