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

            CommandInvoker.ExecuteCommand(buyCommand);

            if (HUDManager.instance != null)
            {
                HUDManager.instance.UpdateSpecificPlayer(GameDataStorage.Instance.PlayerData[PlayerRef.FromEncoded(playerRef)]);
            }

            Debug.Log($"[SERVER]: Player {playerRef} bought a card at slot {slot} in {shopType}");
            // Debug the decks of the player
            Debug.Log($"[SERVER]: Player hand deck: {GameDataStorage.Instance.PlayerDeck[PlayerRef.FromEncoded(playerRef)].Serialize()}");
        }

        #endregion

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_DiscardCard(int playerId, int cardId)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerId);
            ICommand discardCommand = new DiscardCommand(player, cardId);

            CommandInvoker.ExecuteCommand(discardCommand);

            Debug.Log($"[SERVER]: Player {player} discarded card {cardId}");

            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            Debug.Log($"[SERVER]: Card {cardId} is {card.Data.Name}");

            foreach (AEffect effect in card.Data.Effects)
            {
                if (effect.Type is CardSystem.Enumerations.EffectType.Discard)
                {
                    effect.Play(player);
                    Debug.Log("[SERVER]: Effect played");
                    HUDManager.instance.UpdateSpecificPlayer(GameDataStorage.Instance.PlayerData[player]);
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CardPlayed(int playerId, int cardId)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerId);

            ICommand cardPlayedCommand = new CardPlayedCommand(player, cardId);

            CommandInvoker.ExecuteCommand(cardPlayedCommand);

            Debug.Log($"[SERVER]: Player {player} played card {cardId}");
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CardSacrified(int playerId, int cardId)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerId);
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (card == null)
            {
                Debug.LogError($"[SERVER]: Card {cardId} not found");
                return;
            }

            ICommand cardSacrifiedCommand = new CardSacrifiedCommand(player, cardId);

            CommandInvoker.ExecuteCommand(cardSacrifiedCommand);

            if (CommandInvoker.State)
            {
                Debug.Log($"[SERVER]: Player {player} sacrified a card");

                ICommand earnCommand = new EarnCommand(player, card.Data.Souls, DataType.Soul);

                CommandInvoker.ExecuteCommand(earnCommand);

                GameDataStorage.Instance.PlayerData.TryGet(player, out Vermines.Player.PlayerData playerData);
                HUDManager.instance.UpdateSpecificPlayer(playerData);
            }
        }

        #endregion
    }
}
