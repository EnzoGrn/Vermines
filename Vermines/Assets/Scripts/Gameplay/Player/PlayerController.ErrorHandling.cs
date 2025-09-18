using Fusion;

namespace Vermines.Player {

    /// <summary>
    /// 'Error Handling' section of <see cref="PlayerController"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// <para>
    /// This file (<c>PlayerController.ErrorHandling.cs</c>) contains the RPCs triggered from the server (<c>GameManager.Backend.cs</c>)
    /// when it detects an error in a game action (e.g. prohibited action, insufficient resources, invalid target).
    /// </para>
    /// 
    /// <para>
    /// Two types of errors are distinguished:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     <b>Local errors:</b> only transmitted to the player concerned.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     <b>Global errors:</b> transmitted to all players in the game.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    public partial class PlayerController : NetworkBehaviour {

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_LocalError(PlayerRef target)
        {
            if (Object.InputAuthority != target)
                return;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_GlobalError()
        {

        }
    }
}
