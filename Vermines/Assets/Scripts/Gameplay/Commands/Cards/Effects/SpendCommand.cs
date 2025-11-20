using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands.Cards.Effects {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Core.Player;
    using Vermines.Player;

    public class SpendCommand : ICommand {

        private PlayerController _Player;

        private readonly int       _Amount;
        private readonly DataType  _DataType;

        public SpendCommand(PlayerController player, int amount, DataType dataType)
        {
            _Player   = player;
            _Amount   = amount;
            _DataType = dataType;
        }

        public CommandResponse Execute()
        {
            PlayerStatistics stats = _Player.Statistics;

            if (_DataType == DataType.Eloquence) {
                if (stats.Eloquence - _Amount < 0)
                    return new CommandResponse(CommandStatus.Failure, $"Player {_Player.Object.InputAuthority} does not have enough eloquence.");
                _Player.SetEloquence(_Player.Statistics.Eloquence - _Amount);
            } else if (_DataType == DataType.Soul) {
                if (stats.Souls - _Amount < 0)
                    return new CommandResponse(CommandStatus.Failure, $"Player {_Player.Object.InputAuthority} does not have enough souls.");
                _Player.SetSouls(_Player.Statistics.Souls - _Amount);
            }
            return new CommandResponse(CommandStatus.Success, $"Player {_Player.Object.InputAuthority} spend {_Amount} {_DataType}.");
        }

        public void Undo()
        {
            PlayerStatistics stats = _Player.Statistics;

            if (_DataType == DataType.Eloquence)
                _Player.SetEloquence(stats.Eloquence + _Amount);
            else if (_DataType == DataType.Soul)
                _Player.SetSouls(stats.Souls + _Amount);
        }
    }
}
