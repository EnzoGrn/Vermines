using Vermines.CardSystem.Elements;
using Vermines.ShopSystem.Data;
using Vermines.UI.Screen;
using Vermines;
using System.Collections.Generic;

public class CopyEffectToolsPlugin : CopyEffectPlugin {

    public override List<Vermines.UI.Screen.ShopCardEntry> GetEntries()
    {
        MarketSection market = (MarketSection)GameDataStorage.Instance.Shop.Sections[Vermines.ShopSystem.Enumerations.ShopType.Market];
        
        foreach (ICard card in market) {
            if (card.Data.Type == CardTypeTrigger && card.ID != activatedCard.ID)
                currentEntries.Add(new ShopCardEntry(card));
        }

        return currentEntries;
    }
}
