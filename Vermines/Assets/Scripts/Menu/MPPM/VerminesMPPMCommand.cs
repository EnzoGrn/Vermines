using OMGG.Menu.Connection.Data;
using System.Threading.Tasks;
using System;
using Fusion;
using UnityEngine;

namespace Vermines.Menu.MPPM {

    using Vermines.Menu.Controller;
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
            var controller = FindAnyObjectByType<VMUI_Controller>();

            if (!controller)
                return;
            Assert.Check(controller.ConnectionArgs != null);

            Apply(controller.ConnectionArgs);

            controller.Show<VMUI_Loading>();

            var connectionResult = await controller.Connection.ConnectAsync(controller.ConnectionArgs);

            await controller.HandleConnectionResult(connectionResult, controller);
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
