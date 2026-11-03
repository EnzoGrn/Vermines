namespace Vermines.UI {

    using Vermines.Player;

    /// <summary>
    /// Banner of the player who plays on this screen (never another player).
    /// </summary>
    /// <remarks>
    /// Same display as <see cref="PUI_PlayerBanner"/>, bound automatically to <see cref="PlayerController.Local"/>.
    /// </remarks>
    public class PUI_LocalPlayerBanner : PUI_PlayerBanner {

        protected override PlayerController GetAutoBindTarget()
        {
            return PlayerController.Local;
        }
    }
}
