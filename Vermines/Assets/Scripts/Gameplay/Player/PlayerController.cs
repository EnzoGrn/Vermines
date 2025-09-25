using OMGG.DesignPattern;
using Fusion;
using UnityEngine;

namespace Vermines.Player {

    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;
    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Enumerations;
    using Vermines.Gameplay.Cards;
    using Vermines.Gameplay.Commands;
    using Vermines.Gameplay.Commands.Cards.Effects;
    using Vermines.Gameplay.Commands.Deck;
    using Vermines.Menu.Screen;
    using Vermines.Network.Utilities;
    using Vermines.Service;
    using Vermines.ShopSystem;
    using Vermines.ShopSystem.Commands;
    using Vermines.ShopSystem.Enumerations;

    public partial class PlayerController : NetworkBehaviour {

        public static PlayerController Local { get; private set; }

        public PlayerRef PlayerRef => Object.InputAuthority;

        #region Override Methods

        public override void Spawned()
        {
            name = NetworkNameTools.GiveNetworkingObjectName(Object.InputAuthority, HasInputAuthority, HasStateAuthority);

            if (HasInputAuthority)
                Local = this;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (HasInputAuthority && Local == this)
                Local = null;
        }

        #endregion

        #region Methods

        public void Update()
        {
            if (!HasInputAuthority)
                return;
            if (Input.GetKeyDown(KeyCode.Escape)) {
                VMUI_Gameplay gameplay = FindFirstObjectByType<VMUI_Gameplay>();
                
                if (gameplay)
                    gameplay.TogglePauseMenu();
            }
        }

        public void ReturnToMenu()
        {
            VerminesPlayerService services = FindFirstObjectByType<VerminesPlayerService>(FindObjectsInactive.Include);

            if (services.IsCustomGame())
                RPC_ForceQuitCustomGame(PlayerRef);
            else
                RPC_ForceQuit(PlayerRef);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public async void RPC_ForceQuitCustomGame(PlayerRef player)
        {
            GameManager manager = FindFirstObjectByType<GameManager>();

            if (!manager)
                return;
            await manager.ForceEndCustomGame(player);
        }


        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public async void RPC_ForceQuit(PlayerRef player)
        {
            GameManager manager = FindFirstObjectByType<GameManager>();

            if (!manager)
                return;
            await manager.ReturnToTavern();
        }

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
            GameManager.Instance.RPC_DiscardCard(Object.InputAuthority.RawEncoded, cardId, true);
        }

        public void OnDiscardNoEffect(int cardId)
        {
            GameManager.Instance.RPC_DiscardCard(Object.InputAuthority.RawEncoded, cardId, false);
        }

        public void OnBuy(ShopType shopType, int slot)
        {
            GameManager.Instance.RPC_BuyCard(shopType, slot, Object.InputAuthority.RawEncoded);
        }

        public void OnActiveEffectActivated(int cardID)
        {
            GameManager.Instance.RPC_ActivateEffect(Object.InputAuthority.RawEncoded, cardID);
        }

        public void OnShopReplaceCard(ShopType shopType, int slot)
        {
            GameManager.Instance.RPC_ReplaceCardInShop(Object.InputAuthority.RawEncoded, shopType, slot);
        }

        public void OnReducedInSilenced(ICard cardToBeSilenced)
        {
            GameManager.Instance.RPC_ReducedInSilenced(Object.InputAuthority.RawEncoded, cardToBeSilenced.ID);
        }

        public void RemoveReducedInSilenced(ICard card, int originalSouls)
        {
            GameManager.Instance.RPC_RemoveReducedInSilenced(Object.InputAuthority.RawEncoded, card.ID, originalSouls);
        }

        public void CopiedEffect(ICard card, ICard cardCopied)
        {
            GameManager.Instance.RPC_CopiedEffect(Object.InputAuthority.RawEncoded, card.ID, cardCopied.ID);
        }

        public void RemoveCopiedEffect(ICard card)
        {
            GameManager.Instance.RPC_RemoveCopiedEffect(Object.InputAuthority.RawEncoded, card.ID);
        }

        public void NetworkEventCardEffect(int cardID, string data = "")
        {
            GameManager.Instance.RPC_NetworkEventCardEffect(Object.InputAuthority.RawEncoded, cardID, data);
        }

        #endregion

        #region Player's Commands

        #region Shop Commands

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_BuyCard(int playerRef, ShopType shopType, int slot)
        {
            // The state authority already process the command on his side. So ignore it.
            if (!HasStateAuthority) {
                PlayerRef playerSource = PlayerRef.FromEncoded(playerRef);

                ShopArgs parameters = new(GameDataStorage.Instance.Shop, shopType, slot);
                ICommand buyCommand = new CLIENT_BuyCommand(playerSource, parameters);

                CommandInvoker.ExecuteCommand(buyCommand);

                Debug.Log($"[SERVER]: {CommandInvoker.State.Message}");
            }

            GameEvents.OnCardPurchased.Invoke(shopType, slot);
        }

        #endregion

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_DiscardCard(int playerId, int cardId, bool hasEffect = true)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerId);

            ICommand discardCommand = new CLIENT_DiscardCommand(player, cardId);

            CommandResponse response = CommandInvoker.ExecuteCommand(discardCommand);

            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (response.Status == CommandStatus.Success) {
                GameEvents.OnCardDiscarded.Invoke(card);
                GameEvents.OnPlayerUpdated.Invoke(GameDataStorage.Instance.PlayerData[player]);

                if (hasEffect) {
                    foreach (AEffect effect in card.Data.Effects) {
                        if (effect.Type == EffectType.Discard)
                            effect.Play(player);
                    }
                }
            } else {
                if (hasEffect) // If he has an effect, it means that the player played the action by himself, so we need to inform him that the action failed.
                    GameEvents.OnCardDiscardedRefused.Invoke(card);
                Debug.LogWarning($"[SERVER]: {response.Message}");
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
            } if (response.Status == CommandStatus.Success) {
                GameEvents.OnCardPlayed.Invoke(card);

                foreach (AEffect effect in card.Data.Effects)
                    effect.OnAction("Play", player, card);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CardSacrified(int playerId, int cardId)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerId);
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (card == null) {
                Debug.LogError($"[SERVER]: Player {player} tried to sacrify a card that doesn't exist.");
                GameEvents.OnCardSacrifiedRefused.Invoke(card);
                return;
            }

            ICommand cardSacrifiedCommand = new CLIENT_CardSacrifiedCommand(player, cardId);

            CommandResponse response = CommandInvoker.ExecuteCommand(cardSacrifiedCommand);

            if (response.Status == CommandStatus.Success) {
                foreach (AEffect effect in card.Data.Effects) {
                    if (effect.Type == EffectType.Sacrifice)
                        effect.Play(player);
                    else if (effect.Type == EffectType.Passive)
                        effect.Stop(player);
                }

                foreach (ICard playedCard in GameDataStorage.Instance.PlayerDeck[player].PlayedCards) {
                    if (playedCard.Data.Effects != null) {
                        foreach (AEffect effect in playedCard.Data.Effects) {
                            if (effect.Type == EffectType.OnOtherSacrifice)
                                effect.Play(player);
                        }
                    }
                }

                int soulToEarn = card.Data.Souls;

                if (card.Data.Type == CardType.Partisan && card.Data.Family == GameDataStorage.Instance.PlayerData[player].Family)
                    soulToEarn += GameManager.Instance.SettingsData.BonusSoulInFamilySacrifice;
                ICommand earnCommand = new EarnCommand(player, soulToEarn, DataType.Soul);

                response = CommandInvoker.ExecuteCommand(earnCommand);

                if (response.Status == CommandStatus.Success) {
                    GameEvents.OnCardSacrified.Invoke(card);
                }
            } else {
                Debug.LogWarning($"[SERVER]: {response.Message}");
                GameEvents.OnCardSacrifiedRefused.Invoke(card);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ActivateEffect(int playerID, int cardID)
        {
            PlayerRef player = PlayerRef.FromEncoded(playerID);
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);

            if (card == null) {
                Debug.LogError($"[SERVER]: Player {player} tried to activate an effect that doesn't exist.");

                return;
            }

            foreach (AEffect effect in card.Data.Effects) {
                if (effect.Type == EffectType.Activate)
                    effect.Play(player);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ReplaceCardInShop(int playerID, ShopType shopType, int slot)
        {
            ICommand replaceCommand = new CLIENT_ChangeCardCommand(new ShopArgs(GameDataStorage.Instance.Shop, shopType, slot));

            CommandInvoker.ExecuteCommand(replaceCommand);

            GameEvents.OnShopRefilled.Invoke(shopType, GameDataStorage.Instance.Shop.Sections[shopType].AvailableCards);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_ReducedInSilenced(int playerId, int cardId)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardId);

            if (card == null) {
                Debug.LogError($"[SERVER]: Player {playerId} tried to reduce in silence a card that doesn't exist.");

                return;
            }
            ICommand command = new ReducedInSilenceCommand(card);

            CommandResponse response = CommandInvoker.ExecuteCommand(command);

            // TODO: Maybe update the card UI ?
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RemoveReducedInSilenced(int playerId, int cardID, int originalSouls)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);

            if (card == null) {
                Debug.LogError($"[SERVER]: Player {playerId} tried to remove the reduction in silence of a card that doesn't exist.");

                return;
            }
            ICommand command = new RemoveReducedInSilenceCommand(card, originalSouls);

            CommandResponse response = CommandInvoker.ExecuteCommand(command);

            // TODO: Maybe update the card UI ?
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_CopiedEffect(int playerId, int cardID, int cardToCopiedID)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);
            ICard cardToCopied = CardSetDatabase.Instance.GetCardByID(cardToCopiedID);

            if (card == null || cardToCopied == null) {
                Debug.LogError($"[SERVER]: Player {playerId} tried to copy an effect of a card that doesn't exist.");

                return;
            }

            card.Data.CopyEffect(cardToCopied.Data.Effects);

            foreach (AEffect effect in card.Data.Effects) {
                if (effect.Type == EffectType.Sacrifice || effect.Type == EffectType.OnOtherSacrifice)
                    return;
                effect.Play(PlayerRef.FromEncoded(playerId));
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_RemoveCopiedEffect(int playerId, int cardID)
        {
            ICard card = CardSetDatabase.Instance.GetCardByID(cardID);

            if (card == null) {
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

        #endregion
    }
}
