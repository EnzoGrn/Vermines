using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {

    // -- GameObject Constants Name -- //

    /*
     * Name of the canvas GameObject in the game scene that need to be found
     * If it's not found, the game will not work
     */
    public static string PlayGroundName = "PlayGround";

    // -- Prefabs Constants Name -- //

    public static string PlayerPref     = "Prefabs/Player/AU_Player";
    public static string PlayGroundPref = "Prefabs/Game/PlayGround";

    // -- Constants string -- //

    public static string Clone = "(Clone)";
    public static string Mine = "(Mine)";
    public static string Other = "(Other)";

    // -- Get GameObject Constants -- //

    public static GameObject PlayGround
    {
        get
        {
            if (GameObject.Find(PlayGroundName) != null)
                return GameObject.Find(PlayGroundName + Clone);
            else if (GameObject.Find(PlayGroundName + Clone) != null)
                return GameObject.Find(PlayGroundName + Clone);
            else
                return null;
        }
    }

    // -- Constants type of cards in the game -- //

    public static string[] BasicTypes = {
        "Bee", "Mosquito", "Firefly"
    };

    public static List<CardType> FamilyTypes = new () {
        CardType.Fly, CardType.Ladybug, CardType.Scarab, CardType.Cricket
    };

    // -- Scriptable Object (CardData) path -- //

    public static string ScriptableObjectCardsPath =  "ScriptableObject/Cards/";
    public static string PathToStartingCard        = $"{ScriptableObjectCardsPath}StartingCards/";
    public static string PathToItemsCard           = $"{ScriptableObjectCardsPath}Item/";
    public static string PathToEquipmentCard       = $"{PathToItemsCard}Equipment/";
    public static string PathToToolsCard           = $"{PathToItemsCard}Tools/";
    public static string PathToPartisan            = $"{ScriptableObjectCardsPath}Partisans/";
}
