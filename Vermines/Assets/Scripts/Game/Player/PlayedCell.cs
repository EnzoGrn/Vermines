using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayedCell : MonoBehaviour
{
    #region Attributes

    public CardView CardView;

    public DiscardedCardList DiscardedCardList;
    public event Action<PlayedCell> OnClick;

    #endregion

    #region Interface implementation

    public void OnPointerEnter(PointerEventData eventData)
    {
        // TODO: Maybe hover effect.
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // TODO: Remove hover effect.
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("PlayedCell.OnPointerClick");

        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (CardView != null && CardView._Card != null)
            // OnClick event send the card to the discard pile
            OnClick?.Invoke(this);
    }
    #endregion
}
