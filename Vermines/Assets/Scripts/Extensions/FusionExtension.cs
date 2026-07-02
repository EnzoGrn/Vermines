using System.Reflection;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vermines.Extension
{

    public static class FusionExtension
    {

        #region Reflection bindings

        private static readonly FieldInfo _SimulationFieldInfo =
            typeof(NetworkRunner).GetField("_simulation", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo _PlayerFieldInfo;
        private static bool _PlayerFieldResolved;

        private static bool _WarnedOnce;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ValidateReflectionBindings()
        {
            if (_SimulationFieldInfo == null)
            {
                Debug.LogError(
                    "[FusionExtension] Binding réflexion '_simulation' introuvable sur NetworkRunner. " +
                    "L'API interne de Fusion a probablement changé après une mise à jour : " +
                    "SetLocalPlayer ne fonctionnera plus (exceptions proxy possibles après déconnexion). " +
                    "À corriger / signaler à Photon.");
            }
        }

        private static void WarnOnce(string reason)
        {
            if (_WarnedOnce)
                return;
            _WarnedOnce = true;

            Debug.LogWarning($"[FusionExtension] SetLocalPlayer inactif : {reason}. " +
                             "L'API interne de Fusion a peut-être changé.");
        }

        #endregion

        #region Extensions

        public static void SetLocalPlayer(this NetworkRunner runner, PlayerRef playerRef)
        {
            if (runner == null)
                return;
            if (_SimulationFieldInfo == null)
            {
                WarnOnce("FieldInfo '_simulation' indisponible");

                return;
            }

            try
            {
                Simulation simulation = (Simulation)_SimulationFieldInfo.GetValue(runner);

                if (simulation == null)
                    return;

                var simType = simulation.GetType();

                if (!simType.FullName.EndsWith("Client"))
                    return;

                if (!_PlayerFieldResolved)
                {
                    _PlayerFieldInfo = simType.GetField("_player", BindingFlags.Instance | BindingFlags.NonPublic);
                    _PlayerFieldResolved = true;

                    if (_PlayerFieldInfo == null)
                        WarnOnce("champ '_player' introuvable sur la simulation Client");
                }

                _PlayerFieldInfo?.SetValue(simulation, playerRef);
            }
            catch (System.Exception ex)
            {
                // Ce hack ne doit JAMAIS casser le flux de déconnexion.
                WarnOnce($"exception ({ex.GetType().Name}: {ex.Message})");
            }
        }

        public static void MoveToRunnerSceneExtended(this NetworkRunner runner, GameObject gameObject)
        {
            if (gameObject.scene == runner.SimulationUnityScene)
                return;
            if (runner.Config.PeerMode != NetworkProjectConfig.PeerModes.Single)
                runner.AddVisibilityNodes(gameObject);
            SceneManager.MoveGameObjectToScene(gameObject, runner.SimulationUnityScene);
        }

        public static void MoveToRunnerSceneExtended(this NetworkRunner runner, Component component)
        {
            if (component == null)
                return;
            runner.MoveToRunnerSceneExtended(component.gameObject);
        }

        #endregion
    }
}
