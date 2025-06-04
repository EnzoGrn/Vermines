using UnityEngine;
using Vermines.UI;
using Vermines.UI.Screen;

public class CopyContext : IUIContext
{
    /// <summary>
    /// The type of card to be removed.
    /// </summary>
    protected CardCopyEffectContext _cardContext;

    public CopyContext(CardCopyEffectContext cardContext)
    {
        _cardContext = cardContext;

    }
    public void Enter()
    {
        var gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
        if (gameplayUIController == null) return;

        if (gameplayUIController.GetActiveScreen(out var lastScreen))
        {
            gameplayUIController.ShowWithParams<GameplayUICopyEffect, CardCopyEffectContext>(_cardContext, lastScreen);
        }
    }

    public void Exit()
    {
        var gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
        if (gameplayUIController == null) return;
        gameplayUIController.Hide();
    }

    public string GetName()
    {
        return "Copy Card Context";
    }
}