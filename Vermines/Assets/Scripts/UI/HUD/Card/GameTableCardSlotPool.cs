using Vermines.UI.Card;

namespace Vermines.UI.GameTable
{
    public class GameTableCardSlotPool : CardSlotPoolBase<TableCardSlot>
    {
        public static new GameTableCardSlotPool Instance => (GameTableCardSlotPool)CardSlotPoolBase<TableCardSlot>.Instance;
    }
}
