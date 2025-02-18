using Fusion;
using UnityEngine;
using Vermines;

public class Test : NetworkBehaviour
{
    public void TestToCallRPC()
    {
        Debug.Log("Test call RPC");
        RPC_SetEloquence(Runner.LocalPlayer, 10);
    }

    public void EndPhase()
    {
        // Notify the PhaseManager that the phase is completed
        //OnEndPhase.Invoke();
        GameEvents.OnAttemptNextPhase.Invoke();

        //GameEvents.AttemptNextPhase();

        Debug.Log($"End of the TEST OnEndPhase.");
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
    public void RPC_SetEloquence(PlayerRef playerRef, int eloquence)
    {
        Debug.LogWarning($"RPC_SetEloquence Received is runner.IsServer {Runner.IsServer} by {playerRef}");
        GameDataStorage.Instance.SetEloquence(Runner.LocalPlayer, eloquence);
        EndPhase();
    }
}
