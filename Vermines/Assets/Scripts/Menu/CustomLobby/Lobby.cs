using System.Collections.Generic;
using System.Collections;
using Fusion;

namespace Vermines.Menu.CustomLobby {

    using Vermines.Core;
    using Vermines.Core.Scene;
    using Vermines.Extension;

    public class Lobby : Scene {

        #region Methods

        protected override void OnInitialize()
        {
            base.OnInitialize();

            AddService(Context.UI);

            List<IContextBehaviour> contextBehaviours = Context.Runner.SimulationUnityScene.GetComponents<IContextBehaviour>(true);

            foreach (IContextBehaviour behaviour in contextBehaviours)
                behaviour.Context = Context;
        }

        protected override IEnumerator OnActivate()
        {
            yield return base.OnActivate();
        }

        protected override void OnTick()
        {
            if (Context.Runner != null && Context.Runner.HasVisibilityEnabled())
                Context.Runner.SetVisible(Context.IsVisible);
            base.OnTick();
        }

        #endregion
    }
}
