using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading.Tasks;

public static class SceneUtils {

    public static async Task SafeUnload(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);

        if (scene.isLoaded)
            await SceneManager.UnloadSceneAsync(sceneName);
    }

    public static async Task SafeUnloadAll(params string[] sceneNames)
    {
        foreach (var scene in sceneNames)
            await SafeUnload(scene);
    }
}
