using System.Collections.Generic;
using OMGG.DesignPattern;

namespace Vermines.Gameplay.Core {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Core;
    using Vermines.Player;
    using Vermines.ShopSystem.Commands;
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
            foreach (PlayerController player in Context.NetworkGame.ActivePlayers) {
                if (player == null)
                    continue;
                if (player.Statistics.Souls >= SoulsLimit) {
                    FinishGameplay();

                    return;
                }
            }
        }

        protected override void FixedUpdateNetwork_Active()
        {
            base.FixedUpdateNetwork_Active();

            CheckWinCondition();
        }

        #endregion

        #region Initializer

        protected override void OnInitializeCards(List<CardFamily> families)
        {
            CardSetDatabase.Instance.Initialize(families, Context);
        }

        protected override void OnInitializeShop(ShopType shopType, string shopData)
        {
            ICommand fillCommand = new FillShopCommand(Shop);

            Shop.DeserializeSection(shopType, shopData);

            CommandInvoker.ExecuteCommand(fillCommand);
        }

        #endregion
    }
}
