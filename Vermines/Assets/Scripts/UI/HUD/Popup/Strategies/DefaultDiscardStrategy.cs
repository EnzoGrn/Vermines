using Vermines.UI.Card;

public class DefaultDiscardStrategy : IDiscardPopupStrategy
{
    public string GetTitle() => "";
    public string GetMessage() =>
        "You have some cards left. Do you want to discard them without activating their effect?";

    public void OnConfirm()
    {
        HandManager.Instance.DiscardAllCards();
        GameEvents.OnAttemptNextPhase.Invoke();
    }

    public void OnCancel()
    {
        // Nothing
    }
}
