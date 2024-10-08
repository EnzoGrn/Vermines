using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour {

    public GameObject CardUIPrefab;

    public CardUIView BuildUIView(Card card)
    {
        GameObject cardView = GameObject.Instantiate(CardUIPrefab);

        CardUIView cardUIView = cardView.GetComponent<CardUIView>();

        cardUIView.SetCard(card);

        return cardUIView;
    }

    private void Start()
    {
        string json1 = @"
            {
                ""name"": ""Courtisane"",
                ""description"": ""Piochez deux cartes."",
                ""type"": 0,
                ""eloquence"": 12,
                ""souls"": 20,
                ""sprite"": ""Courtesan""
            }
        ";

        string json2 = @"
            {
                ""name"": ""Apothicaire"",
                ""description"": ""Vos achats côutent <b>2E</b> de moins."",
                ""type"": 7,
                ""eloquence"": 8,
                ""souls"": 20,
                ""sprite"": ""Apothecary""
            }
        ";

        string json3 = @"
            {
                ""name"": ""Ouvrière"",
                ""description"": ""Défaussez l'<b>ouvrière</b> pour gagnez 1E.<br>Si vous avez sacrifié 3 ouvrières au cours de la partie gagne <b>10A</b>."",
                ""type"": 0,
                ""souls"": 0,
                ""sprite"": ""Worker""
            }
        ";

        CardUIView card0 = BuildUIView(CardFactory.CreateCard(json1));
        CardUIView card1 = BuildUIView(CardFactory.CreateCard(json2));
        CardUIView card2 = BuildUIView(CardFactory.CreateCard(json3));

        GameObject UI = GameObject.Find("UI");

        card0.transform.SetParent(UI.transform);
        card1.transform.SetParent(UI.transform);
        card2.transform.SetParent(UI.transform);
    }
}
