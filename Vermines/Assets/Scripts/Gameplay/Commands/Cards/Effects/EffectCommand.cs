using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;
    using Vermines.CardSystem.Data.Effect;

    public class ADMIN_CheckEffectCommand : ACommand {

        private readonly PlayerRef _Player;

        private readonly ICard _Card;
        private readonly int _CardId;

        public ADMIN_CheckEffectCommand(PlayerRef player, int cardId)
        {
            _Player = player;
            _Card   = CardSetDatabase.Instance.GetCardByID(cardId);
            _CardId = cardId;
        }

        public override CommandResponse Execute()
        {
            // 0. Check if the card exist.
            if (_Card == null)
                return new CommandResponse(CommandStatus.CriticalError, "CardNotExist", _CardId.ToString());

            // 1. Check if the card is own by the player.
            if (_Card.Owner != PlayerRef.None && _Card.Owner != _Player)
                return new CommandResponse(CommandStatus.CriticalError, "CardWrongOwner", _Card.Data.Name);
            return new CommandResponse(CommandStatus.Success, "");
        }
    }

    public class ADMIN_CheckChosenEffectCommand : ACommand {

        private readonly PlayerRef _Player;

        private readonly ICard _Card;
        private readonly int _CardId;

        private AEffect _Effect;
        private readonly int _EffectIndex;

        public ADMIN_CheckChosenEffectCommand(PlayerRef player, int cardId, int effectIndex)
        {
            _Player      = player;
            _Card        = CardSetDatabase.Instance.GetCardByID(cardId);
            _CardId      = cardId;
            _EffectIndex = effectIndex;
        }

        public override CommandResponse Execute()
        {
            // 0. Check if the card exists.
            if (_Card == null)
                return new CommandResponse(CommandStatus.CriticalError, "CardNotExist", _CardId.ToString());

            // 1. Check if the card is own by the player
            if (_Card.Owner != PlayerRef.None && _Card.Owner != _Player)
                return new CommandResponse(CommandStatus.CriticalError, "CardWrongOwner", _Card.Data.Name);

            // 2. Check the effect exists.
            if (_EffectIndex > _Card.Data.Effects.Count)
                return new CommandResponse(CommandStatus.CriticalError, "EffectNotExist", _CardId.ToString(), _EffectIndex.ToString());
            _Effect = _Card.Data.Effects[_EffectIndex];

            if (_Effect == null)
                return new CommandResponse(CommandStatus.CriticalError, "EffectNotExist", _CardId.ToString(), _EffectIndex.ToString());

            // 3. Vérifier que la carte à un effet à choix.
            if (!_Card.Data.HasChoiceEffect())
                return new CommandResponse(CommandStatus.CriticalError, "NotAChoiceEffect", _CardId.ToString(), _EffectIndex.ToString());
            return new CommandResponse(CommandStatus.Success, "");
        }
    }
}
