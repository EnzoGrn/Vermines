using System;

namespace OMGG.Network.Quantum {

    public class QuantumSceneManager : ISceneManager {

        public event Action<string> OnSceneLoaded;

        public void LoadScene(string sceneName, bool modeAdditive = false)
        {
            throw new NotImplementedException();
        }
    }
}
