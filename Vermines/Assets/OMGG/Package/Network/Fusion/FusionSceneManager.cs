using System;
using System.Threading.Tasks;

namespace OMGG.Network.Fusion {

    public class FusionSceneManager : ISceneManager {

        public event Action<string> OnSceneLoaded;
        public event Action<string, string> OnSceneLoadFailed;

        public void LoadScene(string sceneName, bool modeAdditive = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LoadSceneAsync(string sceneName, bool modeAdditive = false)
        {
            throw new NotImplementedException();
        }
    }
}
