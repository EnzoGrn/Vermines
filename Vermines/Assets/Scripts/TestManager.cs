using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour {

    public GameObject CardUIPrefab;
    public GameObject CardContainer;
    public GameObject EnemyContainer;

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
                ""id"": ""courtesan_000"",
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
                ""id"": ""apothecary_000"",
                ""name"": ""Apothicaire"",
                ""description"": ""Vos achats c�utent <b>2E</b> de moins."",
                ""type"": 7,
                ""eloquence"": 8,
                ""souls"": 20,
                ""sprite"": ""Apothecary""
            }
        ";

        string json3 = @"
            {
                ""id"": ""worker_000"",
                ""name"": ""Ouvri�re"",
                ""description"": ""D�faussez l'<b>ouvri�re</b> pour gagnez 1E.<br>Si vous avez sacrifi� 3 ouvri�res au cours de la partie gagne <b>10A</b>."",
                ""type"": 0,
                ""souls"": 0,
                ""sprite"": ""Worker""
            }
        ";

        string json4 = @"
            {
                ""id"": ""priest_000"",
                ""name"": ""Pr�tre"",
                ""description"": ""Gagnez <b>2E</b>."",
                ""type"": 5,
                ""eloquence"": 5,
                ""souls"": 15,
                ""sprite"": ""Priest""
            }
        ";

        CardUIView card0 = BuildUIView(CardFactory.CreateCard(json1));
        CardUIView card1 = BuildUIView(CardFactory.CreateCard(json2));
        CardUIView card2 = BuildUIView(CardFactory.CreateCard(json3));
        CardUIView card3 = BuildUIView(CardFactory.CreateCard(json3));
        CardUIView card4 = BuildUIView(CardFactory.CreateCard(json3));
        CardUIView card5 = BuildUIView(CardFactory.CreateCard(json4));

        card0.transform.SetParent(CardContainer.transform);
        card1.transform.SetParent(CardContainer.transform);
        card2.transform.SetParent(CardContainer.transform);
        card3.transform.SetParent(CardContainer.transform);
        card4.transform.SetParent(CardContainer.transform);
        card5.transform.SetParent(CardContainer.transform);

        CardUIView card10 = BuildUIView(CardFactory.CreateCard(json1));
        CardUIView card11 = BuildUIView(CardFactory.CreateCard(json2));
        CardUIView card12 = BuildUIView(CardFactory.CreateCard(json3));
        CardUIView card13 = BuildUIView(CardFactory.CreateCard(json3));
        CardUIView card14 = BuildUIView(CardFactory.CreateCard(json3));
        CardUIView card15 = BuildUIView(CardFactory.CreateCard(json4));

        card10.GetCard().IsAnonyme = true;
        card11.GetCard().IsAnonyme = true;
        card12.GetCard().IsAnonyme = true;
        card13.GetCard().IsAnonyme = true;
        card14.GetCard().IsAnonyme = true;
        card15.GetCard().IsAnonyme = true;

        card10.transform.SetParent(EnemyContainer.transform);
        card11.transform.SetParent(EnemyContainer.transform);
        card12.transform.SetParent(EnemyContainer.transform);
        card13.transform.SetParent(EnemyContainer.transform);
        card14.transform.SetParent(EnemyContainer.transform);
        card15.transform.SetParent(EnemyContainer.transform);
    }
}
