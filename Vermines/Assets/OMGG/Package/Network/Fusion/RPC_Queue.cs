using System.Collections.Generic;
using System;

namespace OMGG.Network.Fusion {

    /// <summary>
    /// Class for a waiting queue processing during RPC execution.
    /// </summary>
    /// <note>
    /// This class got created due to an override of value in same time.
    /// </note>
    public class NetworkQueue {

        private readonly Queue<Action> _RPCQueue = new();

        private bool _IsProcessing = false;

        public void EnqueueRPC(Action rpcAction)
        {
            _RPCQueue.Enqueue(rpcAction);

            // If there is no RPC function processing, start processing
            if (!_IsProcessing)
                ProcessNextRPC();
        }

        public void ProcessNextRPC()
        {
            if (_RPCQueue.Count > 0) {
                _IsProcessing = true;

                Action rpc = _RPCQueue.Dequeue();

                // Process the action
                rpc.Invoke();

                _IsProcessing = false;

                ProcessNextRPC();
            }
        }
    }
}
