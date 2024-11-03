using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief Interface for all effects.
 */
public interface IEffect {

    void Apply();
}

/**
 * @brief The PassiveEffect class, will be called by the GameManager, that we always listen to the passive effect of the cards
 * and call the Apply method of the effect.
 *
 * @note If you want to create a new effect that is a PassiveEffect, you need to inherit from this class.
 */
public abstract class PassiveEffect : MonoBehaviour, IEffect {

    public abstract void Apply();
}

/**
 * @brief The TurnStartEffect class, will be called by the GameManager, when the turn start and call the Apply method of the effect.
 *
 * @note If you want to create a new effect that is a TurnStartEffect, you need to inherit from this class.
 */
public abstract class TurnStartEffect : MonoBehaviour, IEffect {

    public abstract void Apply();
}

/**
 * @brief The PlayedEffect class, will be called by the GameManager, when the player play the card and call the Apply method of the effect.
 *
 * @note If you want to create a new effect that is a PlayedEffect, you need to inherit from this class.
 */
public abstract class PlayedEffect : MonoBehaviour, IEffect {

    public abstract void Apply();
}

/**
 * @brief The DiscardEffect class, will be called by the GameManager, when the player discard the card and call the Apply method of the effect.
 *
 * @note If you want to create a new effect that is a DiscardEffect, you need to inherit from this class.
 */
public abstract class DiscardEffect : MonoBehaviour, IEffect {

    public abstract void Apply();
}

/**
 * @brief The SacrificeEffect class, will be called by the GameManager, when the player sacrifice the card and call the Apply method of the effect.
 *
 * @note If you want to create a new effect that is a SacrificeEffect, you need to inherit from this class.
 */
public abstract class SacrificeEffect : MonoBehaviour, IEffect {

    public abstract void Apply();
}
