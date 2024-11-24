using System;

namespace OMGG.Network.Fusion {

    public class FusionSceneManager : ISceneManager {

        public event Action<string> OnSceneLoaded;

        public void LoadScene(string sceneName, bool modeAdditive = false)
        {
            throw new NotImplementedException();
        }
    }
}
