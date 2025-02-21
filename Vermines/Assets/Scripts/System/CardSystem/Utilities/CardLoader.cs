using System.Collections.Generic;
using UnityEngine;

namespace Vermines.CardSystem.Utilities {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;

    public class CardLoader {

        private int _NumberOfPlayerToLoad;

        private List<CardFamily> _FamiliesToLoad;

        /// <summary>
        /// Function to initialize the card loader.
        /// For that we need every family that will be used in the game.
        /// </summary>
        /// <param name="families">Families used in the current game</param>
        public void Initialize(List<CardFamily> families)
        {
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
            List<ICard> cards = null;
            
            for (int i = 0; i < _NumberOfPlayerToLoad; i++)
                cards = Merge(cards, LoadFromPath("ScriptableObject/Cards/StartingCards"));
            cards = Merge(cards, LoadFromPath("ScriptableObject/Cards/Partisans/Bee"));
            cards = Merge(cards, LoadFromPath("ScriptableObject/Cards/Partisans/Firefly"));
            cards = Merge(cards, LoadFromPath("ScriptableObject/Cards/Partisans/Mosquito"));

            foreach (CardFamily family in _FamiliesToLoad)
                cards = Merge(cards, LoadFamilyFromPath("ScriptableObject/Cards/Partisans/Family", family));
            cards = Merge(cards, LoadFromPath("ScriptableObject/Cards/Items/Tools"));
            cards = Merge(cards, LoadFromPath("ScriptableObject/Cards/Items/Equipment"));

            return cards;
        }

        private List<ICard> LoadFromPath(string path)
        {
            List<ICard> cards = new();
            CardData[] cardDataArray = Resources.LoadAll<CardData>(path);

            if (cardDataArray == null || cardDataArray.Length == 0)
                return null;
            foreach (CardData data in cardDataArray) {
                int nbExemplar = data.Exemplars;

                for (int i = 0; i < nbExemplar; i++) {
                    CardData copy = CreateCopy(data);
                    ICard    card = CardFactory.CreateCard(copy);

                    card = SetIdentity(card);

                    cards.Add(card);
                }
            }
            return cards;
        }

        private List<ICard> LoadFamilyFromPath(string path, CardFamily family)
        {
            List<ICard> cards = new();
            CardData[] cardDataArray = Resources.LoadAll<CardData>(path);

            if (cardDataArray == null || cardDataArray.Length == 0)
                return null;
            foreach (CardData data in cardDataArray) {
                int nbExemplar = data.Exemplars;
                Sprite  sprite = Resources.Load<Sprite>($"Sprites/Card/{family.ToString()}/{data.SpriteName}.png");

                for (int i = 0; i < nbExemplar; i++) {
                    CardData copy = CreateCopy(data, family, sprite);
                    ICard card    = CardFactory.CreateCard(copy);

                    card = SetIdentity(card);

                    cards.Add(card);
                }
            }
            return cards;
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

                effect.Initialize(null);
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
                effect.Initialize(card);
            return card;
        }

        private List<ICard> Merge(List<ICard> cards, List<ICard> newCards)
        {
            cards ??= new List<ICard>();

            foreach (ICard card in newCards)
                cards.Add(card);
            return cards;
        }
    }
}
