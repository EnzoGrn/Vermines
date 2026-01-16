using System.Collections.Generic;
using UnityEngine;

namespace Vermines.UI {

    using Vermines.CardSystem.Elements;

    public static class CardLayoutSolver {

        public static void ApplyLayout(List<ICard> sorted, Dictionary<int, PUI_CardView> views)
        {
            for (int i = 0; i < sorted.Count; i++) {
                var card = sorted[i];
                var view = views[card.ID];
                var rt   = view.GetComponent<RectTransform>();

                rt.SetSiblingIndex(i);
            }
        }
    }
}
