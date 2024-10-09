using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief Interface for all cards.
 */
public interface ICard {

    public CardData Data { get; set; }

    public bool HasCost();

    public void OnPlay();
}

/**
 * @brief The Card class, his here to be the base class of all cards.
 *
 * @note It only contains all the data of a card, like its name, description, type, eloquence, souls, sprite and effects.
 */
public abstract class Card : ICard
{

    private CardData _Data;
    public CardData Data
    {
        get => _Data;
        set
        {
            if (value != null)
                _Data = value;
        }
    }

    public bool HasCost()
    {
        return Data.Eloquence != -1;
    }

    public void OnPlay()
    {
        Debug.Log("Card [" + Data.Name + "]: OnPlay");
    }
}
