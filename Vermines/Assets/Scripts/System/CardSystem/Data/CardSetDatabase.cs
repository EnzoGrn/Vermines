using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using OMGG.DesignPattern;
using UnityEngine;

namespace Vermines.CardSystem.Data {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Utilities;
    using Vermines.Core.Scene;

    /// <summary>
    /// Singleton database, that store in game card set.
    /// Card set represent every card playable in this game.
    /// </summary>
    public class CardSetDatabase : Singleton<CardSetDatabase> {

        /// <summary>
        /// Set of card for this game.
        /// </summary>
        private List<ICard> _Cards;

        /// <summary>
        /// Function to initialize the card set database.
        /// For that we need every family that will be used in the game.
        /// </summary>
        /// <param name="families">Families used in the current game</param>
        public void Initialize(List<CardFamily> families, SceneContext context)
        {
            CardFactory.Reset();

            CardLoader loader = new();

            loader.Initialize(families, context);

            _Cards = loader.Load();
        }

        /// <summary>
        /// Check if a card with this identifiers exist in the current set of card.
        /// </summary>
        /// <param name="id">The identifiers of the card</param>
        public bool CardExist(int id)
        {
            if (_Cards == null || _Cards.Count == 0)
                return false;
            foreach (ICard card in _Cards) {
                if (card.ID == id)
                    return true;
            }
            return false;
        }

        public bool CardExist(string id)
        {
            if (_Cards == null || _Cards.Count == 0 || string.IsNullOrEmpty(id))
                return false;
            foreach (ICard card in _Cards) {
                if (card.ID.ToString().CompareTo(id) == 0)
                    return true;
            }
            return false;
        }

        public ICard GetCardByID(int id)
        {
            if (!CardExist(id))
                return null;
            return _Cards.Find(x => x.ID == id);
        }

        public ICard GetCardByID(string id)
        {
            if (!CardExist(id))
                return null;
            return _Cards.Find(x => x.ID.ToString() == id);
        }

        /// <summary>
        /// Give a list of ids and return a list of card.
        /// The list must be separated by a comma.
        /// </summary>
        /// <example>
        /// 5,6,15,82,12,63,13
        /// </example>
        /// <param name="ids">The list of cards you want</param>
        /// <returns>The cards list</returns>
        public List<ICard> GetCardByIds(string ids)
        {
            List<ICard> cards = new();

            if (string.IsNullOrWhiteSpace(ids))
                return cards;
            string[] cardsIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (string cardId in cardsIds) {
                ICard card = GetCardByID(cardId);

                if (card != null)
                    cards.Add(card);
            }

            return cards;
        }

        /// <summary>
        /// Give a list of ids and return a list of card.
        /// </summary>
        /// <param name="ids">The list of cards you want</param>
        /// <returns>The cards list</returns>
        public List<ICard> GetCardByIds(int[] ids)
        {
            List<ICard> cards = new();

            foreach (int id in ids) {
                ICard card = GetCardByID(id);

                if (card != null)
                    cards.Add(card);
            }
            return cards;
        }

        /// <summary>
        /// Function that return a list of card that have the filter function validated.
        /// Becare, if a card is anonyme you can't access it, so at beginning every card are not anonyme for access everything
        /// </summary>
        /// <returns>Every card wanted with the request</returns>
        public List<ICard> GetEveryCardWith(Func<ICard, bool> filter = null)
        {
            if (filter == null)
                return _Cards;
            return _Cards.Where(filter).ToList();
        }

        public int Size => _Cards != null ? _Cards.Count : 0;

        /// <summary>
        /// Call this function to reset the database value.
        /// Also clear the singleton.
        /// </summary>
        public new void Reset()
        {
            // To reset the singleton
            base.Reset();

            Clear();
        }

        /// <summary>
        /// Call this function to clear the database value.
        /// </summary>
        public void Clear()
        {
            _Cards.Clear();
        }

        /// <summary>
        /// Print the database.
        /// </summary>
        public void Print()
        {
            StringBuilder log = new();

            log.AppendLine("[SERVER][DEBUG]: Database information:");

            foreach (ICard card in _Cards) {
                if (card == null || card.Data == null)
                    continue;
                string description = string.Empty;

                for (int i = 0; i < card.Data.Effects.Count; i++) {
                    if (card.Data.Effects[i] == null)
                        continue;
                    description += card.Data.Effects[i].Description + (i + 1 == card.Data.Effects.Count ? "" : "\n");
                }

                log.AppendLine($"{card.ID}[{card.Data.Name}]:\n{description}");
            }

            Debug.Log(log.ToString());
        }
    }
}
