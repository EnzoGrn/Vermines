using System.Collections.Generic;
using UnityEngine;

namespace Vermines.CardSystem.Utilities {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.Core.Scene;

    public class CardLoader {

        private int _NumberOfPlayerToLoad;

        private SceneContext Context;

        private List<CardFamily> _FamiliesToLoad;

        /// <summary>
        /// Function to initialize the card loader.
        /// For that we need every family that will be used in the game.
        /// </summary>
        /// <param name="families">Families used in the current game</param>
        public void Initialize(List<CardFamily> families, SceneContext context)
        {
            Context = context;

            _NumberOfPlayerToLoad = families.Count;
            _FamiliesToLoad       = families;
        }

        /// <summary>
        /// Load the card for the current game.
        /// Don't forget to call 'Initialize' before.
        /// </summary>
        public List<ICard> Load()
        {
            if (_FamiliesToLoad == null || _NumberOfPlayerToLoad == 0)
                return null;
            List<ICard>       cards       = new();
            List<List<ICard>> playerDecks = new();

            for (int p = 0; p < _NumberOfPlayerToLoad; p++)
                playerDecks.Add(new List<ICard>());
            cards = Merge(cards, LoadFromPath("ScriptableObject/Cards/Partisans/Bee"     , playerDecks));
            cards = Merge(cards, LoadFromPath("ScriptableObject/Cards/Partisans/Firefly" , playerDecks));
            cards = Merge(cards, LoadFromPath("ScriptableObject/Cards/Partisans/Mosquito", playerDecks));

            foreach (CardFamily family in _FamiliesToLoad)
                cards = Merge(cards, LoadFamilyFromPath("ScriptableObject/Cards/Partisans/Family", family, playerDecks));
            cards = Merge(cards, LoadFromPath("ScriptableObject/Cards/Items/Tools"    , playerDecks));
            cards = Merge(cards, LoadFromPath("ScriptableObject/Cards/Items/Equipment", playerDecks));

            foreach (List<ICard> deck in playerDecks)
                cards.AddRange(deck);
            return cards;
        }

        #region Loading Functions

        private List<ICard> LoadFromPath(string path, List<List<ICard>> playerDecks)
        {
            if (!System.IO.Directory.Exists($"Assets/Resources/{path}"))
                return null;
            List<ICard> cards = new();
            CardData[] cardDataArray = Resources.LoadAll<CardData>(path);

            if (cardDataArray == null || cardDataArray.Length == 0)
                return null;
            foreach (CardData data in cardDataArray)
                HandleCardData(data, CardFamily.None, null, cards, playerDecks);
            return cards;
        }

        private List<ICard> LoadFamilyFromPath(string path, CardFamily family, List<List<ICard>> playerDecks)
        {
            if (!System.IO.Directory.Exists($"Assets/Resources/{path}"))
                return null;
            List<ICard> cards = new();
            CardData[] cardDataArray = Resources.LoadAll<CardData>(path);

            if (cardDataArray == null || cardDataArray.Length == 0)
                return null;
            foreach (CardData data in cardDataArray) {
                Sprite sprite = Resources.Load<Sprite>($"Sprites/Card/{family.ToString()}/{data.SpriteName}.png");

                HandleCardData(data, family, sprite, cards, playerDecks);
            }

            return cards;
        }

        #endregion

        #region Helpers

        private void HandleCardData(CardData data, CardFamily family, Sprite sprite, List<ICard> shopCards, List<List<ICard>> playerDecks)
        {
            // -- Exemplars
            for (int i = 0; i < data.Exemplars; i++) {
                CardData copy = CreateCopy(data, family, sprite);

                copy.IsStartingCard = false;

                ICard card = SetIdentity(CardFactory.CreateCard(copy));

                shopCards.Add(card);
            }

            // -- Starting Cards
            if (data.IsStartingCard && data.DeckExemplars > 0) {
                for (int p = 0; p < _NumberOfPlayerToLoad; p++) {
                    for (int i = 0; i < data.DeckExemplars; i++) {
                        CardData copy = CreateCopy(data, family, sprite);

                        ICard card = SetIdentity(CardFactory.CreateCard(copy));

                        playerDecks[p].Add(card);
                    }
                }
            }
        }

        private CardData CreateCopy(CardData data, CardFamily family = CardFamily.None, Sprite sprite = null)
        {
            CardData copy = Object.Instantiate(data);

            if (family != CardFamily.None)
                copy.Family = family;
            if (sprite != null)
                copy.Sprite = sprite;
            for (int i = 0; i < copy.Effects.Count; i++) {
                AEffect effect = Object.Instantiate(copy.Effects[i]);

                effect.Initialize(Context, null);

                copy.Effects[i] = effect;
            }

            return copy;
        }

        private ICard SetIdentity(ICard card)
        {
            if (card == null)
                return null;
            card.ID        = CardFactory.CardCount;
            card.IsAnonyme = false;

            foreach (AEffect effect in card.Data.Effects)
                effect.Initialize(Context, card);
            return card;
        }

        private List<ICard> Merge(List<ICard> cards, List<ICard> newCards)
        {
            cards ??= new List<ICard>();

            if (newCards == null)
                return cards;
            cards.AddRange(newCards);

            return cards;
        }

        #endregion
    }
}
