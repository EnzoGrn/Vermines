using UnityEngine;
using UnityEngine.EventSystems;
using Vermines.CardSystem.Elements;
using Vermines.Player;
using Vermines.UI.GameTable;

namespace Vermines.UI.Card
{
    /// <summary>
    /// Handles dropping a card onto a table slot ("playing a card").
    /// Applies an OPTIMISTIC display: the visual change happens immediately on
    /// drop, before server confirmation. If the server refuses, the action is
    /// rolled back visually (see <see cref="OnActionRefused"/>).
    /// </summary>
    public class CardDropHandler : MonoBehaviour, IDropHandler
    {
        protected CardSlotBase slot;

        // Card whose action is currently pending server confirmation/refusal.
        // Null if no action is currently in flight for this slot.
        private ICard _PendingCard;

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            slot = GetComponent<CardSlotBase>();

            // Single, stable subscription: set up once, torn down once.
            // Unlike the previous version, no more dynamic subscription
            // inside OnDrop().
            GameEvents.OnCardPlayedRefused.AddListener(OnActionRefused);
        }

        protected virtual void OnDestroy()
        {
            GameEvents.OnCardPlayedRefused.RemoveListener(OnActionRefused);
        }

        #endregion

        #region Local (read-only) prediction checks

        // LOCAL, non-authoritative check: avoids attempting an action we
        // already know will fail (it's not our turn). The server remains the
        // only real source of truth; this check only exists to give the
        // player instant feedback without a network round-trip.
        protected bool IsMyTurn => PlayerController.Local.Context.GameplayMode.IsMyTurn;

        protected GameObject GetHandDisplay(ICard card)
            => PlayerController.Local.Context.HandManager.GetCardDisplayGO(card);

        #endregion

        #region Drop Handling

        public virtual void OnDrop(PointerEventData eventData)
        {
            DraggableCard drag = eventData.pointerDrag?.GetComponent<DraggableCard>();

            if (drag == null || slot == null)
                return;

            if (!IsMyTurn)
            {
                drag.ReturnToOriginalPosition();

                return;
            }

            ICard card = drag.GetCard();

            if (card == null)
            {
                drag.ReturnToOriginalPosition();

                return;
            }

            if (!slot.CanAcceptCard(card))
            {
                drag.ReturnToOriginalPosition();

                return;
            }

            ApplyOptimistically(card, drag);

            RequestAction(card);
        }

        // Applies the visual change IMMEDIATELY, before any server response.
        // Extension point for subclasses (Discard renders differently: the
        // slot shows the discarded card, not a played one).
        protected virtual void ApplyOptimistically(ICard card, DraggableCard drag)
        {
            GameObject handDisplay = GetHandDisplay(card);

            if (handDisplay != null)
                handDisplay.SetActive(false);

            slot.Init(card, true, new TableCardClickHandler(slot.GetIndex()));

            _PendingCard = card;
        }

        // Sends the request to the server. Extension point (Discard routes
        // through a different mechanism than Play).
        protected virtual void RequestAction(ICard card)
        {
            GameEvents.OnCardPlayedRequested.Invoke(card);
        }

        #endregion

        #region Rollback on refusal

        // Called when the server refuses the action. Rolls back the visual
        // change applied by ApplyOptimistically, restoring the card to hand.
        protected virtual void OnActionRefused(ICard card)
        {
            if (_PendingCard == null || card != _PendingCard)
                return; // not our pending action: ignore

            _PendingCard = null;

            RestoreHandCard(card);
            slot.ResetSlot();
        }

        // Re-displays the card in hand and returns it to its original
        // position. Reused as-is by DiscardDropHandler.
        protected void RestoreHandCard(ICard card)
        {
            GameObject handDisplay = GetHandDisplay(card);

            if (handDisplay == null)
                return;
            handDisplay.SetActive(true);

            if (handDisplay.TryGetComponent<DraggableCard>(out var drag))
            {
                drag.ReturnToOriginalPosition();
                drag.gameObject.SetActive(true);
            }
        }

        #endregion
    }
}
