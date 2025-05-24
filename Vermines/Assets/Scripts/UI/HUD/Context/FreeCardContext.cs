using Vermines.ShopSystem.Enumerations;
using UnityEngine;
using Vermines.UI.Screen;
using Vermines.UI;

public class FreeCardContext : IUIContext
{
    /// <summary>
    /// The shop where the player can buy a card for free.
    /// </summary>
    protected ShopType _shopType;

    public FreeCardContext(ShopType shopType)
    {
        _shopType = shopType;
    }

    public void Enter()
    {
        //var gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
        //if (gameplayUIController == null) return;

        //if (gameplayUIController.GetActiveScreen(out var lastScreen))
        //{
        //    gameplayUIController.ShowWithParams<GameplayUIShop, ShopType>(_shopType, lastScreen);
        //}
    }

    public void Exit()
    {
    }

    public string GetName()
    {
        return "Free Card Context";
    }
}
