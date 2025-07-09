using OMGG.Menu.Connection.Data;
using System.Threading.Tasks;
using System;
using UnityEngine;
using Fusion;

namespace Vermines.Menu.MPPM {

    using Vermines.Menu.Screen;

    using static UnityEngine.Object;

    [Serializable]
    public class VerminesMPPMCommand : FusionMppmCommand {

        public string AppVersion;
        public string Region;
        public string Session;
        public bool   IsSharedMode;

        public override void Execute()
        {
            var task = ExecuteAsync();

            // trace errors
            task.ContinueWith(t => Debug.LogError(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task ExecuteAsync()
        {
            var tavern = FindAnyObjectByType<VMUI_Tavern>();

            if (!tavern)
                return;
            Assert.Check(tavern.Controller.ConnectionArgs != null);

            Apply(tavern.Controller.ConnectionArgs);

            tavern.Controller.Show<VMUI_Loading>();

            var connectionResult = await tavern.Controller.Connection.ConnectAsync(tavern.Controller.ConnectionArgs, tavern.SceneRef);

            await tavern.Controller.HandleConnectionResult(connectionResult, tavern.Controller);
        }

        public void Apply(ConnectionArgs args)
        {
            args.AppVersion = AppVersion;
            args.Region     = Region;
            args.Session    = Session;
            args.Creating   = false;

            ApplyUser(args);
        }

        void ApplyUser(ConnectionArgs args)
        {
            args.GameMode = IsSharedMode ? GameMode.Shared : GameMode.Client;
        }
    }
}
