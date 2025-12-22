namespace Vermines.Gameplay.Errors {

    public enum ErrorSeverity {
        Minor,   // If the error has a small impact on gameplay
        Major,   // If the error has a significant impact on gameplay
        Critical // If the error has a severe impact on gameplay
    }
}

/// <summary>
/// Here some examples of how to use the ErrorSeverity enum:
///
/// <list type="number">
///   <item>
///     <description>
///     <b>Minor:</b> A player's try to buy a card but doesn't have enough resources.
///     </description>
///   </item>
///   <item>
///     <description>
///     <b>Major:</b> A player tries to perform an action that is not allowed by the game rules, such as playing a card out of action phase.
///     </description>
///   </item>
///   <item>
///     <description>
///     <b>Critical:</b> A player attempts to perform an action that could compromise the integrity of the game, such as trying to manipulate the game state in an unauthorized way (e.g., modifying their resources directly).
///     </description>
///   </item>
/// </list>
/// </para>
/// </summary>
