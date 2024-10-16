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

    public static string PlayGroundPref = "Prefabs/Game/PlayGround";
    public static string PlayerViewPref = "Prefabs/Player/Player View";
    public static string PlayerPref     = "Prefabs/Player/Player";

    // -- Constants string -- //

    public static string Clone = "(Clone)";
    public static string Mine = "(Mine)";
    public static string Other = "(Other)";

    // -- Get GameObject Constants -- //

    public static GameObject InstantiatePlayerView(bool viewIsMine)
    {
        if (viewIsMine)
            return GameObject.Instantiate(Resources.Load<GameObject>($"{PlayerViewPref} {Mine}"));
        else
            return GameObject.Instantiate(Resources.Load<GameObject>($"{PlayerViewPref} {Other}"));
    }

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

    public static CardType[] FamilyTypes = {
        CardType.Fly, CardType.Ladybug, CardType.Scarab, CardType.Cricket
    };
}
