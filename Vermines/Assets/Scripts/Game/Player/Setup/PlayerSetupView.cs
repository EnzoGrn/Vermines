using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Vermines;

public class PlayerSetupView : MonoBehaviour {

    #region Attributes /* GameObject to setup */

    private RectTransform _RectTransform;

    [Header("GameObject to setup")]
    public CardContainer Hand;

    [SerializeField]
    private RectTransform _HandRect;

    [SerializeField]
    private RectTransform _DiscardAreaRect;

    [SerializeField]
    private RectTransform _PlayAreaRect;

    [SerializeField]
    private RectTransform _EloquenceTextRect;

    [SerializeField]
    private TMP_Text _PlayerName;

    [SerializeField]
    private TMP_Text _Eloquence;

    [SerializeField]
    private TMP_Text _Family;

    private PhotonView _POV;

    [SerializeField]
    private GameObject _CardUIPrefab;

    #endregion

    public void OnEnable()
    {
        GetComponents();

        Hand.enabled = true;

        ChangeRectTransformOfThePlayerView();
    }

    private void GetComponents()
    {
        _POV           = GetComponent<PhotonView>();
        _RectTransform = GetComponent<RectTransform>();
    }

    private void ChangeRectTransformOfThePlayerView()
    {
        /* Put Rect Transform Bottom Center of the canvas */
        _RectTransform.anchorMin        = new Vector2(0.5f, 0f);
        _RectTransform.anchorMax        = new Vector2(0.5f, 0f);
        _RectTransform.pivot            = new Vector2(0.5f, 0f);
        _RectTransform.anchoredPosition = new Vector2(0f, 0f);

        /* Put Hand Rect Bottom Center of the canvas */
        _HandRect.anchorMin = new Vector2(0.5f, 0f);
        _HandRect.anchorMax = new Vector2(0.5f, 0f);
        _HandRect.pivot = new Vector2(0.5f, 0f);
        _HandRect.anchoredPosition = new Vector2(0f, 0f);

        /* Put Discard Area Bottom Right of the canvas */
        _DiscardAreaRect.anchorMin = new Vector2(1f, 0f);
        _DiscardAreaRect.anchorMax = new Vector2(1f, 0f);
        _DiscardAreaRect.pivot     = new Vector2(1f, 0f);

        /* Put Play Area Top Left of the canvas */
        _PlayAreaRect.anchorMin = new Vector2(0f, 1f);
        _PlayAreaRect.anchorMax = new Vector2(0f, 1f);
        _PlayAreaRect.pivot     = new Vector2(0f, 1f);

        /* Put Text Rect Bottom Left of the canvas + Position x (-200) */
        _EloquenceTextRect.anchorMin        = new Vector2(0f, 0f);
        _EloquenceTextRect.anchorMax        = new Vector2(0f, 0f);
        _EloquenceTextRect.pivot            = new Vector2(0f, 0f);
        _EloquenceTextRect.anchoredPosition = new Vector2(-200f, 0f);
    }

    public void EditView(Data data)
    {
        _PlayerName.text = data.Profile.Nickname + " - " + data.Profile.PlayerID.ToString();
		_Eloquence.text  = $"{data.Eloquence.ToString()}E";
        _Family.text     = data.Family.ToString();
    }

    public void EditHandView(Data data)
    {
        List<ICard> currentCards = Hand.GetCards();
        List<ICard> cards        = data.HandDeck.Cards;

        foreach (ICard card in cards)
            if (!currentCards.Contains(card))
                AddCardToHand(card);
    }

    public void AddCardToHand(ICard card)
    {
        if (card == null)
            return;
        BuildUIView(card);
    }

    private void BuildUIView(ICard card)
    {
        GameObject cardView   = GameObject.Instantiate(_CardUIPrefab);
        CardUIView cardUIView = cardView.GetComponent<CardUIView>();

        cardUIView.SetCard(card);

        cardUIView.transform.SetParent(Hand.transform);
    }
}
