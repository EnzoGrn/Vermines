using Defective.JSON;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using UnityEditor;
using UnityEngine;

/*
"""json example
{
    "name": "Bard",
    "description": "Gagnez 8E.",
    "type": "Bee",
    "eloquence": 14,
    "souls": 25,
    "sprite": "Bard",
    "turnStartEffect": "WonEloquence",
    "turnStartParameters": [
        8
    ],
}
"""
*/
public class JSONCardParser {

    public CardData Parse(string cardConfig)
    {
        CardData cardData = ScriptableObject.CreateInstance<CardData>();

        // Parse JSON
        JSONObject json = new(cardConfig);

        Debug.Assert(json != null, "The JSON file is not valid.");

        // Parse default values of card

        if (json["name"] != null)
            cardData.Name = json["name"].ToString().Trim('\"');
        
        if (json["description"] != null)
            cardData.Description = json["description"].ToString().Trim('\"'); // TODO: Maybe letter Description will have bold world that can display a tooltip with the description of the effect

        if (json["type"] != null) {

            // TODO: When trying to pass a string to the JSON, he didn't recognize it, so I put the type in int
            /*string typeString = json["type"].ToString();

            cardData.Type = CardTypeMethods.ToEnum(typeString);*/

            cardData.Type = (CardType)json["type"].intValue;
        }

        if (json["eloquence"] != null)
            cardData.Eloquence = json["eloquence"].intValue;

        if (json["souls"] != null)
            cardData.Souls = json["souls"].intValue;

        if (json["sprite"] != null)
            cardData.Sprite = Resources.Load<Sprite>("Sprites/Card/" + cardData.Type.ToString().Trim('\"') + "/" + json["sprite"].ToString().Trim('\"'));

        // Parse effects
        ParseEffect(json, "passiveEffect", "passiveParameters", (effect, parameters) => {
            cardData.PassiveEffect = PassiveFactory.Create(effect, parameters);
        });

        ParseEffect(json, "turnStartEffect", "turnStartParameters", (effect, parameters) => {
            cardData.TurnStartEffect = TurnStartFactory.Create(effect, parameters);
        });

        ParseEffect(json, "playedEffect", "playedParameters", (effect, parameters) => {
            cardData.PlayedEffect = PlayedFactory.Create(effect, parameters);
        });

        ParseEffect(json, "discardEffect", "discardParameters", (effect, parameters) => {
            cardData.DiscardEffect = DiscardFactory.Create(effect, parameters);
        });

        ParseEffect(json, "sacrificeEffect", "sacrificeParameters", (effect, parameters) => {
            cardData.SacrificeEffect = SacrificeFactory.Create(effect, parameters);
        });

        return cardData;
    }

    private void ParseEffect(JSONObject json, string effectKey, string parametersKey, System.Action<string, List<object>> applyEffect)
    {
        if (json[effectKey] != null) {
            List<object> parameters = new();

            if (json[parametersKey] != null) {
                JSONObject parameterArray = json[parametersKey];

                foreach (var parameter in parameterArray) {
                    if (parameter is JSONObject value) {
                        if (!string.IsNullOrEmpty(value.ToString()))
                            parameters.Add(value.ToString().Trim('\"'));
                        else if (value.isNumber)
                            parameters.Add(value.intValue);
                        else if (value.isBool)
                            parameters.Add(value.boolValue);
                        else
                            Debug.LogError("The parameter is not a valid type.");
                    }
                }
            }

            applyEffect(json[effectKey].ToString(), parameters);
        }
    }
}
