using OMGG.Chronicle;
using System.Linq;
using UnityEngine;
using Fusion;

namespace Vermines.Player {

    using Vermines.Gameplay.Chronicle;

    public partial class PlayerController : NetworkBehaviour {

        public void AddChronicle(ChronicleEntry entry)
        {
            GameManager manager = FindFirstObjectByType<GameManager>();

            if (!manager) {
                Debug.LogError("[CLIENT]: GameManager not found. Cannot add event to chronicle.");

                return;
            }

            manager.ChronicleManager.AddEvent(entry);

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

            GameManager.Instance.RPC_AskPayload(entryId);
        }

        private void ReceivePayload(string id, string payload)
        {
            GameManager manager = FindFirstObjectByType<GameManager>();

            if (!manager) {
                Debug.LogError("[CLIENT]: GameManager not found. Cannot receive event payload.");

                return;
            }

            ChronicleEntry entry = manager.ChronicleManager.GetHistory().FirstOrDefault(e => e.Id == id);

            if (entry == null) {
                Debug.LogWarning($"[CLIENT]: Chronicle entry with ID {id} not found. Cannot receive payload.");

                return;
            }

            if (entry.PayloadJson != null) // Already has payload
                return;
            entry.DescriptionArgs = ChroniclePayloadHelper.GetDescriptionArgs(payload);
            entry.PayloadJson     = payload;

            manager.ChronicleManager.UpdateEvent(entry);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ReceivePayload(string id, string payload)
        {
            ChroniclePayloadStorage.Add(id, payload);

            ReceivePayload(id, payload);
        }
    }
}
