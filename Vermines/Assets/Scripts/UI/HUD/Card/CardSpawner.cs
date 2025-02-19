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

        public (ShopType, int)? GetCard(int id)
        {
            Debug.Log("GetCard Id: " + id);

            foreach (KeyValuePair<ShopType, Dictionary<int, ICard>> dictionary in ShopCardDictionaries)
            {
                Debug.Log($"ShopType: {dictionary.Key}, Count Value: {dictionary.Value.Count}");

                foreach (KeyValuePair<int, ICard> slot in dictionary.Value)
                {
                    Debug.Log("Slot id: " + slot.Key + ", Id: " + slot.Value.ID);


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
            if (cardPrefab == null)
            {
                Debug.LogError("Card prefab is null!");
                return;
            }

            //if (debugMode)
            //{
            //    debugCardDictionary = new Dictionary<int, CardData>
            //    {
            //        { 0, debugCardList[0] },
            //        { 1, debugCardList[1] },
            //        { 2, debugCardList[2] },
            //        { 3, debugCardList[3] },
            //        { 4, debugCardList[4] }
            //    };
            //    SpawnCardsFromDictionary(debugCardDictionary, "Market");
            //    SpawnCardsFromDictionary(debugCardDictionary, "Courtyard");
            //}

            foreach (var shopEvent in GameEvents.OnShopsEvents)
            {
                ShopCardDictionaries.Add(shopEvent.Key, new ());
                shopEvent.Value.AddListener((id, card) => SetShopCardDictionnary(shopEvent.Key, id, card));
            }
        }

        public void SetShopCardDictionnary(ShopType type, int id, ICard marketCard)
        {
            Debug.Log($"SetShopCardDictionnary type {type}, id {id}, marketCard {marketCard.ID}");
            ShopCardDictionaries[type].Add(id, marketCard);
        }

        public void SpawnCard(ICard cardData, string location)
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

            GameObject newCard = Instantiate(cardPrefab);
            CardInShop cardInShop;

            switch (location)
            {
                case "Market":
                    cardInShop = newCard.AddComponent<CardInShop>();
                    cardInShop.Initialize(newCard.GetComponent<CardBase>());
                    newCard.transform.SetParent(marketContainer, false);
                    newCard.tag = "ShopCard";
                    break;
                case "Courtyard":
                    cardInShop = newCard.AddComponent<CardInShop>();
                    cardInShop.Initialize(newCard.GetComponent<CardBase>());
                    newCard.transform.SetParent(courtyardContainer, false);
                    newCard.tag = "ShopCard";
                    break;
                default:
                    Debug.LogError("Location not found");
                    break;
            }

            newCard.GetComponent<CardBase>().Setup(cardData);
        }

        public void SpawnCardsFromDictionary(Dictionary<int, ICard> cardDictionary, string location)
        {
            if (cardDictionary == null)
            {
                Debug.LogError("Card dictionary is null!");
                return;
            }

            int quantity = cardDictionary.Count;
            Debug.Log($"Spawning {quantity} cards from the dictionary.");

            for (int i = 0; i < quantity; i++)
            {
                ICard cardData = cardDictionary[i];
                SpawnCard(cardData, location);
            }
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