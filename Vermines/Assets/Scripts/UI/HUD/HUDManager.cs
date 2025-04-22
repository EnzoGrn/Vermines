using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

using System.Linq;

namespace Vermines.HUD {

    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Phases;

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
        [SerializeField] private TextMeshProUGUI buttonText; // Texte associ� au bouton
        [SerializeField] private GameObject _PhaseButton;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        //private List<PlayerBanner> playerBanners = new List<PlayerBanner>();
        private int currentPlayerIndex = 0;
        private PhaseType currentPhase = PhaseType.Sacrifice;

        private List<int> playerIds;
        private Dictionary<int, Vermines.Player.PlayerData> players;
        private Dictionary<int, PlayerBanner> playerBanners = new();

        void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);

            deskOverlay.SetActive(false);
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
            players = new Dictionary<int, Vermines.Player.PlayerData>();
            
            // Set all the data of the player
            foreach (var data in playerData)
            {
                players[data.Value.PlayerRef.PlayerId] = new Vermines.Player.PlayerData(data.Value.PlayerRef);
            }

            playerIds = players.Keys.ToList();

            Initialize();
        }

        public void UpdatePlayers(NetworkDictionary<PlayerRef, Vermines.Player.PlayerData> playerData)
        {
            foreach (var data in playerData)
            {
                UpdateSpecificPlayer(data.Value);
            }
        }

        public void UpdateSpecificPlayer(Vermines.Player.PlayerData player)
        {
            players[player.PlayerRef.PlayerId] = player;
            UpdateSpecificPlayerBannerData(player.PlayerRef.PlayerId);
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
                        buttonText.text = "Passer � la phase de Gain";
                        break;
                    case PhaseType.Gain:
                        buttonText.text = "Passer � la phase d'Action";
                        break;
                    case PhaseType.Action:
                        buttonText.text = "Passer � la phase de R�solution";
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
        }

        private void UpdateSpecificPlayerBannerData(int playerKey)
        {
            playerBanners[playerKey].UpdateData(players[playerKey]);
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

        public void OpenDeskOverlay()
        {
            //if (PhaseManager.Instance && PhaseManager.Instance.CurrentPhase == PhaseType.Sacrifice)
            //{
            //    TableManager.instance.EnableDiscard(false);
            //} else
            //{
            //    TableManager.instance.EnableDiscard(true);
            //}
            //deskOverlay.SetActive(true);
        }

        public void CloseDeskOverlay()
        {
            deskOverlay.SetActive(false);
        }

        public void EnablePhaseButton(bool state)
        {
            _PhaseButton.GetComponent<Button>().interactable = state;
        }
    }
}
