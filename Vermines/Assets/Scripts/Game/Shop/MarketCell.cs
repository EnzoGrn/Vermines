using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class MarketCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    #region Attributes

    public ICard Card = null;
    public CardUIView CardUIView;

    public event Action<ICard> OnClick;

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
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (Card != null && Card.Data != null)
            OnClick?.Invoke(Card);
    }

    #endregion
}
