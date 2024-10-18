using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : MonoBehaviour {

    #region Attributes

    private PhotonView _POV;

    public Info Info;

    #endregion

    #region Methods

    public void OnEnable()
    {
        _POV = GetComponent<PhotonView>();

        Info = new Info();
    }

    public void SelectEveryFamily(int playerCount)
    {
        List<CardType> types = Constants.FamilyTypes;
        List<CardType> typesSelected = new();
        List<int> selectedFamily = new();
        System.Random random = new();

        for (int i = 0; i < playerCount; i++)
        {
            while (selectedFamily.Count == i)
            {
                int randomIndex = random.Next(types.Count);

                if (!selectedFamily.Contains(randomIndex))
                {
                    typesSelected.Add(types[randomIndex]);

                    selectedFamily.Add(randomIndex);
                }
            }
        }

        FamilyPlayed = typesSelected;
    }

    #endregion

    #region Getters & Setters

    public List<CardType> FamilyPlayed
    {
        get => Info.FamilyPlayed;
        set
        {
            Info.FamilyPlayed = value;
        }
    }

    #endregion
}

public class Info {

    public List<CardType> FamilyPlayed = new();
}
