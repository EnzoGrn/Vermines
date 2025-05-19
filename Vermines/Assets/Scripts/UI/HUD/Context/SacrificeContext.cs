using UnityEngine;

public class SacrificeContext : IUIContext
{
    public void Enter()
    {
        Debug.Log($"[SacrificeContext] Entering sacrifice context");
    }

    public void Exit()
    {
        Debug.Log($"[SacrificeContext] Exiting sacrifice context");
    }
}
