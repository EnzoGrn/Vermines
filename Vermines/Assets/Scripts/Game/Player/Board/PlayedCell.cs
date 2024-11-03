using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayedCell : MonoBehaviour
{
    #region Attributes

    public CardView CardView;

    public event Action<PlayedCell> OnClick;

    #endregion

    #region Event

    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            OnClick?.Invoke(this);
        }
    }
    #endregion
}
