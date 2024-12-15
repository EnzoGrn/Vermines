using Fusion.Menu;
using UnityEngine;

namespace Vermines {

    public abstract class MenuConnectionPlugin : MonoBehaviour {

        public abstract IFusionMenuConnection Create(MenuConnectionBehaviour connectionBehaviour);
    }
}
