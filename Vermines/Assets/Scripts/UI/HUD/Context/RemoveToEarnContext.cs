using System;
using Vermines.CardSystem.Enumerations;
using Vermines.CardSystem.Elements;
using UnityEngine;
using Vermines.UI.Screen;
using Vermines.UI;

public class RemoveToEarnContext : IUIContext
{
    /// <summary>
    /// The type of card to be removed.
    /// </summary>
    protected CardType _CardType;

    public RemoveToEarnContext(CardType cardType)
    {
        _CardType = cardType;
    }

    public void Enter()
    {
        var gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>();
        if (gameplayUIController == null) return;

        if (gameplayUIController.GetActiveScreen(out var lastScreen))
        {
            gameplayUIController.ShowWithParams<GameplayUISacrifice, CardType>(_CardType, lastScreen);
        }
    }

    public void Exit()
    {
    }

    public string GetName()
    {
        return "Choose a card of type " + _CardType.ToString() + " to remove";
    }
}
