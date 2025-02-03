using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Vermines.CardSystem.Enumerations;
using UnityEngine.UI;
using TMPro;

namespace Vermines.HUD
{
    /// <summary>
    /// TODO:
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
    }

    /// <summary>
    /// 
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance;

        [SerializeField] private Transform playerListParent; // Le parent contenant les bannières
        [SerializeField] private GameObject playerBannerPrefab;
        [SerializeField] private GameObject phaseBannerObject;
        [SerializeField] private GameObject deskOverlay;
        [SerializeField] private GameObject marketOverlay;
        [SerializeField] private TextMeshProUGUI buttonText; // Texte associé au bouton
        [SerializeField] private bool debugMode = false;

        private List<PlayerBanner> playerBanners = new List<PlayerBanner>();
        private int currentPlayerIndex = 0;
        private PhaseType currentPhase = PhaseType.Sacrifice; // Phase initiale

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            deskOverlay.SetActive(false);
            marketOverlay.SetActive(false);

            if (debugMode)
            {
                List<PlayerData> players = new List<PlayerData>
                {
                    new PlayerData { Eloquence = 20, Souls = 100, Nickname = "Player 1", Family = CardFamily.None },
                    new PlayerData { Eloquence = 20, Souls = 100, Nickname = "Player 2", Family = CardFamily.None },
                    new PlayerData { Eloquence = 20, Souls = 100, Nickname = "Player 3", Family = CardFamily.None },
                    new PlayerData { Eloquence = 20, Souls = 100, Nickname = "Player 4", Family = CardFamily.None }
                };

                Initialize(players);
            }
        }

        public void Initialize(List<PlayerData> players) // TODO: Replace Player with the actual Player class
        {
            foreach (var player in players)
            {
                GameObject bannerObject = Instantiate(playerBannerPrefab, playerListParent);
                PlayerBanner banner = bannerObject.GetComponent<PlayerBanner>();
                banner.Setup(player);
                playerBanners.Add(banner);
            }

            UpdatePlayerDisplay();
            UpdatePhaseBanner();
        }

        // Appelée lorsqu'on clique sur le bouton
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
                    NextPlayer(); // Change de joueur
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
            PlayerBanner activeBanner = playerBanners[currentPlayerIndex];
            int nextPlayerIndex = (currentPlayerIndex + 1) % playerBanners.Count;

            float bannerHeight = playerBanners[1].rectTransform.rect.height;

            // Mise à jour de l'index une fois toutes les animations terminées
            currentPlayerIndex = nextPlayerIndex;
            UpdatePlayerDisplay();

            // Replace la bannière active en bas après la montée
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

        private void UpdatePlayerDisplay()
        {
            for (int i = 0; i < playerBanners.Count; i++)
            {
                playerBanners[i].SetSize(i == currentPlayerIndex);
            }
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

        public void OpenMarketOverlay()
        {
            marketOverlay.SetActive(true);
        }

        public void CloseMarketOverlay()
        {
            marketOverlay.SetActive(false);
        }
    }
}
