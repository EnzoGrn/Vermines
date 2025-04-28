using OMGG.DesignPattern;

namespace Vermines.ShopSystem.Commands {

    using Vermines.CardSystem.Elements;
    using Vermines.CardSystem.Data;

    public class ReducedInSilenceCommand : ICommand {

        private readonly ICard _CardToBeReduced;

        private int _SoulsReduction = 0;

        public ReducedInSilenceCommand(ICard cardToBeReduced)
        {
            _CardToBeReduced = cardToBeReduced;
        }

        public CommandResponse Execute()
        {
            if (!CardSetDatabase.Instance.CardExist(_CardToBeReduced.ID))
                return new CommandResponse(CommandStatus.Invalid, $"Card {_CardToBeReduced.ID} does not exist.");
            _SoulsReduction = _CardToBeReduced.Data.CurrentSouls;

            _CardToBeReduced.Data.SoulsReduction(_SoulsReduction);
            _CardToBeReduced.Data.ReduceInSilence = true;

            return new CommandResponse(CommandStatus.Success, $"Card {_CardToBeReduced.ID}: {_CardToBeReduced.Data.Name} has been reduced in silence.");
        }

        public void Undo()
        {
            _CardToBeReduced.Data.RemoveSoulsReduction(_SoulsReduction);
            _CardToBeReduced.Data.ReduceInSilence = false;
        }
    }

    public class RemoveReducedInSilenceCommand : ICommand
    {

        private readonly ICard _CardToBeReduced;

        private int _SoulsReduction = 0;

        public RemoveReducedInSilenceCommand(ICard cardToBeReduced, int originalSouls)
        {
            _CardToBeReduced = cardToBeReduced;
            _SoulsReduction = originalSouls;
        }

        public CommandResponse Execute()
        {
            if (!CardSetDatabase.Instance.CardExist(_CardToBeReduced.ID))
                return new CommandResponse(CommandStatus.Invalid, $"Card {_CardToBeReduced.ID} does not exist.");
            _CardToBeReduced.Data.RemoveSoulsReduction(_SoulsReduction);
            _CardToBeReduced.Data.ReduceInSilence = false;

            return new CommandResponse(CommandStatus.Success, $"Card {_CardToBeReduced.ID}: {_CardToBeReduced.Data.Name} is no longer reduced in silence.");
        }

        public void Undo()
        {
            _CardToBeReduced.Data.SoulsReduction(_SoulsReduction);
            _CardToBeReduced.Data.ReduceInSilence = true;
        }
    }
}
