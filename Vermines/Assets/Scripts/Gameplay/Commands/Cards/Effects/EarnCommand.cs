using OMGG.DesignPattern;

namespace Vermines.Gameplay.Commands.Cards.Effects {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Player;

    public class EarnCommand : ICommand {

        private PlayerController _Player;

        private readonly int       _Amount;
        private readonly DataType  _DataType;

        public EarnCommand(PlayerController player, int amount, DataType dataType)
        {
            _Player   = player;
            _Amount   = amount;
            _DataType = dataType;
        }

        public CommandResponse Execute()
        {
            if (_DataType == DataType.Eloquence)
                _Player.SetEloquence(_Player.Statistics.Eloquence + _Amount);
            else if (_DataType == DataType.Soul)
                _Player.SetSouls(_Player.Statistics.Souls + _Amount);
            return new CommandResponse(CommandStatus.Success, $"Player {_Player.Object.InputAuthority} earned {_Amount} {_DataType}.");
        }

        public void Undo()
        {
            // TODO: Fix that (in case the player have 19 and win 5, it will be at 20, but if we undo he will currently be at 15 and not 19)
            if (_DataType == DataType.Eloquence)
                _Player.SetEloquence(_Player.Statistics.Eloquence - _Amount);
            else if (_DataType == DataType.Soul)
                _Player.SetSouls(_Player.Statistics.Souls - _Amount);
        }
    }
}
