using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Phases;
using Vermines.Player;
using Vermines.UI.GameTable;

namespace Vermines.UI.Card
{
    public class DiscardDropHandler : CardDropHandler
    {
        protected override void Awake()
        {
            base.Awake();

            slot = GetComponent<CardSlotBase>();

            GameEvents.OnCardDiscardedRefused.AddListener(OnDiscardActionRefused);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameEvents.OnCardDiscardedRefused.RemoveListener(OnDiscardActionRefused);
        }

        public override void OnDrop(PointerEventData eventData)
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

            if (!card.Data.CanBeDiscard())
            {
                drag.ReturnToOriginalPosition();

                return;
            }

            if (!slot.CanAcceptCard(card))
            {
                drag.ReturnToOriginalPosition();

                return;
            }

            PhaseManager phaseManager = PlayerController.Local.Context.GameplayMode.PhaseManager;

            if (!phaseManager.Phases.TryGetValue(phaseManager.CurrentPhase, out var phase) || phase is not ActionPhaseAsset actionPhase)
            {
                drag.ReturnToOriginalPosition();
                slot.ResetSlot();

                return;
            }

            ApplyOptimistically(card, drag);

            actionPhase.OnDiscard(card);
        }

        // Discard displays the discarded card in the slot (not a "played" one).
        protected override void ApplyOptimistically(ICard card, DraggableCard drag)
        {
            slot.ResetSlot();
            slot.SetCard(card);

            GameObject handDisplay = GetHandDisplay(card);

            if (handDisplay != null)
                handDisplay.SetActive(false);
        }

        // Discard doesn't use RequestAction (routing happens via
        // ActionPhaseAsset.OnDiscard inside OnDrop) -- no override needed,
        // the base method is simply never called for this class.

        // On refusal, Discard restores the PREVIOUSLY discarded card into the
        // slot rather than leaving it empty -- different from the base
        // class's generic rollback.
        private void OnDiscardActionRefused(ICard card)
        {
            RestoreHandCard(card);
            slot.ResetSlot();

            ICard previousCard = PlayerController.Local.Discard.LastOrDefault();

            if (previousCard != null)
                slot.SetCard(previousCard);
        }

        public void SetLatestDiscardedCard(ICard card)
        {
            if (slot == null)
                return;
            slot.ResetSlot();
            slot.SetCard(card);
        }
    }
}
