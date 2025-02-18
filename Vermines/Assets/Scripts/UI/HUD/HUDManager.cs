using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

using System.Linq;

namespace Vermines.HUD {

    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.CardSystem.Enumerations;

    /// <summary>
    /// This class is only a placeholder for the real Player class.
    /// This needs to be replaced by the actual Player class.
    /// Do not remove this class, it is used in debugging.
    /// </summary>
    public class PlayerData
    {
        public int Eloquence;
        public int Souls;
        public string Nickname;
        public CardFamily Family;

        public PlayerData() {}

        public PlayerData(Vermines.Player.PlayerData playerData)
        {
            Eloquence = playerData.Eloquence;
            Souls = playerData.Souls;
            Nickname = playerData.Nickname;
            Family = playerData.Family;
        }

        public void UpdatePlayerData(Vermines.Player.PlayerData playerData)
        {
            Eloquence = playerData.Eloquence;
            Souls = playerData.Souls;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager instance;

        [SerializeField] private Transform playerListParent;
        [SerializeField] private GameObject playerBannerPrefab;
        [SerializeField] private GameObject phaseBannerObject;
        [SerializeField] private GameObject deskOverlay;
        [SerializeField] private TextMeshProUGUI buttonText; // Texte associé au bouton
        [SerializeField] private GameObject _PhaseButton;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        //private List<PlayerBanner> playerBanners = new List<PlayerBanner>();
        private int currentPlayerIndex = 0;
        private PhaseType currentPhase = PhaseType.Sacrifice;

        private List<int> playerIds;
        private Dictionary<int, PlayerData> players;
        private Dictionary<int, PlayerBanner> playerBanners = new();

        void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);

            deskOverlay.SetActive(false);

            if (debugMode)
            {
                players = new Dictionary<int, PlayerData>
                {
                    { 1, new PlayerData { Eloquence = 20, Souls = 100, Nickname = "Player 1", Family = CardFamily.None } },
                    { 2, new PlayerData { Eloquence = 20, Souls = 100, Nickname = "Player 2", Family = CardFamily.None } },
                    { 3, new PlayerData { Eloquence = 20, Souls = 100, Nickname = "Player 3", Family = CardFamily.None } },
                    { 4, new PlayerData { Eloquence = 20, Souls = 100, Nickname = "Player 4", Family = CardFamily.None } }
                };

                playerIds = players.Keys.ToList();

                Initialize();
            }
        }

        public void Initialize()
        {
            foreach (var player in players)
            {
                // Create a prefab
                GameObject bannerObject = Instantiate(playerBannerPrefab, playerListParent);
                PlayerBanner banner = bannerObject.GetComponent<PlayerBanner>();
                banner.Setup(player.Value);
                playerBanners[player.Key] = banner;
            }

            UpdatePlayerDisplay();
            UpdatePhaseBanner();
        }

        public void SetPlayers(NetworkDictionary<PlayerRef, Vermines.Player.PlayerData> playerData)
        {
            players = new Dictionary<int, PlayerData>();
            
            // Set all the data of the player
            foreach (var data in playerData)
            {
                players[data.Value.PlayerRef.PlayerId] = new PlayerData(data.Value);
            }

            playerIds = players.Keys.ToList();

            Initialize();
        }

        public void UpdatePlayers(NetworkDictionary<PlayerRef, Vermines.Player.PlayerData> playerData)
        {
            foreach (var data in playerData)
            {
                players[data.Value.PlayerRef.PlayerId].UpdatePlayerData(data.Value);
            }

            UpdatePlayerBannerData();
        }

        public void UpdateSpecificPlayer(Vermines.Player.PlayerData player)
        {
            players[player.PlayerRef.PlayerId].UpdatePlayerData(player);

            UpdateSpecificPlayerBannerData(player.PlayerRef.PlayerId, players[player.PlayerRef.PlayerId]);
        }

        public void AttemptToNextPhase()
        {
            GameEvents.OnAttemptNextPhase.Invoke();
        }

        public void NextPhase()
        {
            switch (currentPhase)
            {
                case PhaseType.Sacrifice:
                    currentPhase = PhaseType.Gain;
                    break;
                case PhaseType.Gain:
                    currentPhase = PhaseType.Action;
                    break;
                case PhaseType.Action:
                    currentPhase = PhaseType.Resolution;
                    break;
                case PhaseType.Resolution:
                    currentPhase = PhaseType.Sacrifice;
                    NextPlayer();
                    break;
            }

            UpdatePhaseBanner();
        }

        private void UpdatePhaseBanner()
        {
            PhaseBanner phaseBanner = phaseBannerObject.GetComponent<PhaseBanner>();
            phaseBanner.SetPhase(currentPhase);

            UpdateButtonText();
        }

        public void UpdatePhaseButton(bool isInteractable)
        {
            _PhaseButton.GetComponent<Button>().interactable = isInteractable;

            if (!isInteractable)
                buttonText.text = "Wait your turn";
        }

        private void UpdateButtonText()
        {
            if (buttonText != null)
            {
                switch (currentPhase)
                {
                    case PhaseType.Sacrifice:
                        buttonText.text = "Passer à la phase de Gain";
                        break;
                    case PhaseType.Gain:
                        buttonText.text = "Passer à la phase d'Action";
                        break;
                    case PhaseType.Action:
                        buttonText.text = "Passer à la phase de Résolution";
                        break;
                    case PhaseType.Resolution:
                        buttonText.text = "Changer de joueur (Phase de Sacrifice)";
                        break;
                }
            }
        }

        public void NextPlayer()
        {
            float screenWidth = Screen.width;

            int playerId = playerIds[currentPlayerIndex];

            PlayerBanner activeBanner = playerBanners[playerId];
            int nextPlayerIndex = (currentPlayerIndex + 1) % playerBanners.Count;

            float bannerHeight = playerBanners[1].rectTransform.rect.height;

            currentPlayerIndex = nextPlayerIndex;
            UpdatePlayerDisplay();

            activeBanner.rectTransform.SetSiblingIndex(playerBanners.Count - 1);

            /*
            LayoutGroup layoutGroup = playerListParent.GetComponent<LayoutGroup>();
            layoutGroup.enabled = false; // Désactiver le Layout Group temporairement

            // Étape 1 : Sortie du joueur actif
            activeBanner.rectTransform
                .DOLocalMoveX(-screenWidth, 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    // Étape 2 : La liste monte après la sortie
                    for (int i = 0; i < playerBanners.Count; i++)
                    {
                        if (i != currentPlayerIndex) // Ne pas inclure la bannière active qui est sortie
                        {
                            playerBanners[i].rectTransform
                                .DOLocalMoveY(playerBanners[i].rectTransform.localPosition.y + bannerHeight, 0.5f)
                                .SetEase(Ease.OutQuad);
                        }
                    }

                    // Mise à jour de l'index une fois toutes les animations terminées
                    currentPlayerIndex = nextPlayerIndex;
                    UpdatePlayerDisplay();

                    // Replace la bannière active en bas après la montée
                    activeBanner.rectTransform.SetSiblingIndex(playerBanners.Count - 1);
                    //activeBanner.rectTransform.localPosition = new Vector3(screenWidth, -bannerHeight * (playerBanners.Count - 1), 0);

                    layoutGroup.enabled = true;
                    // Étape 3 : Réintroduction du joueur actif
                    //activeBanner.rectTransform
                    //    .DOLocalMoveX(0, 0.5f)
                    //    .SetEase(Ease.OutBack)
                    //    .OnComplete(() =>
                    //    {
                    //    });
                });
            */
        }

        private void UpdatePlayerBannerData()
        {
            foreach (var player in players)
            {
                playerBanners[player.Key].UpdateData(player.Value);
            }
        }

        private void UpdateSpecificPlayerBannerData(int playerKey, PlayerData playerData)
        {
            playerBanners[playerKey].UpdateData(playerData);
        }

        private void UpdatePlayerDisplay()
        {
            int idx = 0;

            foreach (var player in players)
            {
                playerBanners[player.Key].SetSize(idx == currentPlayerIndex);
                idx++;
            }

            //for (int i = 0; i < playerBanners.Count; i++)
            //{
            //    playerBanners[i].SetSize(i == currentPlayerIndex);
            //}
        }

        private void OnValidate()
        {
            if (debugMode)
            {
                Debug.LogWarning("HUDManager: Debug mode is enabled. Make sure to disable it before building the game.");
            }
        }

        public void OpenDeskOverlay()
        {
            deskOverlay.SetActive(true);
        }

        public void CloseDeskOverlay()
        {
            deskOverlay.SetActive(false);
        }
    }
}
