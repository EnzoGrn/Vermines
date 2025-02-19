using UnityEngine;
using System.Collections.Generic;

namespace Vermines.HUD.Card
{
    using Vermines.CardSystem.Data;

    public class CardSpawner : MonoBehaviour
    {
        public GameObject cardPrefab;
        public Transform handContainer, marketContainer, courtyardContainer, tableContainer;

        [Header("Debug")]
        public bool debugMode = false;
        public List<CardData> debugCardList;
        public Dictionary<int, CardData> debugCardDictionary;

        void Start()
        {
            if (cardPrefab == null)
            {
                Debug.LogError("Card prefab is null!");
                return;
            }

            if (debugMode)
            {
                debugCardDictionary = new Dictionary<int, CardData>
                {
                    { 0, debugCardList[0] },
                    { 1, debugCardList[1] },
                    { 2, debugCardList[2] },
                    { 3, debugCardList[3] },
                    { 4, debugCardList[4] }
                };
                SpawnCardsFromDictionary(debugCardDictionary, "Market");
                SpawnCardsFromDictionary(debugCardDictionary, "Courtyard");
            }
        }

        public void SpawnCard(CardData cardData, string location)
        {
            if (cardData == null)
            {
                Debug.LogError("Card data is null!");
                return;
            }

            GameObject newCard = Instantiate(cardPrefab);

            switch (location)
            {
                case "Market":
                    newCard.AddComponent<CardInShop>();
                    newCard.GetComponent<CardInShop>().SetCardBase(newCard.GetComponent<CardBase>());
                    newCard.transform.SetParent(marketContainer, false);
                    newCard.tag = "ShopCard";
                    break;
                case "Courtyard":
                    newCard.AddComponent<CardInShop>();
                    newCard.GetComponent<CardInShop>().SetCardBase(newCard.GetComponent<CardBase>());
                    newCard.transform.SetParent(courtyardContainer, false);
                    newCard.tag = "ShopCard";
                    break;
                default:
                    Debug.LogError("Location not found");
                    break;
            }

            newCard.GetComponent<CardBase>().Setup(cardData);
        }

        public void SpawnCardsFromDictionary(Dictionary<int, CardData> cardDictionary, string location)
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
                CardData cardData = cardDictionary[i];
                SpawnCard(cardData, location);
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