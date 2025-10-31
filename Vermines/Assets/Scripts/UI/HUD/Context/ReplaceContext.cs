using Vermines.ShopSystem.Enumerations;
using System;
using UnityEngine;
using System.Collections.Generic;
using Vermines.UI;
using Vermines.UI.Screen;

public class ReplaceEffectContext : IUIContext
{
    private readonly Action<Dictionary<ShopType, int>> _onDone;
    public Dictionary<ShopType, int> dictShopSlot;

    public ReplaceEffectContext(Action<Dictionary<ShopType, int>> onDone)
    {
        _onDone = onDone;
    }

    public void Enter()
    {
        GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
        if (gameplayUIController != null)
        {
            gameplayUIController.GetActiveScreen(out GameplayUIScreen lastScreen);
            gameplayUIController.ShowWithParams<GameplayUIReplaceEffect, Action<Dictionary<ShopType, int>>>(_onDone, lastScreen);
        }
    }

    public void Exit()
    {
    }

    public string GetName()
    {
        return "Replace Effect";
    }
}
