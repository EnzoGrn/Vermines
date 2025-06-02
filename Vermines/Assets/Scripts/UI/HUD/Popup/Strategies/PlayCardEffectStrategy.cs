using UnityEngine;
using Vermines.Player;
using UnityEngine.Localization;
using Vermines.CardSystem.Data.Effect;
using Vermines.CardSystem.Elements;

public class PlayCardEffectStrategy : IDiscardPopupStrategy
{
    private readonly AEffect effect;
    private readonly ICard card = null; // Assuming you have a reference to the card that is being played

    public PlayCardEffectStrategy(AEffect effect, ICard card)
    {
        this.effect = effect;
        this.card = card;
    }

    public string GetTitle() =>
        new LocalizedString("PopupTable", "action.title").GetLocalizedString();

    public string GetMessage()
    {
        var text = effect.Description; // ou localisation, ou autre méthode pour obtenir le message
        if (text.EndsWith("."))
        {
            text = text.Substring(0, text.Length - 1) + "?";
        }
        return text;
    }

    public string GetConfirmText() => "Yes"; // ou localisation

    public string GetCancelText() => "Cancel"; // ou localisation

    public void OnConfirm()
    {
        card.HasBeenActivatedThisTurn = true;
        effect.Play(PlayerController.Local.PlayerRef);
    }

    public void OnCancel()
    {
        Debug.Log("[PlayCardEffectStrategy] Action cancelled.");
    }
}
