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

        public bool Execute()
        {
            if (!GameDataStorage.Instance.PlayerData.TryGet(_Player, out PlayerData playerData))
                return false;
            if (_DataType == DataType.Eloquence)
                GameDataStorage.Instance.SetEloquence(_Player, playerData.Eloquence + _Amount);
            else if (_DataType == DataType.Soul)
                GameDataStorage.Instance.SetSouls(_Player, playerData.Souls + _Amount);
            return true;
        }

        public void Undo()
        {
            if (!GameDataStorage.Instance.PlayerData.TryGet(_Player, out PlayerData playerData))
                return;
            if (_DataType == DataType.Eloquence)
                GameDataStorage.Instance.SetEloquence(_Player, playerData.Eloquence - _Amount);
            else if (_DataType == DataType.Soul)
                GameDataStorage.Instance.SetSouls(_Player, playerData.Souls - _Amount);
        }
    }
}
