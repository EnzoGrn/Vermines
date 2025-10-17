using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using OMGG.Network.Fusion;
using OMGG.DesignPattern;
using Fusion;

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
    using Vermines.Configuration.Network;

    public class GameInitializer : NetworkBehaviour {

        private readonly NetworkQueue _Queue = new();

        private bool _DeckInitialized = false;

        #region Overrides Methods

        public override void Spawned()
        {
            if (HasStateAuthority) {
                Runner.LoadScene("GameplayCameraTravelling" , LoadSceneMode.Additive); // Scene that active the camera travelling with spline on the map.
                Runner.LoadScene("UI"                       , LoadSceneMode.Additive); // Scene that contains the UI elements for the game.
                Runner.LoadScene("FinalAnimation"           , LoadSceneMode.Additive); // Scene that active the final animation when the game is over.
            }
        }

        #endregion

        #region Methods

        public void InitializePlayers(GameSettingsData settings)
        {
            List<CardFamily> existingFamilies  = GameDataStorage.Instance.GetPlayersFamily();
            List<CardFamily> generatedFamilies = FamilyUtils.GenerateFamilies(settings.Seed, GameDataStorage.Instance.PlayerData.Count, existingFamilies); 

            int index = 0;

            foreach (var playerEntry in GameDataStorage.Instance.PlayerData) {
                PlayerRef playerRef = playerEntry.Key;
                PlayerData     data = playerEntry.Value;

                if (data.Family == CardFamily.None)
                    data.Family = generatedFamilies[index % generatedFamilies.Count];
                data.Eloquence = settings.EloquenceToStartWith;
                data.Souls     = settings.SoulToStartWith;

                GameDataStorage.Instance.PlayerData.Set(playerRef, data);

                index++;
            }

            RPC_InitializeGame(FamilyUtils.FamiliesListToIds(generatedFamilies));
        }

        public int DeckDistribution(System.Random rand)
        {
            List<ICard> starterCards = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.IsStartingCard == true);

            if (starterCards == null || starterCards.Count == 0)
                return -1;
            int    starterDeckLength    = starterCards.Count / GameDataStorage.Instance.PlayerData.Count;
            string serializedPlayerDeck = string.Empty;

            Dictionary<PlayerRef, PlayerDeck> decks = new();

            foreach (var player in GameDataStorage.Instance.PlayerData) {
                PlayerDeck deck = new();

                deck.Initialize();

                for (int i = 0; i < starterDeckLength; i++) {
                    ICard card = starterCards[rand.Next(starterDeckLength - deck.Deck.Count)];

                    deck.Deck.Add(card);
                    starterCards.Remove(card);
                }

                decks.Add(player.Key, deck);
            }

            GameDataStorage.Instance.PlayerDeck = decks;

            // -- RPC Command for sync initialization
            RPC_InitializeDeck(GameDataStorage.Instance.SerializeDeck());

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

            shop.Initialize(ShopType.Market);
            shop.FillShop(ShopType.Market, objectCards);

            shop.Initialize(ShopType.Courtyard);
            shop.FillShop(ShopType.Courtyard, partisanCards);

            GameDataStorage.Instance.Shop = shop;

            // -- RPC Command for sync initialization
            RPC_InitializeShop(GameDataStorage.Instance.Shop.Serialize());

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

        #endregion

        #region Commands

        /// <summary>
        /// Initialize the CardSetDatabase of the game.
        /// </summary>
        /// <param name="familiesIds">
        /// The list of family IDs to initialize the game with.
        /// </param>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_InitializeGame(int[] familiesIds)
        {
            // -- Initialize CardSetDatabase with the families IDs.
            _Queue.EnqueueRPC(() => {
                List<CardFamily> familiesList = FamilyUtils.FamiliesIdsToList(familiesIds);

                ICommand initializeCommand = new InitializeGameCommand(familiesList);

                CommandResponse response = CommandInvoker.ExecuteCommand(initializeCommand);

                if (response.Status == CommandStatus.Success) {
                    Debug.Log($"[{Runner.LocalPlayer.PlayerId}]: Game initialized with {familiesList.Count} families!");
                    Debug.Log($"[{Runner.LocalPlayer.PlayerId}]: {CardSetDatabase.Instance.Size} cards instantiated!");
                } else
                    Debug.LogWarning($"[{Runner.LocalPlayer.PlayerId}]: {response.Message}");
            });
        }

        #region Initialize Deck

        /// <summary>
        /// RPC that synchronizes the deck of each player at the start of the game.
        /// </summary>
        /// <param name="data">
        /// The serialized data of the player decks.
        /// </param>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_InitializeDeck(string data)
        {
            StartCoroutine(WaitAndInitializeDeck(data));
        }

        /// <summary>
        /// This function wait the player correct initialization before initializing the deck of each player.
        /// </summary>
        /// <param name="data">
        /// The serialized data of the player decks.
        /// </param>
        private IEnumerator WaitAndInitializeDeck(string data)
        {
            yield return new WaitUntil(() => PlayerController.Local != null && GameDataStorage.Instance.PlayerDeck != null && CardSetDatabase.Instance != null && CardSetDatabase.Instance.Size > 0);

            ExecuteInitializeDeck(data);
        }

        /// <summary>
        /// This function initializes the deck of each player at the start of the game.
        /// </summary>
        /// <param name="data">
        /// The serialized data of the player decks.
        /// </param>
        private void ExecuteInitializeDeck(string data)
        {
            ICommand initializeCommand = new InitializeDeckCommand(data);

            CommandInvoker.ExecuteCommand(initializeCommand);

            _DeckInitialized = true;
        }

        #endregion

        #region Initialize Shop

        /// <summary>
        /// This function is called by the host to initialize the shop at the start of the game.
        /// </summary>
        /// <param name="data">
        /// The serialized data of the shop.
        /// </param>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_InitializeShop(string data)
        {
            StartCoroutine(WaitAndInitializeShop(data));
        }

        /// <summary>
        /// This function wait the player correct initialization before initializing the shop.
        /// </summary>
        /// <param name="data">
        /// The serialized data of the shop.
        /// </param>
        private IEnumerator WaitAndInitializeShop(string data)
        {
            yield return new WaitUntil(() => PlayerController.Local != null && GameDataStorage.Instance.PlayerDeck != null && CardSetDatabase.Instance != null && CardSetDatabase.Instance.Size > 0);

            ExecuteShopInitialization(data);
        }

        /// <summary>
        /// This function initializes the shop at the start of the game.
        /// </summary>
        /// <param name="data">
        /// The serialized data of the shop.
        /// </param>
        private void ExecuteShopInitialization(string data)
        {
            // Ignore the host because it's shop is already initialized (it's just synchronising the shop with others)
            if (HasStateAuthority == false) {
                _Queue.EnqueueRPC(() => {
                    GameDataStorage.Instance.Shop = ScriptableObject.CreateInstance<ShopData>();

                    ICommand initializeCommand = new SyncShopCommand(GameDataStorage.Instance.Shop, data);

                    CommandResponse syncStatus = CommandInvoker.ExecuteCommand(initializeCommand);

                    if (syncStatus.Status != CommandStatus.Success)
                        Debug.LogWarning($"[SERVER]: {syncStatus.Message}");
                });
            }

            ICommand fillCommand = new FillShopCommand(GameDataStorage.Instance.Shop);

            CommandResponse response = CommandInvoker.ExecuteCommand(fillCommand);
        }

        #endregion

        #region Starting Draw

        /// <summary>
        /// RPC function called by the host to start the first draw of the game.
        /// </summary>
        /// <param name="number">
        /// The number of cards to draw for each player at the start of the game.
        /// </param>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_StartingDraw(int number)
        {
            StartCoroutine(WaitAndDraw(number));
        }

        /// <summary>
        /// This function waits the player correct initialization before starting the draw of cards for each player.
        /// </summary>
        /// <param name="number">
        /// The number of cards to draw for each player at the start of the game.
        /// </param>
        private IEnumerator WaitAndDraw(int number)
        {
            yield return new WaitUntil(() => PlayerController.Local != null && GameDataStorage.Instance.PlayerDeck != null && _DeckInitialized);

            ExecuteStartingDraw(number);
        }

        /// <summary>
        /// Execute the starting draw of cards for each player.
        /// </summary>
        /// <param name="number">
        /// The number of cards to draw for each player at the start of the game.
        /// </param>
        private void ExecuteStartingDraw(int baseCardsToDraw)
        {
            NetworkArray<PlayerRef> order = GameManager.Instance.PlayerTurnOrder;
            int playerCount = GameDataStorage.Instance.PlayerData.Count;
            
            for (int i = 0; i < playerCount; i++) {
                PlayerRef player = order.Get(i);

                if (player == PlayerRef.None)
                    continue;
                int cardsToDraw = (i == 0) ? baseCardsToDraw - 1 : baseCardsToDraw;

                for (int j = 0; j < cardsToDraw; j++) {
                    ICommand    drawCommand = new DrawCommand(player);
                    CommandResponse command = CommandInvoker.ExecuteCommand(drawCommand);

                    if (command.Status == CommandStatus.Success && PlayerController.Local.PlayerRef == player) {
                        PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[player];

                        GameEvents.InvokeOnDrawCard(deck.Hand.Last());
                    }
                }
            }

            Debug.Log($"[{Runner.LocalPlayer.PlayerId}]: First draw is done.");
            Debug.Log($"[{Runner.LocalPlayer.PlayerId}]: {GameDataStorage.Instance.SerializeDeck()}");
        }

        #endregion

        #endregion
    }
}