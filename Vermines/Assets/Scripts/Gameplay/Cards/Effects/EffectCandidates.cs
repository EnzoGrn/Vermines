using System.Collections.Generic;
using Vermines.CardSystem.Elements;
using Vermines.CardSystem.Enumerations;
using Vermines.Core.Scene;
using Vermines.Player;
using Vermines.ShopSystem.Data;
using Vermines.ShopSystem.Enumerations;

namespace Vermines.Gameplay.Cards.Effect
{

    /// <summary>
    /// Single source of truth for "which cards can this effect target?".
    /// Pure game logic, no UI dependency: effects use it to decide whether
    /// to open a choice at all (and notify the player when there is nothing
    /// to choose), and the choice UI uses it to know what to list.
    /// </summary>
    public static class EffectCandidates
    {

        /// <summary>
        /// Cards of the given type a "remove a card" effect can take from the
        /// player: hand, equipments, played cards, discard.
        /// </summary>
        public static List<ICard> ForRemove(PlayerController player, CardType type)
        {
            List<ICard> result = new();

            AddMatching(result, player.Hand, type);
            AddMatching(result, player.Equipments, type);
            AddMatching(result, player.PlayedCards, type);
            AddMatching(result, player.Discard, type);

            return result;
        }

        /// <summary>
        /// Cards of the given type in the player's graveyard.
        /// </summary>
        public static List<ICard> ForReborn(PlayerController player, CardType type)
        {
            List<ICard> result = new();

            AddMatching(result, player.Graveyard, type);

            return result;
        }

        /// <summary>
        /// Cards a "copy a partisan" effect can copy: every player's played
        /// cards plus the courtyard, excluding the source card itself.
        /// </summary>
        public static List<ICard> ForCopyPartisan(SceneContext context, CardType type, ICard source)
        {
            List<ICard> result = new();

            foreach (PlayerController player in context.Runner.GetAllBehaviours<PlayerController>())
                AddMatching(result, player.PlayedCards, type, source);

            CourtyardSection courtyard = (CourtyardSection)context.GameplayMode.Shop.Sections[ShopType.Courtyard];

            AddMatching(result, courtyard, type, source);

            return result;
        }

        /// <summary>
        /// Cards a "copy a tool" effect can copy: the market, excluding the
        /// source card itself.
        /// </summary>
        public static List<ICard> ForCopyTool(SceneContext context, CardType type, ICard source)
        {
            List<ICard> result = new();

            MarketSection market = (MarketSection)context.GameplayMode.Shop.Sections[ShopType.Market];

            AddMatching(result, market, type, source);

            return result;
        }

        private static void AddMatching(List<ICard> into, IEnumerable<ICard> cards, CardType type, ICard exclude = null)
        {
            foreach (ICard card in cards)
            {
                if (card.Data.Type != type)
                    continue;
                if (exclude != null && card.ID == exclude.ID)
                    continue;
                into.Add(card);
            }
        }
    }
}
