namespace Vermines.Gameplay.Errors {

    /// <summary>
    /// Locate the error, to help UI to display it properly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the location is Shop, the error will be displayed in the shop area.
    /// Because the error is related to an action that is performed in the shop.
    /// </para>
    /// 
    /// <para>
    /// Add other locations as needed, it can be a phase, an action, an area of the map...
    /// </para>
    /// </remarks>
    public enum ErrorLocation {
        None,
        Shop,
        Table,
        Sacrifice,
        Discard,
        Effect
        // ...
    }
}
