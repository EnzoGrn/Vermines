using System;
using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.UI.Card;

public class RecycleClickHandler : ICardClickHandler
{
    private readonly List<ICard> _selectedCards = new();
    public IReadOnlyList<ICard> SelectedCards => _selectedCards;
    public event Action OnSelectionChanged;

    public void OnCardClicked(ICard card)
    {
        CardDisplay cardDisplay = (card as MonoBehaviour)?.GetComponent<CardDisplay>();

        if (_selectedCards.Contains(card))
        {
            _selectedCards.Remove(card);
            cardDisplay?.SetSelected(false);
        }
        else
        {
            _selectedCards.Add(card);
            cardDisplay?.SetSelected(true);
        }

        OnSelectionChanged?.Invoke();
    }

    public int GetTotalEloquence()
    {
        int total = 0;
        foreach (var card in _selectedCards)
            total += card.Data.Eloquence;
        return total;
    }

    public int GetTotalSouls()
    {
        int total = 0;
        foreach (var card in _selectedCards)
            total += card.Data.Souls;
        return total;
    }
}