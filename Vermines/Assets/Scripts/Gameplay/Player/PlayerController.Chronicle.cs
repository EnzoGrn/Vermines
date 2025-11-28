using OMGG.Chronicle;
using System.Linq;
using UnityEngine;
using Fusion;

namespace Vermines.Player {

    using Vermines.Core;
    using Vermines.Core.Player;
    using Vermines.Gameplay.Chronicle;

    public partial class PlayerController : ContextBehaviour, IPlayer {

        public void AddChronicle(ChronicleEntry entry)
        {
            Context.GameplayMode.Announcer.AddEvent(entry);

            AskPayload(entry.Id);
        }

        private void AskPayload(string entryId)
        {
            /// We should add this part of the code to prevent request spam,
            /// but the problem is that non-host clients are unable to send the RPC (no error message),
            /// it just disappears and the backend is unable to receive it...
            /*if (ChroniclePayloadStorage.TryGet(entryId, out string payload)) {
                ReceivePayload(entryId, payload);

                return;
            }*/

            Context.GameplayMode.RPC_AskPayload(Object.InputAuthority.RawEncoded, entryId);
        }

        private void ReceivePayload(string id, string payload)
        {
            ChronicleEntry entry = Context.GameplayMode.Announcer.GetHistory().FirstOrDefault(e => e.Id == id);

            if (entry == null) {
                Debug.LogWarning($"[CLIENT]: Chronicle entry with ID {id} not found. Cannot receive payload.");

                return;
            }

            if (entry.PayloadJson != null) // Already has payload
                return;
            entry.DescriptionArgs = ChroniclePayloadHelper.GetDescriptionArgs(payload);
            entry.PayloadJson     = payload;

            Context.GameplayMode.Announcer.UpdateEvent(entry);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ReceivePayload(string id, string payload)
        {
            ChroniclePayloadStorage.Add(id, payload);

            ReceivePayload(id, payload);
        }
    }
}
