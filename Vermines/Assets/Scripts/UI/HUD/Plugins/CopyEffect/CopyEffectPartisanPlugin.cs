using System.Collections.Generic;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Cards.Effect;
using Vermines.Player;
using Vermines.UI.Shop;

public class CopyEffectPartisanPlugin : CopyEffectPlugin {

    public override List<ShopCardEntry> GetEntries()
    {
        foreach (ICard card in EffectCandidates.ForCopyPartisan(PlayerController.Local.Context, CardTypeTrigger, activatedCard))
            currentEntries.Add(new ShopCardEntry(card));

        return currentEntries;
    }
}
