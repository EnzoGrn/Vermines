using UnityEngine;
using System.Collections.Generic;

namespace Vermines.HUD.Card
{
    using OMGG.DesignPattern;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.ShopSystem.Enumerations;

    public class CardSpawner : MonoBehaviourSingleton<CardSpawner>
    {
        public GameObject cardPrefab;
        public Transform handContainer, marketContainer, courtyardContainer, tableContainer;

        [Header("Debug")]
        public bool debugMode = false;
        public List<CardData> debugCardList;
        public Dictionary<int, CardData> debugCardDictionary;
        public Dictionary<ShopType, Dictionary<int, ICard>> ShopCardDictionaries = new();
        private Dictionary<ShopType, Dictionary<int, GameObject>> _ShopSpawnedCardDictionaries = new();

        public (ShopType, int)? GetCard(int id)
        {
            Debug.Log($"[CLIENT] [{gameObject.name}] [{nameof(GetCard)}] Check if card exist with id: {id}.", this);

            foreach (KeyValuePair<ShopType, Dictionary<int, ICard>> dictionary in ShopCardDictionaries)
            {
                Debug.Log($"[CLIENT] [{gameObject.name}] [{nameof(GetCard)}] ShopType: {dictionary.Key}, Count Value: {dictionary.Value.Count}.", this);

                foreach (KeyValuePair<int, ICard> slot in dictionary.Value)
                {
                    Debug.Log($"[CLIENT] [{gameObject.name}] [{nameof(GetCard)}] Card ID: {slot.Value.ID}, Card Name: {slot.Value.Data.Name}.", this);

                    if (slot.Value.ID == id)
                    {
                        Debug.Log("Card slot id: " + slot.Key);
                        return (dictionary.Key, slot.Key);
                    }
                }
            }
            return null;
        }

        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            foreach (var shopEvent in GameEvents.OnShopsEvents)
            {
                if (!ShopCardDictionaries.ContainsKey(shopEvent.Key))
                {
                    ShopCardDictionaries.Add(shopEvent.Key, new());
                    //shopEvent.Value.AddListener((id, card) => SetShopCardDictionnary(shopEvent.Key, id, card));
                }

                if (!_ShopSpawnedCardDictionaries.ContainsKey(shopEvent.Key))
                {
                    _ShopSpawnedCardDictionaries.Add(shopEvent.Key, new());
                }
            }

            if (cardPrefab == null)
            {
                Debug.LogError("Card prefab is null!");
                return;
            }
        }

        public void SetShopCardDictionnary(ShopType type, int id, ICard marketCard)
        {
            Debug.Log($"SetShopCardDictionnary type {type}, id {id}, marketCard {marketCard.ID}");
            //ShopCardDictionaries[type].Add(id, marketCard);
            // Remove the card from the spawned card dictionary and card dictionary
            // TODO: Set a null la carte d�truite pour remplacer la carte par une autre plus tard, en faisant une boucle sur les places nulles
            //if (_ShopSpawnedCardDictionaries[type].ContainsKey(id))
            //{
            //    Destroy(_ShopSpawnedCardDictionaries[type][id]);
            //    _ShopSpawnedCardDictionaries[type].Remove(id);
            //}
            //if (ShopCardDictionaries[type].ContainsKey(id))
            //{
            //    ShopCardDictionaries[type].Remove(id);
            //}
        }

        public void SpawnCard(ICard cardData, ShopType shopType)
        {
            if (cardData == null)
            {
                Debug.LogError("Card data is null!");
                return;
            }

            if (cardPrefab == null)
            {
                Debug.LogWarning("Card prefab is null!");
                return;
            }

            // TODO: If the card is already in the shop, in the same slot, don't spawn it again

            GameObject newCard = Instantiate(cardPrefab);
            CardInShop cardInShop;

            switch (shopType)
            {
                case ShopType.Market:
                    cardInShop = newCard.AddComponent<CardInShop>();
                    cardInShop.Initialize(newCard.GetComponent<CardBase>());
                    newCard.transform.SetParent(marketContainer, false);
                    newCard.tag = "ShopCard";
                    _ShopSpawnedCardDictionaries[shopType].Add(cardData.ID, newCard);
                    break;
                case ShopType.Courtyard:
                    cardInShop = newCard.AddComponent<CardInShop>();
                    cardInShop.Initialize(newCard.GetComponent<CardBase>());
                    newCard.transform.SetParent(courtyardContainer, false);
                    newCard.tag = "ShopCard";
                    _ShopSpawnedCardDictionaries[shopType].Add(cardData.ID, newCard);
                    break;
                default:
                    Debug.LogError("Location not found");
                    break;
            }

            newCard.GetComponent<CardBase>().Setup(cardData);
        }

        public void SpawnCardsFromDictionary(Dictionary<int, ICard> cardDictionary, ShopType shopType)
        {
            if (cardDictionary == null)
            {
                Debug.LogError($"[CLIENT] [{gameObject.name}] [{nameof(SpawnCardsFromDictionary)}] Card dictionary is null.", this);
                return;
            }

            int quantity = cardDictionary.Count;
            Debug.Log($"[CLIENT] [{gameObject.name}] [{nameof(SpawnCardsFromDictionary)}] Spawn {quantity} cards from dictionary in {shopType}.", this);

            for (int i = 0; i < quantity; i++)
            {
                ICard cardData = cardDictionary[i];
                if (cardData == null)
                {
                    Debug.LogError($"[CLIENT] [{gameObject.name}] [{nameof(SpawnCardsFromDictionary)}] Card data is null.", this);
                    continue;
                }
                Debug.Log($"[CLIENT] [{gameObject.name}] [{nameof(SpawnCardsFromDictionary)}] Card ID: {cardData.ID}, Card Name: {cardData.Data.Name}.", this);
                SpawnCard(cardData, shopType);
            }
        }

        public void UpdateSpecificShop(Dictionary<int, ICard> cardDictionary, ShopType shopType)
        {
            foreach (var card in cardDictionary)
            {
                bool cardExists = false;
                foreach (var cardInShop in ShopCardDictionaries[shopType])
                {
                    Debug.Log("Card id: " + card.Value.ID + ", CardInShop id: " + cardInShop.Value.ID);

                    if (card.Value.ID == cardInShop.Value.ID && card.Key == cardInShop.Key)
                    {
                        Debug.Log("Card already exists in the shop.");
                        cardExists = true;
                        break;
                    }
                }
                if (!cardExists)
                    ShopCardDictionaries[shopType].Add(card.Key, card.Value);
            }
            SpawnCardsFromDictionary(cardDictionary, shopType);
        }

        public void DestroyCard(int id)
        {
            foreach (var shopEvent in GameEvents.OnShopsEvents)
            {
                if (ShopCardDictionaries[shopEvent.Key].ContainsKey(id))
                {
                    // TODO: Destroy Card GameObject
                    ShopCardDictionaries[shopEvent.Key].Remove(id);
                    break;
                }
            }
        }

        private void OnValidate()
        {
            if (debugMode)
            {
                Debug.LogWarning("CardSpawner: Debug mode is enabled. Make sure to disable it before building the game.");
            }
        }
    }
}