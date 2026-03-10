using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Vermines.Gameplay.Core {
    using System.Linq;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Core;
    using Vermines.Player;
    using Vermines.ShopSystem.Enumerations;

    public partial class StandartGameplay : GameplayMode {

        #region Methods

        public override void Spawned()
        {
            base.Spawned();

            if (HasStateAuthority)
                _Initializer = new StandartGameInitializer(this);
        }

        protected override void CheckWinCondition()
        {
            List<PlayerController> players = Context.Runner.GetAllBehaviours<PlayerController>();

            if (Context.NetworkGame == null || players == null)
                return;
            if (EndGameTriggerPlayer == PlayerRef.None) {
                foreach (PlayerController player in players) {
                    if (player == null)
                        continue;
                    if (player.Statistics.Souls >= SoulsLimit) {
                        EndGameTriggerPlayer = player.Object.InputAuthority;
                        EndGameTriggerTurn   = TotalTurnPlayed;

                        Debug.Log($"End game triggered by {player.Nickname}");

                        return;
                    }
                }
            }
        }

        protected override void FixedUpdateNetwork_Active()
        {
            base.FixedUpdateNetwork_Active();

            CheckWinCondition();

            if (EndGameTriggerPlayer != PlayerRef.None) {
                if (CurrentPlayer == EndGameTriggerPlayer && TotalTurnPlayed > EndGameTriggerTurn) {
                    List<PlayerController> players = Context.Runner.GetAllBehaviours<PlayerController>();

                    PlayerController winner = players.OrderByDescending(p => p.Statistics.Souls).ThenByDescending(p => p.Object.InputAuthority == EndGameTriggerPlayer).First();

                    FinishGameplay(winner);
                }
            }
        }

        #endregion

        #region Initializer

        protected override void OnInitializeCards(List<CardFamily> families)
        {
            CardSetDatabase.Instance.Initialize(families, Context);
        }

        protected override void OnInitializeShop(ShopType shopType, string shopData)
        {
            Shop.DeserializeSection(shopType, shopData);

            GameEvents.OnShopRefilled.Invoke(shopType, Shop.GetDisplayCards(shopType));
        }

        #endregion
    }
}
