using System.Threading.Tasks;
using System;

namespace OMGG.Network {

    /*
     * @brief ISceneManager is an interface that represents the management of synchronized scenes.
     * It is used to load and synchronize scenes between players.
     * It also triggers an event when a scene is changed.
     */
    public interface ISceneManager {

        /*
         * @brief Load a scene by its name.
         * @note The additive mode is used to load a scene without unloading the current scene.
         * If you don't want to unload the current scene, set modeAdditive to true.
         * Else, the current scene will be unloaded and the new scene will be loaded. (it's name the Single mode)
         *
         * @overload LoadSceneAsync same as LoadScene but asynchronous.
         */
        void LoadScene(string sceneName, bool modeAdditive = false); /* Maybe add later a sync with client parameters */
        Task<bool> LoadSceneAsync(string sceneName, bool modeAdditive = false); /* Maybe add later a sync with client parameters */

        /*
         * @brief Event that is triggered when a scene is loaded.
         */
        event Action<string> OnSceneLoaded;

        /*
         * @brief Event that is triggered when a scene load failed.
         */
        event Action<string, string> OnSceneLoadFailed;
    }
}
