using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class PassiveFactory {

    static public PassiveEffect Create(string effectName, List<object> parameters)
    {
        PassiveEffect effect = null;

        switch (effectName) {
            default:
                Debug.Log("Warning: The effect [" + effectName + "] is not recognized, the effect will be ignored.");

                break;
        }

        return effect;
    }
}

static class TurnStartFactory {

    static public TurnStartEffect Create(string effectName, List<object> parameters)
    {
        TurnStartEffect effect = null;

        switch (effectName) {
            default:
                Debug.Log("Warning: The effect [" + effectName + "] is not recognized, the effect will be ignored.");

                break;
        }

        return effect;
    }
}

static class PlayedFactory
{

    static public PlayedEffect Create(string effectName, List<object> parameters)
    {
        PlayedEffect effect = null;

        switch (effectName) {
            default:
                Debug.Log("Warning: The effect [" + effectName + "] is not recognized, the effect will be ignored.");

                break;
        }

        return effect;
    }
}

static class DiscardFactory {

    static public DiscardEffect Create(string effectName, List<object> parameters)
    {
        DiscardEffect effect = null;

        switch (effectName) {
            default:
                Debug.Log("Warning: The effect [" + effectName + "] is not recognized, the effect will be ignored.");

                break;
        }

        return effect;
    }
}

static class SacrificeFactory {

    static public SacrificeEffect Create(string effectName, List<object> parameters)
    {
        SacrificeEffect effect = null;

        switch (effectName) {
            default:
                Debug.Log("Warning: The effect [" + effectName + "] is not recognized, the effect will be ignored.");

                break;
        }

        return effect;
    }
}
