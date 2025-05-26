namespace Vermines.UI.Screen
{
    /// <summary>
    /// Interface for screens that can receive a parameter when being shown.
    /// </summary>
    /// <typeparam name="T">The type of the parameter to pass to the screen.</typeparam>
    public interface IParamReceiver<T>
    {
        /// <summary>
        /// Called when the screen is shown with a parameter.
        /// </summary>
        /// <param name="param">The parameter to provide to the screen.</param>
        void SetParam(T param);
    }
}