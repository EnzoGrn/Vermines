using Defective.JSON;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

namespace Vermines.Settings
{
    /// <summary>
    /// Game Settings.
    /// 
    /// Contains all the settings for the game.
    /// 
    /// For now settings must be grouped by category.
    /// 
    /// </summary>
    [CreateAssetMenu(menuName = "Vermines/Settings/GameSetting")]
    public class GameSettings : ScriptableObject
    {
        // Player Settings
        [Tooltip("Maximum number of players allowed in the game.")]
        public IntSetting MaxPlayers = new("Max Players", 4, 2, 4, "Player Settings");
        [Tooltip("Minimum number of players required to start the game.")]
        public IntSetting MinPlayers = new("Min Players", 2, 2, 4, "Player Settings");

        // Game Flow Settings
        public BoolSetting IsRoundBased = new BoolSetting("Is Round Based", false, "Game Flow Settings");
        [Tooltip("Maximum number of rounds per game.")]
        public IntSetting MaxRounds = new IntSetting("Max Rounds", 15, 1, 100, "Game Flow Settings");
        [Tooltip("Time (in seconds) allowed per player's turn.")]
        public IntSetting MaxTurnTime = new IntSetting("Max Turn Time", 120, 60, 300, "Game Flow Settings");

        // Hand Settings
        [Tooltip("Number of cards each player starts with in their hand.")]
        public IntSetting NumberOfCardsToStartWith = new IntSetting("Number Of Cards To Start With", 3, 1, 5, "Hand Settings");
        [Tooltip("Toggle whether to enforce a maximum number of cards per player in hand.")]
        public BoolSetting IsMaxCardsPerPlayerInHandBased = new BoolSetting("Is Max Cards Per Player In Hand Based", false, "Hand Settings");
        [Tooltip("Maximum number of cards a player can hold in their hand.")]
        public IntSetting MaxCardsPerPlayerInHand = new IntSetting("Max Cards Per Player In Hand", 10, 5, 25, "Hand Settings");

        // Deck Settings
        [Tooltip("Number of cards drawn per turn.")]
        public IntSetting NumberOfCardsToDrawPerTurn = new IntSetting("Number Of Cards To Draw Per Turn", 1, 1, 5, "Deck Settings");
        [Tooltip("Toggle whether to enforce a maximum number of cards per player.")]
        public BoolSetting IsMaxCardsNumberBased = new BoolSetting("Is Max Cards Number Based", false, "Deck Settings");
        [Tooltip("Maximum number of cards a player can hold in their deck.")]
        public IntSetting MaxCardsNumber = new IntSetting("Max Cards Number", 20, 10, 25, "Deck Settings");

        // Game Rules Settings
        [Tooltip("Maximum amount of eloquence a player can have.")]
        public IntSetting MaxEloquence = new IntSetting("Max Eloquence", 20, 1, 50, "Game Rules Settings");
        [Tooltip("Initial eloquence for each player at the start of the game.")]
        public IntSetting EloquenceToStartWith = new IntSetting("Eloquence To Start With", 0, 0, 100, "Game Rules Settings");
        [Tooltip("Amount of eloquence a player receives at the start of each turn.")]
        public IntSetting NumberOfEloquencesToStartTheTurnWith = new IntSetting("Number Of Eloquences To Start The Turn With", 2, 1, 100, "Game Rules Settings");

        [Tooltip("Maximum number of souls needed to win.")]
        public IntSetting MaxSoulsToWin = new IntSetting("Max Souls To Win", 100, 1, 100, "Game Rules Settings");
        [Tooltip("Initial number of souls each player starts with.")]
        public IntSetting SoulsToStartWith = new IntSetting("Souls To Start With", 0, 0, 100, "Game Rules Settings");

        // Cards Settings
        public IntSetting MaxCardsToPlay = new IntSetting("Max Cards To Play", 3, 1, 3, "Cards Settings");
        public IntSetting MaxCardsToPlayPerTurn = new IntSetting("Max Cards To Play Per Turn", 3, 1, 3, "Cards Settings");
        public IntSetting MaxPlayerEquipementCards = new IntSetting("Max Player Equipement Cards", 3, 1, 3, "Cards Settings");

        // Sacrifice Settings
        [Tooltip("Maximum number of followers a player can sacrifice per turn.")]
        public IntSetting MaxSacrificesPerTurn = new IntSetting("Max Sacrifices Per Turn", 2, 1, 3, "Sacrifice Settings");

        // Market Settings
        [Tooltip("Number of cards to replenish in the market at the end of a turn.")]
        public IntSetting MaxMarketCards = new IntSetting("Max Market Cards", 5, 1, 5, "Market Settings");
        [Tooltip("Indicates whether the market deck reshuffles when exhausted.")]
        public BoolSetting AllowMarketReshuffle = new BoolSetting("Allow Market Reshuffle", false, "Market Settings");

        // Courtyard  Settings
        [Tooltip("Number of equipment cards a player can attach to their cultist.")]
        public IntSetting MaxCourtyardCards = new IntSetting("Max Courtyard Cards", 5, 1, 5, "Courtyard  Settings");
        [Tooltip("The cost of reshuffling the deck if required (optional balance setting).")]
        public BoolSetting AllowCourtyardReshuffle = new BoolSetting("Allow Market Reshuffle", true, "Courtyard  Settings");

        // Advanced Settings
        [Tooltip("Enable debug mode for additional logs and testing features.")]
        public BoolSetting DebugMode = new BoolSetting("Debug Mode", false, "Advanced Settings");
        [Tooltip("Seed for random number generation (set to 0 for random seed).")]
        public IntSetting RandomSeed = new IntSetting("Random Seed", 0, 0, 1, "Advanced Settings");

        /// <summary>
        /// Serialize GameSettings.
        /// </summary>
        /// <example>
        /// [
        ///    {
        ///        type: 0,
        ///        name: "Max Players",
        ///        category: "Player Settings"
        ///        value: 4,
        ///        minValue: 2,
        ///        maxValue: 4,
        ///    },
        ///    {
        ///        type: 1,
        ///        name: "Is Round Based",
        ///        category: "Game Flow Settings"
        ///        value: false, // bool
        ///    }
        /// ]
        /// </example>
        /// <returns>GameSettings Data.</returns>
        public JSONObject Serialize()
        {
            JSONObject json = new(JSONObject.Type.Array);

            foreach(var field in this.GetType().GetFields())
            {
                try
                {
                    ASetting setting = (ASetting)field.GetValue(this);

                    if (setting == null)
                        continue;

                    setting.Serialize(json);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
            return json;
        }

        /// <summary>
        /// Deserialize gameSettings, please use Initialize before calling this method.
        /// </summary>
        public void Deserialize(string json, int offset, int numberOfData)
        {
            JSONObject data = new(json);

            Debug.Log("DESERIALIZE -> " + data.ToString());

            if (data.type != JSONObject.Type.Array)
            {
                Debug.LogError("Invalid JSON format: Expected an array.");
                return;
            }

            // Loop through each field in GameSettings
            for (int i = 0; i < numberOfData; i++)
            {
                var field = this.GetType().GetFields()[i + offset];
                if (field.GetValue(this) is ASetting setting)
                {
                    // Check if there's a corresponding JSON object with the same name
                    if (i < data.list.Count && data.list[i].HasField("name") && data.list[i].GetField("name").stringValue == setting.Name)
                    {
                        Debug.Log($"AHAHAHA -> {data.list[i]}");

                        if (data.HasField("type"))
                        {
                            Debug.Log($"Type -> {(SettingType)data.GetField("type").intValue}");

                            //Type = (SettingType)serializedGameSettings.GetField("type").intValue; // Convert JSON int to Enum
                        }

                        if (data.HasField("name"))
                        {
                            Debug.Log($"name -> {(SettingType)data.GetField("name").intValue}");

                            //Name = json.GetField("name").stringValue;
                        }

                        if (data.HasField("category"))
                        {
                            Debug.Log($"category -> {(SettingType)data.GetField("category").intValue}");


                            //Category = json.GetField("category").stringValue;
                        }

                        if (data.HasField("value"))
                        {
                            Debug.Log($"value -> {(SettingType)data.GetField("value").intValue}");


                            //Value = (int)json.GetField("value").intValue;
                        }

                        if (data.HasField("minValue"))
                        {
                            Debug.Log($"minValue -> {(SettingType)data.GetField("minValue").intValue}");

                            //MinValue = (int)json.GetField("minValue").intValue;
                        }

                        if (data.HasField("maxValue"))
                        {
                            Debug.Log($"maxValue -> {(SettingType)data.GetField("maxValue").intValue}");

                            //MaxValue = (int)json.GetField("maxValue").intValue;
                        }

                        // Deserialize the field using the corresponding JSON object
                        setting.Deserialize(data.list[i]);
                    }
                    else
                    {
                        // TODO: Find the date in the json if the don't find it at the right possition
                        Debug.LogWarning($"Cannot get the data to deserialize for field {field.Name}");
                    }
                }
            }
        }

    }
}


