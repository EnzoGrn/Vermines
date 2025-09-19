/// <summary>
/// 'Error Handling' section of <see cref="PlayerController"/>.
/// </summary>
/// <remarks>
/// 
/// <para>
/// This file (<c>PlayerController.ErrorHandling.cs</c>) contains the RPCs triggered from the server (<c>GameManager.Backend.cs</c>)
/// when it detects an error in a game action (e.g. prohibited action, insufficient resources, invalid target).
/// </para>
/// </remarks>
using Fusion;
using UnityEngine;

namespace Vermines.Player {

    using Vermines.Gameplay.Errors;

    public partial class PlayerController : NetworkBehaviour {

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ReceiveError(GameActionError error)
        {
            if (error.Scope == ErrorScope.Local && Object.InputAuthority != error.Target)
                return;
            HandleError(error);
        }

        private void HandleError(GameActionError error)
        {
            Debug.LogWarning($"[Error-{error.Scope}] {error.Message} (Loc: {error.Location}, Sev: {error.Severity})");

            // TODO: Link to UI notification system
        }
    }
}
