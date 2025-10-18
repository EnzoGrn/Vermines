using Fusion;
using System.Collections;
using UnityEngine;

namespace Vermines.Core.Scene {

    public class Scene : CoreBehaviour {

        public bool ContextReady { get; private set; }
        public SceneContext Context => _Context;

        [SerializeField]
        private SceneContext _Context;

        public void Initialize()
        {
            
        }

        public void Deinitialize()
        {

        }

        public void PrepareContext()
        {

        }

        public IEnumerator Activate()
        {
            yield return null;
        }
    }
}
