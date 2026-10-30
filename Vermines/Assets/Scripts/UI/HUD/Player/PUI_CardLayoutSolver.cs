using System.Collections.Generic;
using UnityEngine;

namespace Vermines.UI {

    using Vermines.CardSystem.Enumerations;
    using Vermines.CardSystem.Elements;

    /// <summary>
    /// Computes the slot of each card in the mini deck drawer: cards overlap from left to right,
    /// with a small gap between two card types, and the step shrinks when the cards don't fit anymore.
    /// </summary>
    public static class PUI_CardLayoutSolver {

        /// <summary>
        /// Fills <paramref name="positions"/> with the anchored position of each card of <paramref name="sorted"/>.
        /// </summary>
        /// <param name="sorted">Cards already sorted per type.</param>
        /// <param name="positions">Output, one position per card.</param>
        /// <param name="cardWidth">Width of a card view.</param>
        /// <param name="maxWidth">Available width, 0 or less for no limit.</param>
        /// <param name="step">Wanted distance between two cards.</param>
        /// <param name="groupGap">Extra distance between two card types.</param>
        /// <param name="minStep">Smallest step allowed when the cards are compressed.</param>
        public static void Solve(IReadOnlyList<ICard> sorted, List<Vector2> positions, float cardWidth, float maxWidth, float step, float groupGap, float minStep = 2f)
        {
            positions.Clear();

            if (sorted.Count == 0)
                return;
            int groupChanges = 0;

            for (int i = 1; i < sorted.Count; i++) {
                if (TypeOf(sorted[i]) != TypeOf(sorted[i - 1]))
                    groupChanges++;
            }

            float wanted = (sorted.Count - 1) * step + groupChanges * groupGap + cardWidth;

            if (maxWidth > 0f && wanted > maxWidth && sorted.Count > 1)
                step = Mathf.Max(minStep, (maxWidth - cardWidth - groupChanges * groupGap) / (sorted.Count - 1));
            float x = 0f;

            for (int i = 0; i < sorted.Count; i++) {
                if (i > 0) {
                    x += step;

                    if (TypeOf(sorted[i]) != TypeOf(sorted[i - 1]))
                        x += groupGap;
                }
                positions.Add(new Vector2(x, 0f));
            }
        }

        private static CardType TypeOf(ICard card)
        {
            return card?.Data != null ? card.Data.Type : CardType.None;
        }
    }
}
