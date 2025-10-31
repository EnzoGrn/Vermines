
namespace Vermines.Gameplay.Phases.Data
{
    public struct GainSummaryData
    {
        public int BaseValue;
        public int FollowerValue;
        public int EquipmentValue;
        public int Total => BaseValue + FollowerValue + EquipmentValue;
    }
}