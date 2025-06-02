using System.Collections.Generic;
using Vermines.UI.Screen;
using Vermines;
using UnityEngine;

public class CopyEffectToolsPlugin : CopyEffectPlugin
{
    public override List<Vermines.UI.Screen.ShopCardEntry> GetEntries()
    {
        foreach (var card in GameDataStorage.Instance.Shop.Sections[Vermines.ShopSystem.Enumerations.ShopType.Market].AvailableCards)
        {
            Debug.Log($"Card stored is: {activatedCard.Data.Name} with ID: {activatedCard.ID}");
            Debug.Log($"Card in market is: {card.Value.Data.Name} with ID: {card.Value.ID}");
            if (card.Value.Data.Type == CardTypeTrigger &&
                card.Value.ID != activatedCard.ID) // Ensure we don't include the activated card itself
            {
                currentEntries.Add(new ShopCardEntry(card.Value));
            }
        }

        return currentEntries;
    }
}
