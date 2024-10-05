using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief The ToolsCard class, his here to be the base class of all tools cards.
 * A tools card can only be a CardType.Tools.
 */
public class ToolsCard : Card {

    public void OnPlay()
    {
        Debug.Log("Tools Card [" + Data.Name + "]: OnPlay");
    }
}
