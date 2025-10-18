using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine;

namespace Vermines.Core {

    using Vermines.Utils;
    using Vermines.Core.Services;
    using Vermines.Core.Settings;
    using Vermines.Core.Network;
    using UnityEngine.PlayerLoop;
    using UnityEngine.LowLevel;

    /// <summary>
    /// The script defines a static entry point (Global) used throughout the project to access the main services, much like a Service Locator.
    /// It's therefore a centralised front end for all critical systems in the game.
    /// </summary>
    /// <example>
    /// Global.Networking.StartGame(request);
    /// </example>
    public static class Global {

        #region Global Attributes

        public static GlobalSettings Settings { get; private set; }
        public static RuntimeSettings RuntimeSettings { get; private set; }

        public static Networking Networking { get; private set; }
        public static PlayerService PlayerService { get; private set; }

        #endregion

        #region Attributes

        private static bool _IsInitialized;
        private static List<IGlobalService> _GlobalServices = new();

        #endregion

        #region Methods

        #region Initialization

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            Initialize();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeAfterSceneLoad() {}

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeSubsystem()
        {
            if (Application.isBatchMode) {
                UnityEngine.AudioListener.volume = 0.0f;

                PlayerLoopUtility.RemovePlayerLoopSystems(typeof(PostLateUpdate.UpdateAudio));
            }

            #if UNITY_EDITOR
                if (!Application.isPlaying)
                    return;
            #endif

            if (!PlayerLoopUtility.HasPlayerLoopSystem(typeof(Global)))
                PlayerLoopUtility.AddPlayerLoopSystems(typeof(Global), typeof(Update.ScriptRunBehaviourUpdate), BeforeUpdate, AfterUpdate);
            Application.quitting -= OnApplicationQuit;
            Application.quitting += OnApplicationQuit;

            _IsInitialized = true;
        }

        private static void Initialize()
        {
            if (!_IsInitialized)
                return;
            if (typeof(DebugManager).GetField("m_DebugActions", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DebugManager.instance) == null) {
                typeof(DebugManager).GetMethod("RegisterInputs" , BindingFlags.Instance | BindingFlags.NonPublic).Invoke(DebugManager.instance, null);
                typeof(DebugManager).GetMethod("RegisterActions", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(DebugManager.instance, null);
            }

            GlobalSettings[] globalSettings = Resources.LoadAll<GlobalSettings>("Settings");

            Settings = globalSettings.Length > 0 ? Object.Instantiate(globalSettings[0]) : null;

            RuntimeSettings = new();

            RuntimeSettings.Initialize(Settings);

            PrepareGlobalServices();

            Networking = CreateStaticObject<Networking>();

            _IsInitialized = true;
        }

        private static void PrepareGlobalServices()
        {
            PlayerService = new();

            _GlobalServices.Add(PlayerService);

            for (int i = 0; i < _GlobalServices.Count; i++)
                _GlobalServices[i].Initialize();
        }

        private static void Deinitialize()
        {
            if (_IsInitialized == false)
                return;
            for (int i = _GlobalServices.Count - 1; i >= 0; i--) {
                IGlobalService service = _GlobalServices[i];

                service?.Deinitialize();
            }

            _IsInitialized = false;
        }

        #endregion

        public static void Quit()
        {
            Deinitialize();

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        #region Update

        private static void BeforeUpdate()
        {
            for (int i = 0; i < _GlobalServices.Count; i++)
                _GlobalServices[i].Tick();
        }

        private static void AfterUpdate()
        {
            if (!Application.isPlaying)
                PlayerLoopUtility.RemovePlayerLoopSystems(typeof(Global));
        }

        #endregion

        #endregion

        #region Events

        private static void OnApplicationQuit()
        {
            Deinitialize();
        }

        #endregion

        #region Utilities

        private static T CreateStaticObject<T>() where T : Component
        {
            GameObject go = new(typeof(T).Name);

            Object.DontDestroyOnLoad(go);

            return go.AddComponent<T>();
        }

        #endregion
    }
}
