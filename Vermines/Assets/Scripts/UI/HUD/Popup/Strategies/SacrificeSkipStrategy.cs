using Vermines.Core.Scene;
using Vermines.Gameplay.Phases;
using Vermines.Player;

public class SacrificeSkipStrategy : IDiscardPopupStrategy
{
    public string GetTitle() => "End of Sacrifice Phase";
    public string GetMessage() =>
        "Are you sure you want to end your sacrifice phase without selecting any card?";
    public string GetConfirmText() => "Yes";
    public string GetCancelText() => "No";

    public void OnConfirm()
    {
        SceneContext context      = PlayerController.Local.Context;
        PhaseManager phaseManager = context.GameplayMode.PhaseManager;

        phaseManager.Phases[phaseManager.CurrentPhase].OnPhaseEnding(context.Runner.LocalPlayer, false);
    }

    public void OnCancel()
    {
        // Nothing
    }
}
