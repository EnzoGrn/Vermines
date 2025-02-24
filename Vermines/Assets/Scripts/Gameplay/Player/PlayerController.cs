using OMGG.DesignPattern;
using Fusion;
using UnityEngine;

namespace Vermines.Player {
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Gameplay.Commands.Deck;
    using Vermines.HUD;
    using Vermines.Network.Utilities;
    using Vermines.ShopSystem.Commands;
    using Vermines.ShopSystem.Enumerations;

    public class PlayerController : NetworkBehaviour {

        public static PlayerController Local { get; private set; }

        public PlayerRef PlayerRef => Object.InputAuthority;

        #region Override Methods

        public override void Spawned()
        {
            name = NetworkNameTools.GiveNetworkingObjectName(Object.InputAuthority, HasInputAuthority, HasStateAuthority);

            if (HasInputAuthority)
                Local = this;
        }

        #endregion

        #region Methods

        public void OnCardSacrified(int cardId)
        {
            GameManager.Instance.RPC_CardSacrified(Object.InputAuthority.RawEncoded, cardId);
        }

        public void OnPlay(int cardId)
        {
            GameManager.Instance.RPC_CardPlayed(Object.InputAuthority.RawEncoded, cardId);
        }

        public void OnDiscard(int cardId)
        {
            GameManager.Instance.RPC_DiscardCard(Object.InputAuthority.RawEncoded, cardId);
        }

        public void OnBuy(ShopType shopType, int slot)
        {
            GameManager.Instance.RPC_BuyCard(shopType, slot, Object.InputAuthority.RawEncoded);
        }

        public void BuyCard(ShopType shopType, int slot)
        {
            RPC_BuyCard(Object.InputAuthority.RawEncoded, shopType, slot);
        }

        #endregion

        #region Player's Commands

        #region Shop Commands

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_BuyCard(int playerRef, ShopType shopType, int slot)
        {
            BuyParameters parameters = new() {
                Decks    = GameDataStorage.Instance.PlayerDeck,
                Player   = PlayerRef.FromEncoded(playerRef),
                Shop     = GameDataStorage.Instance.Shop,
                ShopType = shopType,
                Slot     = slot
            };

            if (parameters.Shop == null)
            {
                Debug.LogError("Shop is null");
            }

            ICommand buyCommand = new BuyCommand(parameters);

            CommandResponse response = CommandInvoker.ExecuteCommand(buyCommand);

            if (response.Status == CommandStatus.Success) {
                if (HUDManager.instance != null)
                    HUDManager.instance.UpdateSpecificPlayer(GameDataStorage.Instance.PlayerData[PlayerRef.FromEncoded(playerRef)]);
                Debug.Log($"[SERVER]: Player {parameters.Player} deck after bought a card : {GameDataStorage.Instance.PlayerDeck[PlayerRef.FromEncoded(playerRef)].Serialize()}");
            }
        }

        #endregion

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_DiscardCard(int playerId, int cardId)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerId);
            ICommand discardCommand = new DiscardCommand(player, cardId);

            CommandResponse response = CommandInvoker.ExecuteCommand(discardCommand);

            if (response.Status == CommandStatus.Success) {
                Debug.Log($"[SERVER]: {response.Message}");

                ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

                foreach (AEffect effect in card.Data.Effects) {
                    if (effect.Type is CardSystem.Enumerations.EffectType.Discard) {
                        effect.Play(player);

                        HUDManager.instance.UpdateSpecificPlayer(GameDataStorage.Instance.PlayerData[player]);
                    }
                }
            } else {
                Debug.LogWarning($"[SERVER]: {response.Message}");
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CardPlayed(int playerId, int cardId)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerId);

            ICommand cardPlayedCommand = new CardPlayedCommand(player, cardId);

            CommandResponse response = CommandInvoker.ExecuteCommand(cardPlayedCommand);

            if (response.Status == CommandStatus.Invalid)
                Debug.LogWarning($"[SERVER]: {response.Message}");
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CardSacrified(int playerId, int cardId)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerId);
            ICard card       = CardSetDatabase.Instance.GetCardByID(cardId);

            if (card == null) {
                Debug.LogError($"[SERVER]: Player {player} tried to sacrify a card that doesn't exist.");

                return;
            }

            ICommand cardSacrifiedCommand = new CardSacrifiedCommand(player, cardId);

            CommandResponse response = CommandInvoker.ExecuteCommand(cardSacrifiedCommand);

            if (response.Status == CommandStatus.Success) {
                ICommand earnCommand = new EarnCommand(player, card.Data.Souls, DataType.Soul);

                response = CommandInvoker.ExecuteCommand(earnCommand);

                if (response.Status == CommandStatus.Success) {
                    GameDataStorage.Instance.PlayerData.TryGet(player, out PlayerData playerData);
                    HUDManager.instance.UpdateSpecificPlayer(playerData);
                }
            } else {
                Debug.LogWarning($"[SERVER]: {response.Message}");
            }
        }

        #endregion
    }
}
