using UnityEditor;
using UnityEngine;

namespace Vermines
{
    [CustomEditor(typeof(GameSettings))]
    public class GameSettingsEditor : Editor
    {
        private GUIStyle warningStyle;
        private void InitializeStyles()
        {
            if (warningStyle == null)
            {
                warningStyle = new GUIStyle(EditorStyles.label);
                warningStyle.normal.textColor = Color.yellow; // Warning text color
                warningStyle.fontStyle = FontStyle.Bold; // Make it bold for emphasis
            }
        }

        public override void OnInspectorGUI()
        {
            // Get reference to the target object
            GameSettings gameSettings = (GameSettings)target;

            // Player Settings
            GUILayout.Label("Player Settings", EditorStyles.boldLabel);
            gameSettings.MinPlayers = EditorGUILayout.IntField("Min Players", gameSettings.MinPlayers);
            gameSettings.MaxPlayers = EditorGUILayout.IntField("Max Players", gameSettings.MaxPlayers);

            if (gameSettings.MinPlayers < 2 || gameSettings.MaxPlayers > 4)
            {
                EditorGUILayout.HelpBox("Min Players must be >= 2 and Max Players must be <= 4.", MessageType.Error);
            }

            if (gameSettings.MaxPlayers < gameSettings.MinPlayers)
            {
                EditorGUILayout.HelpBox("Max Players cannot be less than Min Players.", MessageType.Error);
            }

            // Game Flow Settings
            GUILayout.Space(10);
            GUILayout.Label("Game Flow Settings", EditorStyles.boldLabel);

            gameSettings.IsRoundBased = EditorGUILayout.Toggle("Round Based", gameSettings.IsRoundBased);

            if (gameSettings.IsRoundBased)
            {
                gameSettings.MaxRounds = EditorGUILayout.IntField("Max Rounds", gameSettings.MaxRounds);

                if (gameSettings.MaxRounds < 1 || gameSettings.MaxRounds > 100)
                {
                    EditorGUILayout.HelpBox("Max Rounds must be between 1 and 100.", MessageType.Error);
                }
            }

            gameSettings.MaxTurnTime = EditorGUILayout.IntField("Max Turn Time (seconds)", gameSettings.MaxTurnTime);

            if (gameSettings.MaxTurnTime < 60 || gameSettings.MaxTurnTime > 300)
            {
                EditorGUILayout.HelpBox("Max Turn Time must be between 60 and 300 seconds.", MessageType.Error);
            }

            // Hand Settings
            GUILayout.Space(10);
            GUILayout.Label("Hand Settings", EditorStyles.boldLabel);
            gameSettings.NumberOfCardsToStartWith = EditorGUILayout.IntField("Number of Cards to Start With", gameSettings.NumberOfCardsToStartWith);
            gameSettings.IsMaxCardsPerPlayerInHandBased = EditorGUILayout.Toggle("Limit Max Cards in Hand", gameSettings.IsMaxCardsPerPlayerInHandBased);

            if (gameSettings.IsMaxCardsPerPlayerInHandBased)
            {
                gameSettings.MaxCardsPerPlayerInHand = EditorGUILayout.IntField("Max Cards Per Player In Hand", gameSettings.MaxCardsPerPlayerInHand);

                if (gameSettings.MaxCardsPerPlayerInHand < gameSettings.NumberOfCardsToStartWith || gameSettings.MaxCardsPerPlayerInHand > 25)
                {
                    EditorGUILayout.HelpBox("Max Cards Per Player In Hand must be >= Number of Cards to Start With and <= 25.", MessageType.Error);
                }
            }

            if (gameSettings.NumberOfCardsToStartWith < 1 || gameSettings.NumberOfCardsToStartWith > 20)
            {
                EditorGUILayout.HelpBox("Number of Cards to Start With must be between 1 and 20.", MessageType.Error);
            }

            // Deck Settings
            GUILayout.Space(10);
            GUILayout.Label("Deck Settings", EditorStyles.boldLabel);
            gameSettings.NumberOfCardsToDrawPerTurn = EditorGUILayout.IntField("Number of Cards to Draw Per Turn", gameSettings.NumberOfCardsToDrawPerTurn);

            if (gameSettings.NumberOfCardsToDrawPerTurn < 1 || gameSettings.NumberOfCardsToDrawPerTurn > 5)
            {
                EditorGUILayout.HelpBox("Number of Cards to Draw Per Turn must be between 1 and 5.", MessageType.Error);
            }

            gameSettings.IsMaxCardsNumberBased = EditorGUILayout.Toggle("Limit Max Cards in Deck", gameSettings.IsMaxCardsNumberBased);

            if (gameSettings.IsMaxCardsNumberBased)
            {
                gameSettings.MaxCardsNumber = EditorGUILayout.IntField("Max Cards Number", gameSettings.MaxCardsNumber);

                if (gameSettings.MaxCardsNumber < gameSettings.NumberOfCardsToStartWith || gameSettings.MaxCardsNumber > 25)
                {
                    EditorGUILayout.HelpBox("Max Cards Number must be >= Number of Cards to Start With and <= 25.", MessageType.Error);
                }
            }

            // General Game Rules
            GUILayout.Space(10);
            GUILayout.Label("General Game Rules", EditorStyles.boldLabel);
            gameSettings.MaxEloquence = EditorGUILayout.IntField("Max Eloquence", gameSettings.MaxEloquence);
            gameSettings.EloquenceToStartWith = EditorGUILayout.IntField("Eloquence To Start With", gameSettings.EloquenceToStartWith);
            gameSettings.NumberOfEloquencesToStartTheTurnWith = EditorGUILayout.IntField("Eloquences To Start Each Turn", gameSettings.NumberOfEloquencesToStartTheTurnWith);

            if (gameSettings.MaxEloquence < 1 || gameSettings.MaxEloquence > 50)
            {
                EditorGUILayout.HelpBox("Max Eloquence must be between 1 and 50.", MessageType.Error);
            }

            if (gameSettings.EloquenceToStartWith < 0 || gameSettings.EloquenceToStartWith > gameSettings.MaxEloquence)
            {
                EditorGUILayout.HelpBox("Eloquence To Start With must be between 0 and Max Eloquence.", MessageType.Error);
            }

            if (gameSettings.NumberOfEloquencesToStartTheTurnWith < 0 || gameSettings.NumberOfEloquencesToStartTheTurnWith > gameSettings.MaxEloquence)
            {
                EditorGUILayout.HelpBox("Eloquences To Start Each Turn must be between 0 and Max Eloquence.", MessageType.Error);
            }

            // Souls Settings
            GUILayout.Space(10);
            GUILayout.Label("Souls Settings", EditorStyles.boldLabel);
            gameSettings.MaxSoulsToWin = EditorGUILayout.IntField("Max Souls To Win", gameSettings.MaxSoulsToWin);
            gameSettings.SoulsToStartWith = EditorGUILayout.IntField("Souls To Start With", gameSettings.SoulsToStartWith);

            if (gameSettings.MaxSoulsToWin < 1 || gameSettings.MaxSoulsToWin > 200)
            {
                EditorGUILayout.HelpBox("Max Souls To Win must be between 1 and 200.", MessageType.Error);
            }

            if (gameSettings.SoulsToStartWith < 0 || gameSettings.SoulsToStartWith > gameSettings.MaxSoulsToWin)
            {
                EditorGUILayout.HelpBox("Souls To Start With must be between 0 and Max Souls To Win.", MessageType.Error);
            }

            // Card To Play Settings
            GUILayout.Space(10);
            GUILayout.Label("Card To Play Settings", EditorStyles.boldLabel);
            gameSettings.MaxCardsToPlay = EditorGUILayout.IntField("Max Cards To Play", gameSettings.MaxCardsToPlay);
            gameSettings.MaxCardsToPlayPerTurn = EditorGUILayout.IntField("Max Cards To Play Per Turn", gameSettings.MaxCardsToPlayPerTurn);
            gameSettings.MaxPlayerEquipementCards = EditorGUILayout.IntField("Max Player Equipment Cards", gameSettings.MaxPlayerEquipementCards);

            if (gameSettings.MaxCardsToPlay < 1 || gameSettings.MaxCardsToPlay > 3)
            {
                EditorGUILayout.HelpBox("Max Cards To Play must be between 1 and 3.", MessageType.Error);
            }

            if (gameSettings.MaxCardsToPlayPerTurn < 1 || gameSettings.MaxCardsToPlayPerTurn > gameSettings.MaxCardsToPlay)
            {
                EditorGUILayout.HelpBox("Max Cards To Play Per Turn must be between 1 and Max Cards To Play.", MessageType.Error);
            }

            if (gameSettings.MaxPlayerEquipementCards < 1 || gameSettings.MaxPlayerEquipementCards > 3)
            {
                EditorGUILayout.HelpBox("Max Player Equipment Cards must be between 1 and 3.", MessageType.Error);
            }

            // Sacrifice Settings
            GUILayout.Space(10);
            GUILayout.Label("Sacrifice Settings", EditorStyles.boldLabel);
            gameSettings.MaxSacrificesPerTurn = EditorGUILayout.IntField("Max Sacrifices Per Turn", gameSettings.MaxSacrificesPerTurn);

            if (gameSettings.MaxSacrificesPerTurn < 1 || gameSettings.MaxSacrificesPerTurn > 5)
            {
                EditorGUILayout.HelpBox("Max Sacrifices Per Turn must be between 1 and 5.", MessageType.Error);
            }

            // Market Settings
            GUILayout.Space(10);
            GUILayout.Label("Market Settings", EditorStyles.boldLabel);
            gameSettings.MaxMarketCards = EditorGUILayout.IntField("Max Market Cards", gameSettings.MaxMarketCards);
            gameSettings.AllowMarketReshuffle = EditorGUILayout.Toggle("Allow Market Reshuffle", gameSettings.AllowMarketReshuffle);

            if (gameSettings.MaxMarketCards < 1 || gameSettings.MaxMarketCards > 5)
            {
                EditorGUILayout.HelpBox("Max Market Cards must be between 1 and 5.", MessageType.Error);
            }

            // Courtyard Settings
            GUILayout.Space(10);
            GUILayout.Label("Courtyard Settings", EditorStyles.boldLabel);
            gameSettings.MaxCourtyardCards = EditorGUILayout.IntField("Max Courtyard Cards", gameSettings.MaxCourtyardCards);
            gameSettings.AllowCourtyardReshuffle = EditorGUILayout.Toggle("Allow Courtyard Reshuffle", gameSettings.AllowCourtyardReshuffle);

            if (gameSettings.MaxCourtyardCards < 1 || gameSettings.MaxCourtyardCards > 5)
            {
                EditorGUILayout.HelpBox("Max Courtyard Cards must be between 1 and 5.", MessageType.Error);
            }

            // Save changes made in the inspector
            if (GUI.changed)
            {
                EditorUtility.SetDirty(gameSettings);
                Repaint();
            }
        }
    }
}
