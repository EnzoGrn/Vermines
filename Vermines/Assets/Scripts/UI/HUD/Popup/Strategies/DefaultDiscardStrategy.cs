using Vermines.Gameplay.Phases;
using Vermines.Player;
using Vermines.UI.Card;

public class DefaultDiscardStrategy : IDiscardPopupStrategy
{
    public string GetTitle() => "";
    public string GetMessage() =>
        "You have some cards left. Do you want to discard them without activating their effect?";
    public string GetConfirmText() => "Yes";
    public string GetCancelText() => "No";

    public void OnConfirm()
    {
        HandManager.Instance.DiscardAllCards();
        PhaseManager.Instance.Phases[PhaseManager.Instance.CurrentPhase].OnPhaseEnding(PlayerController.Local.PlayerRef, false);
    }

    public void OnCancel()
    {
        // Nothing
    }
}
