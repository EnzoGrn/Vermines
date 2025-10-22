using System.Reflection;
using Fusion;

namespace Vermines.Extension {

    public static class FusionExtension {

        private static readonly FieldInfo _SimulationFieldInfo = typeof(NetworkRunner).GetField("_simulation", BindingFlags.Instance | BindingFlags.NonPublic);

        // Hack - Local player is reset back after disconnect, otherwise exceptions are thrown all over the code because Object.HasStateAuthority == true on proxies
        // This shouldn't be harmful as NetworkRunner gets destroyed anyway.
        public static void SetLocalPlayer(this NetworkRunner runner, PlayerRef playerRef)
        {
            Simulation simulation = (Simulation)_SimulationFieldInfo.GetValue(runner);

            if (simulation == null)
                return;
            // Check if the private type name ends with "Client"
            var simType = simulation.GetType();

            if (simType.FullName.EndsWith("Client")) {

                // Access the private field "_player" and set it
                var playerField = simType.GetField("_player", BindingFlags.Instance | BindingFlags.NonPublic);

                playerField?.SetValue(simulation, playerRef);
            }
        }
    }
}
