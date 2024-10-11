using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief The PartisanCard class, his here to be the base class of all partisan cards.
 * A partisan card can't be a CardType.Equipment or CardType.Tools.
 */
public class PartisanCard : Card {

    public void PassiveEffect()
    {
        Debug.Log("Partisan Card [" + Data.Name + "]: PassiveEffect");
    }

    public void StartingTurnEffect()
    {
        Debug.Log("Partisan Card [" + Data.Name + "]: StartingTurnEffect");
    }

    public void OnSacrificeEffect()
    {
        Debug.Log("Partisan Card [" + Data.Name + "]: Sacrifice +" + Data.Souls + " souls");
    }

    public void OnDiscard()
    {
        Debug.Log("Partisan Card [" + Data.Name + "]: OnDiscard");
    }
}
