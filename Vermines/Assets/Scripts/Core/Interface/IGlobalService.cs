namespace Vermines.Core {

    /// <summary>
    /// Interface implemented by all global services.
    /// This allows the static <see cref="Vermines.Core.Global"/> class to manage their entire lifecycle (init, tick, cleanup).
    /// </summary>
    /// <example>
    /// Example of global services:
    /// - PlayerService
    /// - Networking
    /// - MultiplayManager
    /// </example>
    public interface IGlobalService {

        void Initialize();
        void Deinitialize();

        void Tick();
    }
}
