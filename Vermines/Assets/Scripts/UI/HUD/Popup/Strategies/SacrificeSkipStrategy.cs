using Vermines.Gameplay.Phases;
using Vermines.Player;

public class SacrificeSkipStrategy : IDiscardPopupStrategy
{
    public string GetTitle() => "End of Sacrifice Phase";
    public string GetMessage() =>
        "Are you sure you want to end your sacrifice phase without selecting any card?";

    public void OnConfirm()
    {
        PhaseManager.Instance.Phases[PhaseManager.Instance.CurrentPhase].OnPhaseEnding(PlayerController.Local.PlayerRef, false);
    }

    public void OnCancel()
    {
        // Nothing
    }
}
