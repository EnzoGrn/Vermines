using System.Runtime.Serialization;
using Fusion;

namespace Vermines.Core {

    using Vermines.Core.Scene;

    public interface IContextBehaviour {

        SceneContext Context { get; set; }
    }

    public abstract class ContextBehaviour : NetworkBehaviour, IContextBehaviour {

        [IgnoreDataMember]
        public SceneContext Context { get; set; }
    }

    public abstract class ContextTRSPBehaviour : NetworkTRSP, IContextBehaviour {

        public SceneContext Context { get; set; }
    }
}
