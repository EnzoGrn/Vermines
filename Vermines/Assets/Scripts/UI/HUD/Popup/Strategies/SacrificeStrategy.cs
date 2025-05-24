using UnityEngine;
using UnityEngine.Localization;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Phases;
using Vermines.Player;

public class SacrificeStrategy : IDiscardPopupStrategy
{
    private readonly ICard card;

    public SacrificeStrategy(ICard card)
    {
        this.card = card;
    }

    public string GetTitle() => 
        new LocalizedString("PopupTable", "sacrifice.title").GetLocalizedString();

    public string GetMessage() =>
        new LocalizedString("PopupTable", "sacrifice.message").GetLocalizedString();

    public string GetConfirmText() => "Yes";

    public string GetCancelText() => "No";

    public void OnConfirm()
    {
        GameEvents.OnCardSacrificedRequested.Invoke(card);
    }

    public void OnCancel()
    {
        // Nothing
    }
}