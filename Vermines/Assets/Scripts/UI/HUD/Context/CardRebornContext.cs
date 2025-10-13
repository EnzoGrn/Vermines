using UnityEngine;
using Vermines.UI;
using Vermines.UI.Screen;

public class CardRebornContext : IUIContext {

    protected CardSelectedEffectContext _cardContext;

    public CardRebornContext(CardSelectedEffectContext cardContext)
    {
        _cardContext = cardContext;

    }

    public void Enter()
    {
        var gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();

        if (gameplayUIController == null)
            return;
        if (gameplayUIController.GetActiveScreen(out var lastScreen))
            gameplayUIController.ShowWithParams<GameplayUIRebornEffect, CardSelectedEffectContext>(_cardContext, lastScreen);
    }

    public void Exit()
    {
        var gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();

        if (gameplayUIController == null)
            return;
        gameplayUIController.Hide();
    }

    public string GetName()
    {
        return "Card Reborn Context";
    }
}
