using Fusion;

namespace OMGG.Menu.Connection.Data {

    using OMGG.Menu.Configuration;
    using System;

    public partial class ConnectionArgs {

        private readonly IConnectionDataProvider _DataProvider;

        public ConnectionArgs(IConnectionDataProvider provider)
        {
            _DataProvider = provider;
        }

        public virtual string Username
        {
            get => _DataProvider.GetUsername();
            set => _DataProvider.SetUsername(value);
        }

        /// <summary>
        /// The session that the client wants to join.
        /// </summary>
        public virtual string Session { get; set; } = null;

        public GameMode? GameMode;

        public virtual string PreferredRegion
        {
            get => _DataProvider.GetPreferredRegion();
            set => _DataProvider.SetPreferredRegion(value);
        }

        /// <summary>
        /// The actual region that the client will connect to.
        /// </summary>
        public virtual string Region { get; set; }

        /// <summary>
        /// The max player allowed in the game session.
        /// </summary>
        public virtual int MaxPlayerCount
        {
            get => _DataProvider.GetMaxPlayerCount();
            set => _DataProvider.SetMaxPlayerCount(value);
        }

        /// <summary>
        /// Toggle to create or join-only game sessions/rooms.
        /// </summary>
        public virtual bool Creating { get; set; } = false;

        public virtual ServerConfig Config { get; set; }

        public string AppVersion;

        /// <summary>
        /// Partial method to expand defaults to SDK variations.
        /// </summary>
        /// <param name="config"></param>
        partial void SetDefaultsUser(ServerConfig config);

        /// <summary>
        /// Make sure that all configuration have a default settings.
        /// </summary>
        /// <param name="config">The menu config.</param>
        public virtual void SetDefaults(ServerConfig config)
        {
            Session    = null;
            Creating   = false;
            Config     = config;
            AppVersion = config.MachineId;

            if (PreferredRegion != null && config.AvailableRegions.Contains(PreferredRegion) == false)
                PreferredRegion = string.Empty;
            if (MaxPlayerCount <= 0 || MaxPlayerCount > config.MaxPlayerCount)
                MaxPlayerCount = config.MaxPlayerCount;
            if (string.IsNullOrEmpty(Username))
                Username = $"Player{config.CodeGenerator.Create(3)}";
            SetDefaultsUser(config);
        }
    }
}
