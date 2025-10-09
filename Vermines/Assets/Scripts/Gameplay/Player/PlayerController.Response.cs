using OMGG.DesignPattern;
using OMGG.Chronicle;
using UnityEngine;
using Fusion;

namespace Vermines.Player {

    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Commands;
    using Vermines.ShopSystem.Commands;
    using Vermines.ShopSystem.Enumerations;
    using Vermines.ShopSystem;
    using Vermines.UI.Card;

    public partial class PlayerController : NetworkBehaviour {

        #region Shop

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_BuyCard(int playerSRef, NetworkChronicleEntry nEntry, ShopType sType, int slot)
        {
            PlayerRef playerSource = PlayerRef.FromEncoded(playerSRef);

            // The state authority already process the command on his side. So ignore it.
            if (!HasStateAuthority) {
                ShopArgs parameters = new(GameDataStorage.Instance.Shop, sType, slot);
                ICommand buyCommand = new CLIENT_BuyCommand(playerSource, parameters);

                CommandInvoker.ExecuteCommand(buyCommand);
            }

            GameEvents.OnCardPurchased.Invoke(sType, slot);

            AddChronicle(nEntry.ToChronicleEntry());
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ReplaceCardInShop(int playerSRef, NetworkChronicleEntry nEntry, ShopType shopType, int slot)
        {
            ICommand replaceCommand = new CLIENT_ChangeCardCommand(new ShopArgs(GameDataStorage.Instance.Shop, shopType, slot));

            CommandInvoker.ExecuteCommand(replaceCommand);

            GameEvents.OnShopRefilled.Invoke(shopType, GameDataStorage.Instance.Shop.Sections[shopType].AvailableCards);

            AddChronicle(nEntry.ToChronicleEntry());
        }

        #endregion

        #region Sacrifice

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CardSacrified(int playerSRef, NetworkChronicleEntry nEntry, int cardId)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerSRef);
            ICard       card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (!HasStateAuthority) {
                ICommand cardSacrifiedCommand = new CLIENT_CardSacrifiedCommand(player, cardId);

                CommandInvoker.ExecuteCommand(cardSacrifiedCommand);

                foreach (AEffect effect in card.Data.Effects) {
                    if ((effect.Type & EffectType.Sacrifice) != 0)
                        effect.Play(player);
                    else if ((effect.Type & EffectType.Passive) != 0)
                        effect.Stop(player);
                }

                foreach (ICard playedCard in GameDataStorage.Instance.PlayerDeck[player].PlayedCards) {
                    if (playedCard.Data.Effects != null) {
                        foreach (AEffect effect in playedCard.Data.Effects) {
                            if ((effect.Type & EffectType.OnOtherSacrifice) != 0)
                                effect.Play(player);
                        }
                    }
                }
            }

            GameEvents.OnCardSacrified.Invoke(card);

            AddChronicle(nEntry.ToChronicleEntry());
        }

        #endregion

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_DiscardCard(int playerId, int cardId, bool hasEffect = true)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerId);

            ICommand discardCommand = new CLIENT_DiscardCommand(player, cardId);

            CommandInvoker.ExecuteCommand(discardCommand);

            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            GameEvents.OnPlayerUpdated.Invoke(GameDataStorage.Instance.PlayerData[player]);
            HandManager.Instance.RemoveCard(card);

            if (hasEffect) {
                foreach (AEffect effect in card.Data.Effects) {
                    if ((effect.Type & EffectType.Discard) != 0)
                        effect.Play(player);
                }
            } else {
                if (Local.PlayerRef == player) {
                    DiscardDropHandler discardDropHandler = FindFirstObjectByType<DiscardDropHandler>();

                    if (discardDropHandler != null)
                        discardDropHandler.SetLatestDiscardedCard(card);
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CardPlayed(int playerId, int cardId)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerId);

            ICommand cardPlayedCommand = new CLIENT_PlayCommand(player, cardId);

            CommandResponse response = CommandInvoker.ExecuteCommand(cardPlayedCommand);

            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (response.Status == CommandStatus.Invalid)
            {
                Debug.LogWarning($"[SERVER]: {response.Message}");
                GameEvents.OnCardPlayedRefused.Invoke(card);
            }
            else if (response.Status == CommandStatus.Success)
            {
                GameEvents.OnCardPlayed.Invoke(card);

                foreach (AEffect effect in card.Data.Effects)
                    effect.OnAction("Play", player, card);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ActivateEffect(int playerID, int cardID)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerID);
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);

            foreach (AEffect effect in card.Data.Effects)
            {
                if ((effect.Type & EffectType.Activate) != 0)
                    effect.Play(player);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ReducedInSilenced(int playerId, int cardId)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (card == null)
            {
                Debug.LogError($"[SERVER]: Player {playerId} tried to reduce in silence a card that doesn't exist.");

                return;
            }
            ICommand command = new ReducedInSilenceCommand(card);

            CommandResponse response = CommandInvoker.ExecuteCommand(command);

            // TODO: Maybe update the card UI ?
            // TODO: Maybe update the card UI ?
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RemoveReducedInSilenced(int playerId, int cardID, int originalSouls)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);

            if (card == null)
            {
                Debug.LogError($"[SERVER]: Player {playerId} tried to remove the reduction in silence of a card that doesn't exist.");

                return;
            }
            ICommand command = new RemoveReducedInSilenceCommand(card, originalSouls);

            CommandResponse response = CommandInvoker.ExecuteCommand(command);

            // TODO: Maybe update the card UI ?
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CopiedEffect(int playerId, NetworkChronicleEntry nEntry, int cardID, int cardToCopiedID)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);
            ICard cardToCopied = CardSetDatabase.Instance.GetCardByID(cardToCopiedID);

            if (card == null || cardToCopied == null)
            {
                Debug.LogError($"[SERVER]: Player {playerId} tried to copy an effect of a card that doesn't exist.");

                return;
            }

            card.Data.CopyEffect(cardToCopied.Data.Effects);

            foreach (AEffect effect in card.Data.Effects)
            {
                if ((effect.Type & EffectType.Sacrifice) != 0 || (effect.Type & EffectType.OnOtherSacrifice) != 0)
                    return;
                effect.Play(PlayerRef.FromEncoded(playerId));
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RemoveCopiedEffect(int playerId, int cardID)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);

            if (card == null)
            {
                Debug.LogError($"[SERVER]: Player {playerId} tried to removed a copied effect of a card that doesn't exist.");

                return;
            }

            card.Data.RemoveEffectCopied();

            // TODO: Maybe update the card UI, of the card (effect)
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_NetworkEventCardEffect(int playerID, int cardID, string data)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);
            PlayerRef player = PlayerRef.FromEncoded(playerID);

            foreach (AEffect effect in card.Data.Effects)
                effect.NetworkEventFunction(player, data);
        }
    }
}
