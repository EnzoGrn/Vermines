namespace Vermines.UI.Plugin
{

    public partial class NavButtonPlugin : GameplayScreenPlugin
    {
        #region Events

        /// <summary>
        /// Is called when the courtyard button is clicked.
        /// </summary>
        protected virtual void OnOpenCourtyard()
        {
            TryNavigate(camManager => camManager.GoOnCourtyardLocation());
        }

        #endregion
    }
}
