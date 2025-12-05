using System;
using System.Collections.Generic;
using Vermines.CardSystem.Elements;

public class RecycleClickHandler : ICardClickHandler
{
    private readonly List<ICard> _selectedCards = new();

    public IReadOnlyList<ICard> SelectedCards => _selectedCards;

    public event Action OnSelectionChanged;

    public void OnCardClicked(ICard card)
    {
        if (_selectedCards.Contains(card))
            _selectedCards.Remove(card);
        else
            _selectedCards.Add(card);

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
