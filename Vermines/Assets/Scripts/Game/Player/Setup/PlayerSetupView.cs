using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerSetupView : MonoBehaviour {

    #region Attributes /* GameObject to setup */

    private RectTransform _RectTransform;

    [Header("GameObject to setup")]
    public CardContainer Hand;

    [SerializeField]
    private RectTransform _DiscardAreaRect;

    [SerializeField]
    private RectTransform _PlayAreaRect;

    [SerializeField]
    private TMP_Text _PlayerName;

    #endregion

    public void OnEnable()
    {
        GetComponents();

        Hand.enabled = true;

        ChangeRectTransformOfThePlayerView();

        SetPlayerName(PhotonNetwork.LocalPlayer.NickName);
    }

    /**
     * @brief Call this method to update the name of the player (UI).
     */
    public void SetPlayerName(string playerName)
    {
        _PlayerName.text = playerName;
    }

    private void GetComponents()
    {
        _RectTransform = GetComponent<RectTransform>();
    }

    private void ChangeRectTransformOfThePlayerView()
    {
        /* Put Rect Transform Bottom Center of the canvas */
        _RectTransform.anchorMin = new Vector2(0.5f, 0f);
        _RectTransform.anchorMax = new Vector2(0.5f, 0f);
        _RectTransform.pivot = new Vector2(0.5f, 0f);
        _RectTransform.anchoredPosition = new Vector2(0f, 0f);

        /* Put Discard Area Bottom Right of the canvas */
        _DiscardAreaRect.anchorMin = new Vector2(1f, 0f);
        _DiscardAreaRect.anchorMax = new Vector2(1f, 0f);
        _DiscardAreaRect.pivot = new Vector2(1f, 0f);

        /* Put Play Area Top Left of the canvas */
        _PlayAreaRect.anchorMin = new Vector2(0f, 1f);
        _PlayAreaRect.anchorMax = new Vector2(0f, 1f);
        _PlayAreaRect.pivot = new Vector2(0f, 1f);
    }
}
