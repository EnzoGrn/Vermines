using Vermines.CardSystem.Elements;
using Vermines.ShopSystem.Data;
using Vermines.UI.Screen;
using Vermines;
using System.Collections.Generic;

public class CopyEffectPartisanPlugin : CopyEffectPlugin {

    public override List<Vermines.UI.Screen.ShopCardEntry> GetEntries()
    {
        foreach (var playerDeck in GameDataStorage.Instance.PlayerDeck) {
            foreach (var card in playerDeck.Value.PlayedCards) {
                if (card.Data.Type == CardTypeTrigger && card.ID != activatedCard.ID)
                    currentEntries.Add(new ShopCardEntry(card));
            }
        }

        CourtyardSection courtyard = (CourtyardSection)GameDataStorage.Instance.Shop.Sections[Vermines.ShopSystem.Enumerations.ShopType.Courtyard];

        foreach (ICard card in courtyard) {
            if (card.Data.Type == CardTypeTrigger && card.ID != activatedCard.ID)
                currentEntries.Add(new ShopCardEntry(card));
        }

        return currentEntries;
    }
}
