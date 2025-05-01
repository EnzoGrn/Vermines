using System;
using UnityEngine;
using Vermines.CardSystem.Elements;
using Vermines.UI.Card;
using Vermines.UI.GameTable;

public class ForceDiscardContext : IUIContext
{
    private readonly Action<ICard> _onDiscardComplete;
    private bool _hasDiscarded = false;

    public ForceDiscardContext(Action<ICard> onDiscardComplete)
    {
        Debug.Log($"[ForceDiscardContext] Entering discard context");
        _onDiscardComplete = onDiscardComplete;
    }

    public void Enter()
    {
        Debug.Log($"[ForceDiscardContext] Entering discard context");
        if (TableUI.Instance != null)
            TableUI.Instance.SetOnlyDiscardInteractable(true);
        GameEvents.OnCardDiscarded.AddListener(OnCardDiscarded);
    }

    public void Exit()
    {
        Debug.Log($"[ForceDiscardContext] Exiting discard context");
        if (TableUI.Instance != null)
            TableUI.Instance.SetOnlyDiscardInteractable(false);
        GameEvents.OnCardDiscarded.RemoveListener(OnCardDiscarded);
    }

    private void OnCardDiscarded(ICard card)
    {
        if (_hasDiscarded) return;
        _hasDiscarded = true;

        Debug.Log($"[ForceDiscardContext] Discarded card: {card}");

        _onDiscardComplete?.Invoke(card);
        UIContextManager.Instance.ClearContext();
    }
}
