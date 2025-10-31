using Fusion;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Phases;
using Vermines.Player;
using Vermines.UI.GameTable;

namespace Vermines.UI.Card
{
    public class DiscardDropHandler : CardDropHandler
    {
        private void Awake()
        {
            // Initialize any necessary components or variables here
            GameEvents.OnCardDiscardedRefused.AddListener(OnDiscardRefused);
            slot = GetComponent<CardSlotBase>();
        }

        private void OnDestroy()
        {
            // Clean up event listeners to avoid memory leaks
            GameEvents.OnCardDiscardedRefused.RemoveListener(OnDiscardRefused);
        }

        public override void OnDrop(PointerEventData eventData)
        {
            DraggableCard drag = eventData.pointerDrag?.GetComponent<DraggableCard>();
            if (drag == null || slot == null) return;

            if (GameManager.Instance.IsMyTurn() == false)
            {
                Debug.Log("[DiscardDropHandler] Not your turn, cannot discard card.");
                drag.ReturnToOriginalPosition();
                return;
            }

            ICard card = drag.GetCard();
            if (card == null)
            {
                Debug.Log("[DiscardDropHandler] Card is null, cannot discard.");
                return;
            }

            Debug.Log($"[DiscardDropHandler] Card {card.Data.Name} discard requested.");

            if (slot.CanAcceptCard(card))
            {
                slot.ResetSlot();
                slot.SetCard(card);
                GameObject go = HandManager.Instance.GetCardDisplayGO(card);
                if (go != null)
                {
                    go.SetActive(false);
                }

                if (PhaseManager.Instance.Phases.TryGetValue(GameManager.Instance.GetCurrentPhase(), out var phase) && phase is ActionPhaseAsset actionPhase)
                {
                    actionPhase.OnDiscard(card);
                }
                else
                {
                    Debug.LogWarning("[DiscardDropHandler] Cannot discard card outside of Action Phase.");
                    drag.ReturnToOriginalPosition();
                    slot.ResetSlot();
                }
            }
            else
            {
                Debug.Log($"[CardDropHandler] Cannot accept card {card.Data.Name} in slot {slot.GetIndex()}");
                drag.ReturnToOriginalPosition();
            }
        }

        private void OnDiscardRefused(ICard card)
        {
            // Handle the discard refusal event here if needed
            Debug.Log($"[DiscardDropHandler] Card {card.Data.Name} discard refused.");
            GameObject go = HandManager.Instance.GetCardDisplayGO(card);
            if (go != null)
            {
                go.SetActive(true);
                if (go.TryGetComponent<DraggableCard>(out var drag))
                {
                    drag.ReturnToOriginalPosition();
                    drag.gameObject.SetActive(true);
                }
            }

            // Reset the slot to the previous card or empty state
            slot.ResetSlot();
            PlayerRef player = GameManager.Instance.Runner.LocalPlayer;
            PlayerDeck deck = GameDataStorage.Instance.PlayerDeck[player];

            ICard previousCard = deck.Discard.LastOrDefault();
            if (previousCard != null)
            {
                Debug.Log($"[DiscardDropHandler] Restoring card {previousCard.Data.Name} to discard slot.");
                slot.SetCard(previousCard);
            }
        }

        public void SetLatestDiscardedCard(ICard card)
        {
            if (slot == null) return;
            slot.ResetSlot();
            slot.SetCard(card);
        }
    }
}
