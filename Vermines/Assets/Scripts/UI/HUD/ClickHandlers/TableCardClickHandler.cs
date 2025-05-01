using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.Gameplay.Phases;
using Vermines.UI;
using Vermines.UI.Popup;

public class TableCardClickHandler : ICardClickHandler
{
    private readonly int _slotId;

    public TableCardClickHandler(int slotId)
    {
        _slotId = slotId;
    }

    public void OnCardClicked(ICard card)
    {
        Debug.Log($"[TableCardClickHandler] Card clicked: {card.Data.Name}");

        if (PhaseManager.Instance.CurrentPhase == Vermines.Gameplay.Phases.Enumerations.PhaseType.Sacrifice)
        {
            OpenConfirmationDialog(card);
        }
    }

    private void OpenConfirmationDialog(ICard card)
    {
        PopupManager.Instance.ShowConfirm(
            title: "Sacrifier ce partisan ?",
            message: $"Souhaitez-vous sacrifier {card.Data.Name} ?",
            onConfirm: () =>
            {
                GameEvents.OnCardSacrificedRequested.Invoke(card);
                PopupManager.Instance.CloseCurrentPopup();
            },
            onCancel: () =>
            {
                Debug.Log("[TableCardClickHandler] Sacrifice cancelled.");
                PopupManager.Instance.CloseCurrentPopup();
            }
        );

        /* TODO: Uncomment and implement localization
        string title = LocalizationManager.Instance.Get("popup.sacrifice.title");
        string message = LocalizationManager.Instance.Get("popup.sacrifice.message", card.Data.Name);

        PopupManager.Instance.ShowConfirm(
            title: title,
            message: message,
            onConfirm: () =>
            {
                GameEvents.OnCardSacrificedRequested.Invoke(card);
                DisableSacrificeMode();
                PopupManager.Instance.CloseCurrentPopup();
            },
            onCancel: () =>
            {
                Debug.Log("[TableCardClickHandler] Sacrifice cancelled.");
                PopupManager.Instance.CloseCurrentPopup();
            }
        );
        */
    }
}
