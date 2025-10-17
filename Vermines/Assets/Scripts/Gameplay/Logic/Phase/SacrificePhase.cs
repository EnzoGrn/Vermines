using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Phases
{

    using Vermines.CardSystem.Elements;
    using Vermines.Gameplay.Phases.Enumerations;
    using Vermines.Player;
    using Vermines.UI;
    using Vermines.UI.Screen;

    public class SacrificePhase : APhase
    {

        #region Type

        public override PhaseType Type => PhaseType.Sacrifice;

        #endregion

        #region Properties

        private int _NumberOfCardSacrified = 0;
        private PlayerRef _CurrentPlayer;

        private Coroutine _sacrificeCoroutine;

        #endregion

        public SacrificePhase() { }

        #region Override Methods

        public override void Run(PlayerRef player)
        {
            PlayerController.Local.StartCoroutine(SacrificeRoutine());

            if (player == PlayerRef.None ||
                PlayerController.Local == null ||
                GameDataStorage.Instance.PlayerDeck == null ||
                !GameDataStorage.Instance.PlayerDeck.TryGetValue(player, out PlayerDeck _))
                return;

            Reset();
            _CurrentPlayer = player;
        }

        private IEnumerator SacrificeRoutine()
        {
            GameplayUIController gameplayUIController = GameObject.FindAnyObjectByType<GameplayUIController>(FindObjectsInactive.Include);

            if (gameplayUIController != null)
                gameplayUIController.Show<GameplayUITurn>();

            yield return new WaitForSeconds(3f);

            if (gameplayUIController != null && gameplayUIController.GetActiveScreen(out GameplayUIScreen activescreen) &&
                activescreen is GameplayUITurn)
                gameplayUIController.Hide();

            if (_CurrentPlayer == PlayerRef.None)
            {
                Debug.LogWarning("[SacrificePhase] CurrentPlayer is None. Ending phase early.");
                OnPhaseEnding(_CurrentPlayer, true);
                yield break;
            }

            if (GameDataStorage.Instance.PlayerDeck == null)
            {
                Debug.LogError("[SacrificePhase] PlayerDeck is null.");
                OnPhaseEnding(_CurrentPlayer, true);
                yield break;
            }

            if (!GameDataStorage.Instance.PlayerDeck.TryGetValue(_CurrentPlayer, out PlayerDeck playerDeck))
            {
                Debug.LogWarning($"[SacrificePhase] No PlayerDeck entry for player {_CurrentPlayer}. Ending phase early.");
                OnPhaseEnding(_CurrentPlayer, true);
                yield break;
            }

            List<ICard> playedCards = playerDeck.PlayedCards;

            GameEvents.OnCardSacrificedRequested.AddListener(OnCardSacrified);

            try
            {
                if (playedCards.Count > 0 && _CurrentPlayer == PlayerController.Local.PlayerRef)
                {
                    CamManager camera = Object.FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);
                    if (camera != null)
                        camera.GoOnSacrificeLocation();

                    while (_NumberOfCardSacrified < GameManager.Instance.SettingsData.MaxSacrificesPerTurn &&
                           GameDataStorage.Instance.PlayerDeck.TryGetValue(_CurrentPlayer, out var currentDeck) &&
                           currentDeck.PlayedCards.Count > 0)
                    {
                        yield return null;
                    }

                    CamManager cam = Object.FindFirstObjectByType<CamManager>(FindObjectsInactive.Include);
                    if (cam != null)
                        cam.GoOnNoneLocation();

                    OnPhaseEnding(_CurrentPlayer, true);
                }
                else
                {
                    OnPhaseEnding(_CurrentPlayer, true);
                }
            }
            finally
            {
                GameEvents.OnCardSacrificedRequested.RemoveListener(OnCardSacrified);
            }
        }


        public override void Reset()
        {
            _NumberOfCardSacrified = 0;
            _CurrentPlayer = PlayerRef.None;
        }

        public override void OnPhaseEnding(PlayerRef player, bool logic = false)
        {
            base.OnPhaseEnding(player, logic);

            if (_sacrificeCoroutine != null && PlayerController.Local != null)
            {
                PlayerController.Local.StopCoroutine(_sacrificeCoroutine);
                _sacrificeCoroutine = null;
            }

            GameEvents.OnCardSacrificedRequested.RemoveListener(OnCardSacrified);
        }

        #endregion

        #region Events

        public void OnCardSacrified(ICard cardSacrified)
        {
            if (Type != PhaseType.Sacrifice)
                return;
            if (_CurrentPlayer != PlayerController.Local.PlayerRef)
                return;
            if (_NumberOfCardSacrified >= GameManager.Instance.SettingsData.MaxSacrificesPerTurn)
                return;

            Debug.Log("[Client]: Card Sacrified");

            int cardId = cardSacrified.ID;
            ICard card = GameDataStorage.Instance.PlayerDeck[_CurrentPlayer].PlayedCards.Find(c => c.ID == cardId);

            if (card != null)
            {
                PlayerController.Local.OnCardSacrified(card.ID);
                _NumberOfCardSacrified++;
            }
            else
            {
                Debug.LogWarning($"[Client]: Card {cardId} not found in played cards.");
                GameEvents.OnCardSacrifiedRefused.Invoke(cardSacrified);
            }
        }

        #endregion
    }
}
