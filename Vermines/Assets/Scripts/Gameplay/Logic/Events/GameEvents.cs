using System;
using UnityEngine;
using UnityEngine.Events;

public static class GameEvents
{
    public static UnityEvent OnAttemptNextPhase = new UnityEvent();

    public static void AttemptNextPhase()
    {
        OnAttemptNextPhase.Invoke();
        return;
    }
}
