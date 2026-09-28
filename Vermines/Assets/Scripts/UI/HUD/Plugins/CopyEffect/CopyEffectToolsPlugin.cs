using Vermines.CardSystem.Elements;
using Vermines.ShopSystem.Data;
using Vermines.UI.Shop;
using System.Collections.Generic;
using Vermines.Player;

public class CopyEffectToolsPlugin : CopyEffectPlugin {

    public override List<ShopCardEntry> GetEntries()
    {
        MarketSection market = (MarketSection)PlayerController.Local.Context.GameplayMode.Shop.Sections[Vermines.ShopSystem.Enumerations.ShopType.Market];
        
        foreach (ICard card in market) {
            if (card.Data.Type == CardTypeTrigger && card.ID != activatedCard.ID)
                currentEntries.Add(new ShopCardEntry(card));
        }

        return currentEntries;
    }
}
