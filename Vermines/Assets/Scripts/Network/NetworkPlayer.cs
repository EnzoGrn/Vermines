using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft {

    public static NetworkPlayer Local { get; set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority) {
            Local = this;

            Debug.Log("Spawned as local player.");
        } else {
            Debug.Log("Spawned as remote player.");
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority) {
            Runner.Despawn(Object);
        }
    }
}
