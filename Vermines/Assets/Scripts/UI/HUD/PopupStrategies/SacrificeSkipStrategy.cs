public class SacrificeSkipStrategy : IDiscardPopupStrategy
{
    public string GetTitle() => "End of Sacrifice Phase";
    public string GetMessage() =>
        "Are you sure you want to end your sacrifice phase without selecting any card?";

    public void OnConfirm()
    {
        GameEvents.OnAttemptNextPhase.Invoke(); // or something more specific
    }

    public void OnCancel()
    {
        // Nothing
    }
}
