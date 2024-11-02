using Config;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class CardContainer : MonoBehaviour {

    public PhotonView POV;

    [Header("Constraints")]
    [SerializeField]
    private bool _ForceFitContainer;

    [SerializeField]
    private bool _PreventCardInteraction;

    [Header("Alignment")]
    [SerializeField]
    private CardAlignment _Alignment = CardAlignment.Center;

    [Header("Rotation")]
    [SerializeField]
    [Range(-90f, 90f)]
    private float _MaxCardRotation;

    [SerializeField]
    private float _MaxHeightDisplacement;

    [SerializeField]
    private PlayedCardList _PlayedCardList;

    [SerializeField]
    private DiscardedCardList _DiscardedCardList;

    [SerializeField]
    private Config.Zoom _ZoomConfig;

    [SerializeField]
    private Config.AnimationSpeed _AnimationSpeedConfig;

    [SerializeField]
    private Config.CardPlay _CardPlayConfig;

    [Header("Events")]
    [SerializeField]
    private Config.AllEvents _EventsConfig;

    private CardWrapper       _CurrentDraggedCard;
    private RectTransform     _RectTransform;
    private List<CardWrapper> _Cards = new();

    private void OnEnable()
    {
        _RectTransform = GetComponent<RectTransform>();

        InitView();
        InitCards();
    }

    private void InitView()
    {
        if (POV.IsMine) {
            _MaxCardRotation       = -_MaxCardRotation / 3;
            _MaxHeightDisplacement = -_MaxHeightDisplacement;
        }
    }

    private void InitCards()
    {
        SetUpCards();
        SetCardsAnchor();
    }

    private void SetCardsRotation()
    {
        for (var i = 0; i < _Cards.Count; i++) {
            _Cards[i].TargetRotation             = GetCardRotation(i);
            _Cards[i].TargetVerticalDisplacement = GetCardVerticalDisplacement(i);
        }
    }

    private float GetCardVerticalDisplacement(int index)
    {
        if (_Cards.Count < 3)
            return 0;
        // Associate a vertical displacement based on the index in the cards list so that the center card is at max displacement while the edges are at 0 displacement
        return _MaxHeightDisplacement * (1 - Mathf.Pow(index - (_Cards.Count - 1) / 2f, 2) / Mathf.Pow((_Cards.Count - 1) / 2f, 2));
    }

    private float GetCardRotation(int index)
    {
        if (_Cards.Count < 3)
            return 0;
        // Associate a rotation based on the index in the cards list so that the first and last cards are at max rotation, mirrored around the center
        return -_MaxCardRotation * (index - (_Cards.Count - 1) / 2f) / ((_Cards.Count - 1) / 2f);
    }

    void Update()
    {
        UpdateCards();
    }

    void SetUpCards()
    {
        _Cards.Clear();

        foreach (Transform card in transform) {
            var wrapper = card.GetComponent<CardWrapper>();

            if (wrapper == null)
                wrapper = card.gameObject.AddComponent<CardWrapper>();
            wrapper.POV = POV;

            _Cards.Add(wrapper);

            AddOtherComponentsIfNeeded(wrapper);

            // Pass child card any extra config it should be aware of
            wrapper.ZoomConfig             = _ZoomConfig;
            wrapper.AnimationSpeedConfig   = _AnimationSpeedConfig;
            wrapper.EventsConfig           = _EventsConfig;
            wrapper.PreventCardInteraction = _PreventCardInteraction;
            wrapper.Container              = this;
        }
    }

    private void AddOtherComponentsIfNeeded(CardWrapper wrapper)
    {
        var canvas = wrapper.GetComponent<Canvas>();

        if (canvas == null)
            canvas = wrapper.gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;

        if (wrapper.GetComponent<GraphicRaycaster>() == null)
            wrapper.gameObject.AddComponent<GraphicRaycaster>();
    }

    private void UpdateCards()
    {
        if (transform.childCount != _Cards.Count)
            InitCards();
        if (_Cards.Count == 0)
            return;
        if (_Cards.Count >= 6)
            _ForceFitContainer = true;
        else
            _ForceFitContainer = false;
        SetCardsPosition();
        SetCardsRotation();
        SetCardsUILayers();
    }

    private void SetCardsUILayers()
    {
        for (var i = 0; i < _Cards.Count; i++)
            _Cards[i].UILayer = _ZoomConfig.DefaultSortOrder + i;
    }

    private void SetCardsPosition()
    {
        // Compute the total width of all the cards in global space
        var cardsTotalWidth = _Cards.Sum(card => card.Width * card.transform.lossyScale.x);

        // Compute the width of the container in global space
        var containerWidth = _RectTransform.rect.width * transform.lossyScale.x;

        if (_ForceFitContainer && cardsTotalWidth > containerWidth)
            DistributeChildrenToFitContainer(cardsTotalWidth);
        else
            DistributeChildrenWithoutOverlap(cardsTotalWidth);
    }

    private void DistributeChildrenToFitContainer(float childrenTotalWidth)
    {
        // Get the width of the container
        var width = _RectTransform.rect.width * transform.lossyScale.x;

        // Get the distance between each child
        var distanceBetweenChildren = (width - childrenTotalWidth) / (_Cards.Count - 1);

        // Set all children's positions to be evenly spaced out
        var currentX = transform.position.x - width / 2;

        foreach (CardWrapper child in _Cards) {
            var adjustedChildWidth = child.Width * child.transform.lossyScale.x;

            child.TargetPosition = new Vector2(currentX + adjustedChildWidth / 2, transform.position.y);
            currentX            += adjustedChildWidth + distanceBetweenChildren;
        }
    }

    private void DistributeChildrenWithoutOverlap(float childrenTotalWidth)
    {
        var currentPosition = GetAnchorPositionByAlignment(childrenTotalWidth);

        foreach (CardWrapper child in _Cards) {
            var adjustedChildWidth = child.Width * child.transform.lossyScale.x;

            child.TargetPosition = new Vector2(currentPosition + adjustedChildWidth / 2, transform.position.y);
            currentPosition     += adjustedChildWidth;
        }
    }

    private float GetAnchorPositionByAlignment(float childrenWidth)
    {
        var containerWidthInGlobalSpace = _RectTransform.rect.width * transform.lossyScale.x;

        switch (_Alignment)
        {
            case CardAlignment.Left:
                return transform.position.x - containerWidthInGlobalSpace / 2;
            case CardAlignment.Center:
                return transform.position.x - childrenWidth / 2;
            case CardAlignment.Right:
                return transform.position.x + containerWidthInGlobalSpace / 2 - childrenWidth;
            default:
                return 0;
        }
    }

    private void SetCardsAnchor()
    {
        foreach (CardWrapper child in _Cards)
            child.SetAnchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f));
    }

    public void OnCardDragStart(CardWrapper card)
    {
        _CurrentDraggedCard = card;
    }

    public void OnCardDragEnd()
    {
        // If card is in play area, play it!
        if (IsCursorInPlayArea()) {

            // Get the PlayedList from the parent gameobejct

            if (_PlayedCardList == null)
            {
                Debug.LogError("PlayedCardList not found.");
                return;
            }

            _EventsConfig?.OnCardPlayed?.Invoke(new Events.CardPlayed(_CurrentDraggedCard, _PlayedCardList, null, true));

            if (_CardPlayConfig.DestroyOnPlay)
                DestroyCard(_CurrentDraggedCard);
        } else if (IsCursorInDiscardArea()) {

            if (_DiscardedCardList == null)
            {
                Debug.LogError("DiscardedCardList not found.");
                return;
            }

            Events.CardPlayed cardPlayed = new Events.CardPlayed(_CurrentDraggedCard, null, _DiscardedCardList, false);

            // cardPlayed.PlayedOrDiscard = false;

            _EventsConfig?.OnCardPlayed?.Invoke(cardPlayed);

            if (_CardPlayConfig.DestroyOnDiscard)
                DestroyCard(_CurrentDraggedCard);
        }
        _CurrentDraggedCard = null;
    }

    public void DestroyCard(CardWrapper card)
    {
        _Cards.Remove(card);

        _EventsConfig.OnCardDestroy?.Invoke(new Events.CardDestroy(card));

        Destroy(card.gameObject);
    }

    private bool IsCursorInPlayArea()
    {
        if (_CardPlayConfig.PlayArea == null)
            return false;
        var cursorPosition  = Input.mousePosition;
        var playArea        = _CardPlayConfig.PlayArea;
        var playAreaCorners = new Vector3[4];

        playArea.GetWorldCorners(playAreaCorners);

        return (
            cursorPosition.x > playAreaCorners[0].x && cursorPosition.x < playAreaCorners[2].x &&
            cursorPosition.y > playAreaCorners[0].y && cursorPosition.y < playAreaCorners[2].y
        );
    }

    private bool IsCursorInDiscardArea()
    {
        if (_CardPlayConfig.DiscardArea == null)
            return false;
        var cursorPosition = Input.mousePosition;
        var playArea = _CardPlayConfig.DiscardArea;
        var playAreaCorners = new Vector3[4];

        playArea.GetWorldCorners(playAreaCorners);

        return (
            cursorPosition.x > playAreaCorners[0].x && cursorPosition.x < playAreaCorners[2].x &&
            cursorPosition.y > playAreaCorners[0].y && cursorPosition.y < playAreaCorners[2].y
        );
    }

    public List<ICard> GetCards()
    {
        List<ICard> cards = new();

        foreach (Transform child in transform) {
            var card = child.GetComponent<CardUIView>();

            if (card != null)
                cards.Add(card._Card);
        }
        return cards;
    }
}
