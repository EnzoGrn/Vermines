using System;
using UnityEngine;
using Vermines.UI;
using Vermines.UI.Screen;
using Vermines.CardSystem.Enumerations;

public class SpendEffectContext : IUIContext
{
    private int _amountSpent = 0;
    private readonly Action<int> _callback;
    private readonly DataType _dataToSpend;
    private readonly DataType _dataToEarn;
    private readonly int _multiplicator;

    public SpendEffectContext(Action<int> callback, DataType dataToSpend, DataType dataToEarn, int multiplicator)
    {
        _callback = callback;
        _dataToSpend = dataToSpend;
        _dataToEarn = dataToEarn;
        _multiplicator = multiplicator;
    }

    public void Enter()
    {
        GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
        if (gameplayUIController != null)
        {
            gameplayUIController.GetActiveScreen(out GameplayUIScreen lastScreen);
            gameplayUIController.ShowWithParams<GameplayUISpendEffect, (Action<int>, DataType, DataType, int)>(
                (
                    _callback,
                    _dataToSpend,
                    _dataToEarn,
                    _multiplicator
                )
            );
        }
    }

    public void Exit()
    {
    }

    public string GetName()
    {
        return "Spend";
    }
}
