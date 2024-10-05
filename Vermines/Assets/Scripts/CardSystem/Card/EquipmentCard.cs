using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief The EquipmentCard class, his here to be the base class of all equipment cards.
 * A Equipment card can only be a CardType.Equipment.
 */
public class EquipmentCard : Card {

    public void Passive()
    {
        Debug.Log("Equipment Card [" + Data.Name + "]: Passive");
    }

    public void StartingTurn()
    {
        Debug.Log("Equipment Card [" + Data.Name + "]: StartingTurn");
    }
}
