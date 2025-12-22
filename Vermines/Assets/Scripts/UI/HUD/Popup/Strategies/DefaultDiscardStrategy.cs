using System.Collections.Generic;
using Vermines.Core.Scene;
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
        SceneContext context      = PlayerController.Local.Context;
        PhaseManager phaseManager = context.GameplayMode.PhaseManager;

        phaseManager.Phases.TryGetValue(phaseManager.CurrentPhase, out var phase);

        context.HandManager.DiscardAllCards(phase);

        phaseManager.Phases[phaseManager.CurrentPhase].OnPhaseEnding(context.Runner.LocalPlayer, false);
    }

    public void OnCancel()
    {
        // Nothing
    }
}
