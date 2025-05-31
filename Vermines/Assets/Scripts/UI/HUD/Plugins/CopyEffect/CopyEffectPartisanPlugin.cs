using System.Collections.Generic;
using Vermines.UI.Screen;
using Vermines;
using UnityEngine;

public class CopyEffectPartisanPlugin : CopyEffectPlugin
{
    public override List<Vermines.UI.Screen.ShopCardEntry> GetEntries()
    {
        foreach (var playerDeck in GameDataStorage.Instance.PlayerDeck)
        {
            foreach (var card in playerDeck.Value.PlayedCards)
            {
                Debug.Log($"Card stored is: {activatedCard.Data.Name} with ID: {activatedCard.ID}");
                Debug.Log($"Card in played cards is: {card.Data.Name} with ID: {card.ID}");
                if (card.Data.Type == CardTypeTrigger &&
                    card.ID != activatedCard.ID) // Ensure we don't include the activated card itself
                {
                    currentEntries.Add(new ShopCardEntry(card));
                }
            }
        }

        foreach (var card in GameDataStorage.Instance.Shop.Sections[Vermines.ShopSystem.Enumerations.ShopType.Courtyard].AvailableCards)
        {
            if (card.Value.Data.Type == CardTypeTrigger &&
                    card.Value.ID != activatedCard.ID) // Ensure we don't include the activated card itself
            {
                currentEntries.Add(new ShopCardEntry(card.Value));
            }
        }

        return currentEntries;
    }
}
