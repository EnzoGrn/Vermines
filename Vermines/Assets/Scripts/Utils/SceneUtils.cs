using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading.Tasks;
using Fusion;

public static class SceneUtils {

    public static async Task SafeUnload(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);

        if (scene.isLoaded)
            await SceneManager.UnloadSceneAsync(sceneName);
    }

    public static async Task SafeUnload(NetworkRunner runner, string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);

        if (scene.isLoaded) {
            int buildIndex = scene.buildIndex;

            if (buildIndex >= 0) {
                var sceneRef = SceneRef.FromIndex(buildIndex);

                if (runner != null && runner.IsRunning) {
                    var op = runner.UnloadScene(sceneRef);

                    await op;
                } else {
                    await SceneManager.UnloadSceneAsync(sceneName);
                }
            } else {
                await SceneManager.UnloadSceneAsync(sceneName);
            }
        }
    }

    public static async Task SafeUnloadAll(params string[] sceneNames)
    {
        foreach (var scene in sceneNames)
            await SafeUnload(scene);
    }

    public static async Task SafeUnloadAll(NetworkRunner runner, params string[] sceneNames)
    {
        foreach (var scene in sceneNames)
            await SafeUnload(runner, scene);
    }
}
