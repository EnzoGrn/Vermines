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
    using Vermines.UI;
    using Vermines.UI.Screen;
    using Vermines.Core.Player;
    using Vermines.Core;

    public partial class PlayerController : ContextBehaviour, IPlayer {

        #region Shop

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_BuyCard(NetworkChronicleEntry nEntry, ShopType sType, int cardId)
        {
            // The state authority already process the command on his side. So ignore it.
            if (!HasStateAuthority) {
                ShopArgs parameters = new(Context.GameplayMode.Shop, sType, cardId);
                ICommand buyCommand = new CLIENT_BuyCommand(this, parameters);

                CommandInvoker.ExecuteCommand(buyCommand);
            }

            GameEvents.OnCardPurchased.Invoke(sType, cardId);

            AddChronicle(nEntry.ToChronicleEntry());
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ReplaceCardInShop(NetworkChronicleEntry nEntry, ShopType shopType, int cardId)
        {
            ICommand replaceCommand = new CLIENT_ChangeCardCommand(new ShopArgs(Context.GameplayMode.Shop, shopType, cardId));

            CommandInvoker.ExecuteCommand(replaceCommand);

            GameEvents.OnShopRefilled.Invoke(shopType, Context.GameplayMode.Shop.GetDisplayCards(shopType));

            AddChronicle(nEntry.ToChronicleEntry());
        }

        #endregion

        #region Sacrifice

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CardSacrified(NetworkChronicleEntry nEntry, int cardId)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (!HasStateAuthority) {
                ICommand cardSacrifiedCommand = new CLIENT_CardSacrifiedCommand(this, cardId);

                CommandInvoker.ExecuteCommand(cardSacrifiedCommand);

                foreach (AEffect effect in card.Data.Effects) {
                    if ((effect.Type & EffectType.Sacrifice) != 0)
                        effect.Play(Object.InputAuthority);
                    else if ((effect.Type & EffectType.Passive) != 0)
                        effect.Stop(Object.InputAuthority);
                }

                foreach (ICard playedCard in Deck.PlayedCards) {
                    if (playedCard.Data.Effects != null) {
                        foreach (AEffect effect in playedCard.Data.Effects) {
                            if ((effect.Type & EffectType.OnOtherSacrifice) != 0)
                                effect.Play(Object.InputAuthority);
                        }
                    }
                }
            }

            GameEvents.OnCardSacrified.Invoke(card);

            AddChronicle(nEntry.ToChronicleEntry());
        }

        #endregion

        #region Recycle

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CardRecycled(NetworkChronicleEntry nEntry, int cardId)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (!HasInputAuthority) {
                ICommand cardSacrifiedCommand = new CLIENT_CardRecycleCommand(this, cardId, Context.GameplayMode.Shop.Sections[ShopType.Market]);

                CommandInvoker.ExecuteCommand(cardSacrifiedCommand);
            }

            foreach (AEffect effect in card.Data.Effects) {
                if ((effect.Type & EffectType.Recycle) != 0)
                    effect.Play(Object.InputAuthority);
            }

            foreach (ICard playedCard in Deck.PlayedCards) {
                if (playedCard.Data.Effects != null) {
                    foreach (AEffect effect in playedCard.Data.Effects) {
                        if ((effect.Type & EffectType.OnOtherRecycle) != 0)
                            effect.Play(Object.InputAuthority);
                    }
                }
            }

            GameEvents.OnCardRecycled.Invoke(card);

            AddChronicle(nEntry.ToChronicleEntry());
        }

        #endregion

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_DiscardCard(int cardId, bool hasEffect = true)
        {
            ICommand discardCommand = new CLIENT_DiscardCommand(this, cardId);

            CommandInvoker.ExecuteCommand(discardCommand);

            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            GameEvents.OnPlayerUpdated.Invoke(this);

            Context.HandManager.RemoveCard(card);

            if (!hasEffect) {
                if (Object.InputAuthority == Context.Runner.LocalPlayer) {
                    DiscardDropHandler discardDropHandler = FindFirstObjectByType<DiscardDropHandler>();

                    if (discardDropHandler != null)
                        discardDropHandler.SetLatestDiscardedCard(card);
                }

                return;
            }

            if (card.Data.HasChoiceEffect(EffectType.Discard)) {
                if (Object.InputAuthority == Context.Runner.LocalPlayer) {
                    GameplayUIController uiController = FindFirstObjectByType<GameplayUIController>();

                    if (uiController != null)
                        uiController.ShowWithParams<GameplayUIChoiceEffect, ICard>(card);
                }
            } else {
                foreach (AEffect effect in card.Data.Effects) {
                    if ((effect.Type & EffectType.Discard) != 0)
                        effect.Play(Object.InputAuthority);
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CardPlayed(int cardId)
        {
            ICommand cardPlayedCommand = new CLIENT_PlayCommand(this, cardId);

            CommandResponse response = CommandInvoker.ExecuteCommand(cardPlayedCommand);

            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (response.Status == CommandStatus.Invalid) {
                Debug.LogWarning($"[SERVER]: {response.Message}");

                GameEvents.OnCardPlayedRefused.Invoke(card);
            } else if (response.Status == CommandStatus.Success) {
                GameEvents.OnCardPlayed.Invoke(card);

                foreach (AEffect effect in card.Data.Effects)
                    effect.OnAction("Play", Object.InputAuthority, card);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ActivateEffect(int cardID)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);

            foreach (AEffect effect in card.Data.Effects) {
                if ((effect.Type & EffectType.Activate) != 0)
                    effect.Play(Object.InputAuthority);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ReducedInSilenced(int cardId)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (card == null) {
                Debug.LogError($"[SERVER]: Player {Object.InputAuthority} tried to reduce in silence a card that doesn't exist.");

                return;
            }

            ICommand command = new ReducedInSilenceCommand(card);

            CommandInvoker.ExecuteCommand(command);

            // TODO: Maybe update the card UI ?
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RemoveReducedInSilenced(int cardID, int originalSouls)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);

            if (card == null) {
                Debug.LogError($"[SERVER]: Player {Object.InputAuthority} tried to remove the reduction in silence of a card that doesn't exist.");

                return;
            }

            ICommand command = new RemoveReducedInSilenceCommand(card, originalSouls);

            CommandInvoker.ExecuteCommand(command);

            // TODO: Maybe update the card UI ?
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CopiedEffect(NetworkChronicleEntry nEntry, int cardID, int cardToCopiedID)
        {
            ICard card         = CardSetDatabase.Instance.GetCardByID(cardID);
            ICard cardToCopied = CardSetDatabase.Instance.GetCardByID(cardToCopiedID);

            if (card == null || cardToCopied == null) {
                Debug.LogError($"[SERVER]: Player {Object.InputAuthority} tried to copy an effect of a card that doesn't exist.");

                return;
            }

            card.Data.CopyEffect(cardToCopied.Data.Effects);

            foreach (AEffect effect in card.Data.Effects) {
                if ((effect.Type & EffectType.Sacrifice) != 0 || (effect.Type & EffectType.OnOtherSacrifice) != 0)
                    return;
                effect.Play(Object.InputAuthority);
            }

            AddChronicle(nEntry.ToChronicleEntry());
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RemoveCopiedEffect(int cardID)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);

            if (card == null) {
                Debug.LogError($"[SERVER]: Player {Object.InputAuthority} tried to removed a copied effect of a card that doesn't exist.");

                return;
            }

            card.Data.RemoveEffectCopied();

            // TODO: Maybe update the card UI, of the card (effect)
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_NetworkEventCardEffect(int cardID, string data)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);

            foreach (AEffect effect in card.Data.Effects)
                effect.NetworkEventFunction(Object.InputAuthority, data);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_EffectChosen(int cardId, int effectIndex)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (Object.InputAuthority == Context.Runner.LocalPlayer) {
                GameplayUIController uiController = FindFirstObjectByType<GameplayUIController>();

                if (uiController != null)
                    uiController.Hide<GameplayUIChoiceEffect>();
            }

            card.Data.Effects[effectIndex].Play(Object.InputAuthority);
        }
    }
}
