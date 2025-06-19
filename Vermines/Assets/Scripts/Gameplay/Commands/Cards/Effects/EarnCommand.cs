using OMGG.DesignPattern;
using Fusion;

namespace Vermines.Gameplay.Commands.Cards.Effects {
    
    using Vermines.CardSystem.Enumerations;
    using Vermines.Player;

    public class EarnCommand : ICommand {

        private readonly PlayerRef _Player;
        private readonly int       _Amount;
        private readonly DataType  _DataType;

        public EarnCommand(PlayerRef player, int amount, DataType dataType)
        {
            _Player   = player;
            _Amount   = amount;
            _DataType = dataType;
        }

        public CommandResponse Execute()
        {
            if (!GameDataStorage.Instance.PlayerData.TryGet(_Player, out PlayerData playerData))
                return new CommandResponse(CommandStatus.Invalid, $"Player {_Player} does not have any data.");
            if (_DataType == DataType.Eloquence)
                GameDataStorage.Instance.SetEloquence(_Player, playerData.Eloquence + _Amount);
            else if (_DataType == DataType.Soul)
                GameDataStorage.Instance.SetSouls(_Player, playerData.Souls + _Amount);
            return new CommandResponse(CommandStatus.Success, $"Player {_Player} earned {_Amount} {_DataType}.");
        }

        public void Undo()
        {
            if (!GameDataStorage.Instance.PlayerData.TryGet(_Player, out PlayerData playerData))
                return;
            // TODO: Fix that (in case the player have 19 and win 5, it will be at 20, but if we undo he will currently be at 15 and not 19)
            if (_DataType == DataType.Eloquence)
                GameDataStorage.Instance.SetEloquence(_Player, playerData.Eloquence - _Amount);
            else if (_DataType == DataType.Soul)
                GameDataStorage.Instance.SetSouls(_Player, playerData.Souls - _Amount);
        }
    }
}
