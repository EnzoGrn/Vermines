using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vermines.CardSystem.Data;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;
using Vermines.Player;
using Vermines.UI;


public class DeckTestController : MonoBehaviour
{
    [SerializeField] private MiniDeckDrawerUI drawer;

    private PlayerDeck _deck;

    private void Start()
    {
        CardSetDatabase.Instance.Initialize(new List<CardFamily>{ CardFamily.Ladybug, CardFamily.Scarab }, null);

        _deck = new PlayerDeck();

        _deck.Initialize(0x0005);

        List<ICard> cards = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data != null);

        _deck.Deck.AddRange(cards.GetRange(0, 5));

        drawer.OnPlayersDeckUpdate(_deck);

        StartCoroutine(TestRoutine());
    }

    private IEnumerator TestRoutine()
    {
        for (int j = 0; j < 15; j++) {
            for (int i = 0; i < 5; i++) {
                AddCard(CardType.Partisan, j * i + i);

                yield return new WaitForSeconds(1.2f);

                AddCard(CardType.Tools, j * i + i);

                yield return new WaitForSeconds(1.2f);
            }

            for (int i = 0; i < 3; i++) {
                RemoveCard(_deck.Deck[Random.Range(0, _deck.Deck.Count)]);

                yield return new WaitForSeconds(1.2f);
            }
        }
    }

    private void AddCard(CardType type, int index)
    {
        List<ICard> cards = CardSetDatabase.Instance.GetEveryCardWith(card => card.Data.Type == type);

        if (cards.Count == 0 || cards[index] == null)
            return;
        _deck.Deck.Add(cards[index]);

        drawer.OnPlayersDeckUpdate(_deck);
    }

    private void RemoveCard(ICard card)
    {
        if (card == null)
            return;
        _deck.Deck.Remove(card);

        drawer.OnPlayersDeckUpdate(_deck);
    }
}
