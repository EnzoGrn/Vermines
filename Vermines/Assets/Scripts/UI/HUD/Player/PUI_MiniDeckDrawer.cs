using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vermines.Player;

namespace Vermines.UI {

    using Vermines.CardSystem.Elements;

    public class MiniDeckDrawerUI : MonoBehaviour {

        #region Attributes

        [SerializeField]
        private PUI_CardView _CardPrefab;

        [SerializeField]
        private RectTransform _Container;

        private readonly Dictionary<int, PUI_CardView> _Views = new();

        private List<ICard> _LastSortedDeck = new();

        #endregion

        public void OnPlayersDeckUpdate(PlayerDeck deck)
        {
            List<ICard> sorted = CardSortUtility.SortDeckPerType(deck.Deck);

            sorted.Reverse();

            var currentIds = new HashSet<int>(sorted.Select(c => c.ID));
            var removedIds = _Views.Keys.Where(id => !currentIds.Contains(id)).ToList();

            foreach (int id in removedIds) {
                var view = _Views[id];

                PUI_CardAnimationController.PlayRemove(view, () => {
                    Destroy(view.gameObject);
                });

                _Views.Remove(id);
            }

            List<ICard> newCards = sorted.Where(c => !_Views.ContainsKey(c.ID)).ToList();

            foreach (var card in newCards) {
                var view = Instantiate(_CardPrefab, _Container);

                view.Bind(card);
                view.SetGhost(true);

                _Views.Add(card.ID, view);

                PUI_CardAnimationController.PlayGhostShake(view);
            }

            CardLayoutSolver.ApplyLayout(sorted, _Views);

            foreach (var card in newCards)
                PUI_CardAnimationController.DelayedPop(_Views[card.ID], 0.8f);
            _LastSortedDeck = sorted;
        }
    }
}
