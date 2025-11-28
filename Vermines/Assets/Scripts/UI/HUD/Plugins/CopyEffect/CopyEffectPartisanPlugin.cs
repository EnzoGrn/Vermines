using Vermines.CardSystem.Elements;
using Vermines.ShopSystem.Data;
using Vermines.UI.Screen;
using Vermines;
using Vermines.Player;
using System.Collections.Generic;
using Vermines.Core.Scene;

public class CopyEffectPartisanPlugin : CopyEffectPlugin {

    public override List<Vermines.UI.Screen.ShopCardEntry> GetEntries()
    {
        SceneContext context = PlayerController.Local.Context;

        List<PlayerController> players = context.Runner.GetAllBehaviours<PlayerController>();

        foreach (PlayerController player in players) {
            foreach (ICard card in player.Deck.PlayedCards) {
                if (card.Data.Type == CardTypeTrigger && card.ID != activatedCard.ID)
                    currentEntries.Add(new ShopCardEntry(card));
            }
        }

        CourtyardSection courtyard = (CourtyardSection)context.GameplayMode.Shop.Sections[Vermines.ShopSystem.Enumerations.ShopType.Courtyard];

        foreach (ICard card in courtyard) {
            if (card.Data.Type == CardTypeTrigger && card.ID != activatedCard.ID)
                currentEntries.Add(new ShopCardEntry(card));
        }

        return currentEntries;
    }
}
