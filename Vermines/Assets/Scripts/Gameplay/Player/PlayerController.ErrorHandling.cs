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
using UnityEngine.Localization.Settings;
using UnityEngine;
using Fusion;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Data;

namespace Vermines.Player {

    using Vermines.Gameplay.Errors;

    public partial class PlayerController : NetworkBehaviour {

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ReceiveError(GameActionError error)
        {
            // Note: Use 'Local.PlayerRef' and not 'Object.InputAuthority'.
            //       Because the GameManager aimed the host PlayerController when he send a RPC.
            //       So if you use 'Object.InputAuthority' will always be the host PlayerRef.
            //       To know if the error is yours, you have to compare with 'Local.PlayerRef' that is the real local player.
            if (error.Scope == ErrorScope.Local && Local.PlayerRef != error.Target)
                return;
            HandleError(error);
        }

        private void HandleError(GameActionError error)
        {
            string localizedMessage = LocalizationSettings.StringDatabase.GetLocalizedString("Back-end Error", error.MessageKey.ToString(), error.MessageArgs.ToArray());

            Debug.LogWarning($"[Error-{error.Scope}] {localizedMessage} (Loc: {error.Location}, Sev: {error.Severity})");

            // TODO: Link to UI notification system

            switch (error.Location)
            {
                case ErrorLocation.Discard:
                    ICard card = CardSetDatabase.Instance.GetCardByID(error.MessageArgs.Arg0.ToString());
                    GameEvents.OnCardDiscardedRefused.Invoke(card);
                    break;
                default:
                    break;
            }

        }
    }
}
