using UnityEngine;
using Vermines.UI.Screen;
using Vermines.UI;

public class SacrificeContext : IUIContext
{
    public void Enter()
    {
        Debug.Log($"[SacrificeContext] Entering sacrifice context");
        GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
        GameplayUITable tableScreen = null;
        if (gameplayUIController != null)
        {
            tableScreen = gameplayUIController.Get<GameplayUITable>();
            tableScreen.UpdateUIForPhase(Vermines.Gameplay.Phases.Enumerations.PhaseType.Sacrifice);
        }
    }

    public void Exit()
    {
        Debug.Log($"[SacrificeContext] Exiting sacrifice context");
        GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
        GameplayUITable tableScreen = null;
        if (gameplayUIController != null)
        {
            tableScreen = gameplayUIController.Get<GameplayUITable>();
            tableScreen.UpdateUIForPhase(Vermines.Gameplay.Phases.Enumerations.PhaseType.Action);
        }
    }

    public string GetName()
    {
        return "Sacrifice";
    }
}
