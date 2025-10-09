using OMGG.Chronicle;

namespace Vermines.Gameplay.Chronicle {

    public enum VerminesLogsType {
        Unknown = 0,
        BuyCard,
        ChangeCard,
        SacrificeCard,
        CopiedEffect,
        RebornEffect
    }

    public readonly struct VerminesLogEventType : IEventType {

        public readonly VerminesLogsType Type;

        public VerminesLogEventType(VerminesLogsType type)
        {
            Type = type;
        }
    }
}
