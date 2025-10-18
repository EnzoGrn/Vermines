namespace Vermines.Core.Settings {

    using Vermines.Core.Settings;

    public class RuntimeSettings {

        #region Attributes

        public Options Options => _Options;

        private Options _Options = new();

        #endregion

        #region Methods

        public void Initialize(GlobalSettings settings)
        {

        }

        #endregion
    }
}
