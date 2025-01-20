using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vermines
{
    public class GameSettingsUI : MonoBehaviour
    {
        [Header("Button Game Settings")]
        [SerializeField] private Button _ResetButton;
        [SerializeField] private Button _ApplyButton;

        [Header("Text Game Settings")]
        [SerializeField] private TMP_Text _InfoStateGameSettings; // Usefull to display error/success messages

        [Header("Input Field Game Settings")]
        [SerializeField] private GameObject _Content;

        // Player Settings
        [SerializeField] private TMP_InputField _MinPlayersNumber;
        [SerializeField] private TMP_InputField _MaxPlayersNumber;
        [Space(10)]

        // Game Flow Settings
        // [SerializeField] private TMP_InputField _MaxRounds;
        [SerializeField] private TMP_InputField _MaxTurnTime;
        [Space(10)]

        // Hand Settings
        [SerializeField] private TMP_InputField _NumberOfCardsToStartWith;
        // [SerializeField] private TMP_InputField _MaxCardsPerPlayerInHand;
        [Space(10)]
        
        // Deck Settings
        [SerializeField] private TMP_InputField _NumberOfCardsToDrawPerTurn;
        // [SerializeField] private TMP_InputField _MaxCardsNumber;
        [Space(10)]
        
        // General Game Rules
        [SerializeField] private TMP_InputField _MaxEloquence;
        [SerializeField] private TMP_InputField _EloquenceToStartWith;
        [SerializeField] private TMP_InputField _NumberOfEloquencesToStartTheTurnWith;
        [Space(10)]
        
        // Souls Settings
        [SerializeField] private TMP_InputField _MaxSoulsToWin;
        [SerializeField] private TMP_InputField _SoulsToStartWith;
        [Space(10)]
        
        // Cards Settings
        [SerializeField] private TMP_InputField _MaxCardsToPlay;
        [SerializeField] private TMP_InputField _MaxCardsToPlayPerTurn;
        [SerializeField] private TMP_InputField _MaxPlayerEquipementCards;
        [Space(10)]
        
        // Sacrifices Settings
        [SerializeField] private TMP_InputField _MaxSacrificesPerTurn;
        [Space(10)]

        // Market Settings
        [SerializeField] private TMP_InputField _MaxMarketCards;
        [Space(10)]
        
        // Courtyard Settings
        [SerializeField] private TMP_InputField _MaxCourtyardCards;

        [Header("Resources")]
        [SerializeField] private GameSettings _GameSettings;
        [SerializeField] private GameSettings _DefaultGameSettings;

        // TODO : Change the way to handle the GameSettings scriptable object
        // Create a specific Object with a type a default value and some restrictions to loop over the fields instead of hardcoding them
        // Also Load them dinamically from the GameSettings scriptable object instead of placing them in the scene
        // Also add a way to add some spaces between some fields to avoid block and display settings as categories
        // Faire des input fiels dynamiques en fonction des settings

        private void Awake()
        {
            if (_GameSettings == null)
            {
                Debug.LogError("GameSettings scriptable object not found");
            }
        }

        public void ResetGameSettings()
        {
            _MinPlayersNumber.text = _DefaultGameSettings.MinPlayers.ToString();
            _MaxPlayersNumber.text = _DefaultGameSettings.MaxPlayers.ToString();
            _MaxTurnTime.text = _DefaultGameSettings.MaxTurnTime.ToString();
            _NumberOfCardsToStartWith.text = _DefaultGameSettings.NumberOfCardsToStartWith.ToString();
            _NumberOfCardsToDrawPerTurn.text = _DefaultGameSettings.NumberOfCardsToDrawPerTurn.ToString();
            _MaxEloquence.text = _DefaultGameSettings.MaxEloquence.ToString();
            _EloquenceToStartWith.text = _DefaultGameSettings.EloquenceToStartWith.ToString();
            _NumberOfEloquencesToStartTheTurnWith.text = _DefaultGameSettings.NumberOfEloquencesToStartTheTurnWith.ToString();
            _MaxSoulsToWin.text = _DefaultGameSettings.MaxSoulsToWin.ToString();
            _SoulsToStartWith.text = _DefaultGameSettings.SoulsToStartWith.ToString();
            _MaxCardsToPlay.text = _DefaultGameSettings.MaxCardsToPlay.ToString();
            _MaxCardsToPlayPerTurn.text = _DefaultGameSettings.MaxCardsToPlayPerTurn.ToString();
            _MaxPlayerEquipementCards.text = _DefaultGameSettings.MaxPlayerEquipementCards.ToString();
            _MaxSacrificesPerTurn.text = _DefaultGameSettings.MaxSacrificesPerTurn.ToString();
            _MaxMarketCards.text = _DefaultGameSettings.MaxMarketCards.ToString();
            _MaxCourtyardCards.text = _DefaultGameSettings.MaxCourtyardCards.ToString();
            
            _InfoStateGameSettings.text = "Game settings reset to default";
            _InfoStateGameSettings.color = Color.white;
        }

        public void ApplyGameSettings()
        {
            try
            {
                CheckGameSettings();
                ApplyChangesToGameSettings();
                _InfoStateGameSettings.text = "Game settings applied successfully";
                _InfoStateGameSettings.color = Color.green;
            }
            catch (System.Exception e)
            {
                _InfoStateGameSettings.text = e.Message;
                _InfoStateGameSettings.color = Color.red;
            }
        }

        private void CheckGameSettings()
        {
            try
            {
                CheckPlayerSetting();
                CheckOtherSettings();
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        private void CheckPlayerSetting()
        {
            int minPlayers = int.Parse(_MinPlayersNumber.text);
            int maxPlayers = int.Parse(_MaxPlayersNumber.text);

            if (minPlayers < 2 || minPlayers > 4)
            {
                // Throw error message
                throw new System.Exception("Minimum number of players should be between 2 and 4");
            }

            if (maxPlayers > 4 || maxPlayers < 2)
            {
                // Throw error message
                throw new System.Exception("Maximum number of players should be between 2 and 4");
            }

            if (minPlayers > maxPlayers)
            {
                // Throw error message
                throw new System.Exception("Minimum number of players cannot be greater than maximum number of players");
            }

            // TODO : Update the lobby settings of Photon Fusion
        }

        private void ValidateAndSetField(TMP_InputField field, string fieldName)
        {
            if (string.IsNullOrEmpty(field.text))
            {
                throw new System.Exception(fieldName + " cannot be empty");
            }

            if (!int.TryParse(field.text, out int value))
            {
                throw new System.Exception(fieldName + " should be a number");
            }

            if (value < 0)
            {
                throw new System.Exception(fieldName + " should be a positive number");
            }
        }

        private void CheckOtherSettings()
        {
            ValidateAndSetField(_MaxTurnTime, "Max Turn Time");
            ValidateAndSetField(_NumberOfCardsToStartWith, "Number of Cards to Start With");
            ValidateAndSetField(_NumberOfCardsToDrawPerTurn, "Number of Cards to Draw Per Turn");
            ValidateAndSetField(_MaxEloquence, "Max Eloquence");
            ValidateAndSetField(_EloquenceToStartWith, "Eloquence to Start With");
            ValidateAndSetField(_NumberOfEloquencesToStartTheTurnWith, "Number of Eloquences to Start the Turn With");
            ValidateAndSetField(_MaxSoulsToWin, "Max Souls to Win");
            ValidateAndSetField(_SoulsToStartWith, "Souls to Start With");
            ValidateAndSetField(_MaxCardsToPlay, "Max Cards to Play");
            ValidateAndSetField(_MaxCardsToPlayPerTurn, "Max Cards to Play Per Turn");
            ValidateAndSetField(_MaxPlayerEquipementCards, "Max Player Equipment Cards");
            ValidateAndSetField(_MaxSacrificesPerTurn, "Max Sacrifices Per Turn");
            ValidateAndSetField(_MaxMarketCards, "Max Market Cards");
            ValidateAndSetField(_MaxCourtyardCards, "Max Courtyard Cards");
        }

        private void ApplyChangesToGameSettings()
        {
            _GameSettings.MinPlayers = int.Parse(_MinPlayersNumber.text);
            _GameSettings.MaxPlayers = int.Parse(_MaxPlayersNumber.text);
            _GameSettings.MaxTurnTime = int.Parse(_MaxTurnTime.text);
            _GameSettings.NumberOfCardsToStartWith = int.Parse(_NumberOfCardsToStartWith.text);
            _GameSettings.NumberOfCardsToDrawPerTurn = int.Parse(_NumberOfCardsToDrawPerTurn.text);
            _GameSettings.MaxEloquence = int.Parse(_MaxEloquence.text);
            _GameSettings.EloquenceToStartWith = int.Parse(_EloquenceToStartWith.text);
            _GameSettings.NumberOfEloquencesToStartTheTurnWith = int.Parse(_NumberOfEloquencesToStartTheTurnWith.text);
            _GameSettings.MaxSoulsToWin = int.Parse(_MaxSoulsToWin.text);
            _GameSettings.SoulsToStartWith = int.Parse(_SoulsToStartWith.text);
            _GameSettings.MaxCardsToPlay = int.Parse(_MaxCardsToPlay.text);
            _GameSettings.MaxCardsToPlayPerTurn = int.Parse(_MaxCardsToPlayPerTurn.text);
            _GameSettings.MaxPlayerEquipementCards = int.Parse(_MaxPlayerEquipementCards.text);
            _GameSettings.MaxSacrificesPerTurn = int.Parse(_MaxSacrificesPerTurn.text);
            _GameSettings.MaxMarketCards = int.Parse(_MaxMarketCards.text);
            _GameSettings.MaxCourtyardCards = int.Parse(_MaxCourtyardCards.text);
        }
    }
}
