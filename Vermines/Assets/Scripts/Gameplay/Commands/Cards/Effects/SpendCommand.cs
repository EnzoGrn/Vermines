using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands.Cards.Effects {

    using Vermines.CardSystem.Enumerations;
    using Vermines.Player;

    public class SpendCommand : ICommand {

        private readonly PlayerRef _Player;
        private readonly int       _Amount;
        private readonly DataType  _DataType;

        public SpendCommand(PlayerRef player, int amount, DataType dataType)
        {
            _Player   = player;
            _Amount   = amount;
            _DataType = dataType;
        }

        public CommandResponse Execute()
        {
            if (!GameDataStorage.Instance.PlayerData.TryGet(_Player, out PlayerData playerData))
                return new CommandResponse(CommandStatus.Invalid, $"Player {_Player} does not have any data.");
            if (_DataType == DataType.Eloquence) {
                if (playerData.Eloquence - _Amount < 0)
                    return new CommandResponse(CommandStatus.Failure, $"Player {_Player} does not have enough eloquence.");
                GameDataStorage.Instance.SetEloquence(_Player, playerData.Eloquence - _Amount);
            } else if (_DataType == DataType.Soul) {
                if (playerData.Souls - _Amount < 0)
                    return new CommandResponse(CommandStatus.Failure, $"Player {_Player} does not have enough souls.");
                GameDataStorage.Instance.SetSouls(_Player, playerData.Souls - _Amount);
            }
            return new CommandResponse(CommandStatus.Success, $"Player {_Player} spend {_Amount} {_DataType}.");
        }

        public void Undo()
        {
            if (!GameDataStorage.Instance.PlayerData.TryGet(_Player, out PlayerData playerData))
                return;
            if (_DataType == DataType.Eloquence)
                GameDataStorage.Instance.SetEloquence(_Player, playerData.Eloquence + _Amount);
            else if (_DataType == DataType.Soul)
                GameDataStorage.Instance.SetSouls(_Player, playerData.Souls + _Amount);
        }
    }
}
