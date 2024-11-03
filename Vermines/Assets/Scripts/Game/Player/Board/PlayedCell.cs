using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayedCell : MonoBehaviour
{
    #region Attributes

    public CardView CardView;

    //public SacrifiedCardList SacrifiedCardList;
    public event Action<PlayedCell> OnClick;

    #endregion

    void Update()
    {
        //if (Input.GetMouseButtonDown(0)) // Left click
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    if (Physics.Raycast(ray, out RaycastHit hit))
        //    {
        //        Debug.Log("Raycast hit: " + hit.collider.gameObject.name);
        //    }
        //    else
        //    {
        //        Debug.Log("No object detected at click position.");
        //    }
        //}
    }

    #region Interface implementation

    void OnMouseDown()
    {
        Debug.Log("Clicked on " + gameObject.name);

        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Debug.Log("Left mouse button clicked on " + gameObject.name);
            OnClick?.Invoke(this);
        }
    }
    #endregion
}
